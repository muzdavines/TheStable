using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Rules.PostEvents
{
    public class SR_FlattenTerrain : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Flatten Terrain Ground"; }
        public override string Tooltip() { return "Detect Unity Terrain below spawned object and adjust terrain ground height to fit to the object's origin\n" + base.Tooltip(); }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [Header("Detecting Terrain Object")]
        public LayerMask GroundRaycastMask = 1 << 0;
        [Tooltip("Most cases it will be 0,-1,0 so straight down")]
        public Vector3 RaycastDirection = Vector3.down;
        [Tooltip("How far collision raycast can go")]
        public float RaycastLength = 7f;
        [Tooltip("Casting ray from upper or lower position of the object")]
        public Vector3 OffsetRaycastOrigin = Vector3.up;

        [Header("Terrain Shaping Parameters")]
        [Range(0f, 1f)]
        [Header("How much terrain should be flattened like opacity")]
        public float FlattenAmount = 1f;
        [Header("How far flatten-falloff should go")]
        public float BrushRadius = 3f;
        [Header("If ground should be leveled with object's origin or with some offset")]
        public Vector3 OffsetGround = Vector3.zero;
        [Header("Spherical falloff for flattening terrain level")]
        public AnimationCurve Falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [Space(4)]
        public bool FlattenOnlyInPlaymode = false;


        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("Beware, you will be most likely NOT ABLE to UNDO modified terrain heights!", MessageType.None);
            base.NodeBody(so);
        }
#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CellInfluence(preset, mod, cell, ref spawn, grid);

            if (FlattenOnlyInPlaymode)
            {
                // Add start-flatten component to spawned object with node's stats
                Action<GameObject> flattenTerrainComponent =
                (o) =>
                {
                    PGGTool_FlattenTerrain flatten = o.AddComponent<PGGTool_FlattenTerrain>();
                    flatten.AllowPostGenerator = false;
                    flatten.FlattenOnGameStart = true;

                    flatten.GroundRaycastMask = GroundRaycastMask;
                    flatten.RaycastDirection = RaycastDirection;
                    flatten.RaycastLength = RaycastLength;
                    flatten.OffsetRaycastOrigin = OffsetRaycastOrigin;
                    flatten.FlattenAmount = FlattenAmount;
                    flatten.BrushRadius = BrushRadius;
                    flatten.OffsetGround = OffsetGround;
                    flatten.Falloff = Falloff;
                };

                spawn.OnGeneratedEvents.Add(flattenTerrainComponent);
            }
            else
            {
                Action<GameObject> flattenTerrain =
                (o) =>
                {
                    DetectTerrainAndFlattenGroundLevel(o, DetectTerrain(o, GroundRaycastMask, RaycastDirection, RaycastLength, OffsetRaycastOrigin), FlattenAmount, BrushRadius, OffsetGround, Falloff);
                };

                spawn.OnGeneratedEvents.Add(flattenTerrain);
            }
        }

        private static RaycastHit[] rays = new RaycastHit[64];

        public static Terrain DetectTerrain(GameObject o, LayerMask groundRaycastMask, Vector3 raycastDirection, float raycastLength, Vector3 offsetRaycastOrigin)
        {
            Terrain terr = null;
            Ray ray = new Ray(o.transform.TransformPoint(offsetRaycastOrigin), raycastDirection.normalized);
            int hitsC = Physics.RaycastNonAlloc(ray, rays, raycastLength, groundRaycastMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitsC; i++)
            {
                if (rays[i].transform == null) continue;
                terr = rays[i].transform.GetComponent<Terrain>();
                if (terr) break;
            }

            return terr;
        }

        public static float[,] DetectTerrainAndFlattenGroundLevel(GameObject o, Terrain terr, float flattenAmount, float brushRadius, Vector3 offsetGround, AnimationCurve falloff)
        {
            float[,] heights = null;

            if (terr)
            {
                int tScale = terr.terrainData.heightmapResolution;

                Vector3 onTerrain = ((o.transform.position + offsetGround) - terr.gameObject.transform.position);
                Vector3 terrLocalPos;
                terrLocalPos.x = onTerrain.x / terr.terrainData.size.x;
                terrLocalPos.y = onTerrain.y / terr.terrainData.size.y;
                terrLocalPos.z = onTerrain.z / terr.terrainData.size.z;

                int posXInTerrain = (int)(terrLocalPos.x * tScale);
                int posYInTerrain = (int)(terrLocalPos.z * tScale);

                heights = terr.terrainData.GetHeights(0, 0, tScale, tScale);
                float[,] newHeights = terr.terrainData.GetHeights(0, 0, tScale, tScale);

                float targetHeight = terrLocalPos.y;
                int radiusInSamples = Mathf.CeilToInt((brushRadius * tScale) / terr.terrainData.size.x);

                for (int x = -radiusInSamples; x <= radiusInSamples; x++)
                    for (int z = -radiusInSamples; z <= radiusInSamples; z++)
                    {
                        int tZ = posXInTerrain + x;
                        int tX = posYInTerrain + z;
                        if (tX < 0 || tZ < 0 || tX >= newHeights.GetLength(0) || tZ >= newHeights.GetLength(1)) continue;

                        float fallf = falloff.Evaluate(Vector2.Distance(Vector2.zero, new Vector2(x, z)) / (float)radiusInSamples);
                        newHeights[tX, tZ] = Mathf.Lerp(newHeights[tX, tZ], targetHeight, fallf * flattenAmount);
                    }

                terr.terrainData.SetHeights(0, 0, newHeights);
            }

            return heights;
        }

        public static void DrawTerrainFlatteningGizmos(GameObject o, LayerMask groundRaycastMask, Vector3 raycastDirection, float raycastLength, Vector3 offsetRaycastOrigin, float flattenAmount, float brushRadius, Vector3 offsetGround, AnimationCurve falloff)
        {
            Vector3 origin = o.transform.TransformPoint(offsetRaycastOrigin);
            Vector3 flattenTo = o.transform.TransformPoint(offsetGround);

#if UNITY_EDITOR
            // Handles are editor only
            Handles.color = new Color(1f, 1f, 1f, 0.85f);
            Handles.CircleHandleCap(0, flattenTo, Quaternion.LookRotation(raycastDirection), brushRadius, EventType.Repaint);
#endif

            Gizmos.color = new Color(1f, 1f, 1f, 0.75f);
            Gizmos.DrawRay(origin, raycastDirection.normalized * raycastLength);
            Gizmos.DrawSphere(origin, raycastLength * 0.03f);
            Gizmos.DrawSphere(origin + raycastDirection.normalized * raycastLength, raycastLength * 0.03f);
            //Gizmos.DrawWireCube(flattenTo, new Vector3(brushRadius * 2f, brushRadius * 0.02f, brushRadius * 2f));
        }

    }
}