#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/Level Design/Pose Stamp", 3)]
    public class PoseStamp : MonoBehaviour, IGenerating
    {
        public Transform ToMove;

        [HideInInspector]
        public List<Coords> Coordinates = new List<Coords>();

#if UNITY_EDITOR
        [HideInInspector] public bool _EditorShow = true;
#endif


        [Space(4)]
        [Range(0f, 1f)]
        [HideInInspector] public float RandomizePosition = 0f;
        [HideInInspector] public Vector3 RandomPosition = new Vector3(1f, 0f, 1f);
        [Range(0f, 1f)]
        [HideInInspector] public float RandomizeRotation = 0f;
        [HideInInspector] public Vector3 RandomRotation = new Vector3(45f, 25f, 0f);
        [Space(4)]
        public bool ResetScale = false;
        [Space(7)]
        [Range(0f, 1f)]
        [HideInInspector] public float GizmosSize = 0.3f;

        [System.Serializable]
        public class Coords
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        void Reset()
        {
            ToMove = transform;
        }


        public void RandomizeCoords()
        {
            if (Coordinates.Count <= 0) return;
            Coords c = Coordinates[FGenerators.GetRandom(0, Coordinates.Count)];
            if (FGenerators.CheckIfIsNull(c)) return;

            ToMove.position = transform.TransformPoint(c.position);

            if (RandomizePosition > 0f)
            {
                Vector3 rOffset = new Vector3();
                rOffset.x = FGenerators.GetRandom(-RandomPosition.x, RandomPosition.x);
                rOffset.y = FGenerators.GetRandom(-RandomPosition.y, RandomPosition.y);
                rOffset.z = FGenerators.GetRandom(-RandomPosition.z, RandomPosition.z);
                ToMove.position += Quaternion.Euler(transform.rotation.eulerAngles + c.rotation) * (rOffset * RandomizePosition);
            }

            Vector3 rotOff = c.rotation;
            if (RandomizeRotation > 0f)
            {
                Vector3 rOffset = new Vector3();
                rOffset.x = FGenerators.GetRandom(-RandomRotation.x, RandomRotation.x);
                rOffset.y = FGenerators.GetRandom(-RandomRotation.y, RandomRotation.y);
                rOffset.z = FGenerators.GetRandom(-RandomRotation.z, RandomRotation.z);

                rotOff += rOffset * RandomizeRotation;
            }

            ToMove.rotation = FEngineering.QToWorld(transform.rotation, Quaternion.Euler(rotOff));
            if (ResetScale) ToMove.localScale = new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1f / transform.lossyScale.z);
        }

        public void Generate()
        {
            RandomizeCoords();
        }

        public void PreviewGenerate()
        {
            RandomizeCoords();
        }

        public void IG_CallAfterGenerated() { }


        public void AddNewCoord()
        {
            Coords c = new Coords();

            if (ToMove)
            {
                c.position = transform.InverseTransformPoint(ToMove.position);
                c.rotation = FEngineering.QToLocal(transform.rotation, ToMove.rotation).eulerAngles;
            }
            else
            {
                c.position = Vector3.zero;
                c.rotation = Vector3.zero;
            }

            Coordinates.Add(c);
        }

    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PoseStamp))]
    public class PoseStampEditor : UnityEditor.Editor
    {
        public PoseStamp Get { get { if (_get == null) _get = (PoseStamp)target; return _get; } }
        private PoseStamp _get;

        private SerializedProperty sp_Coordinates;
        private SerializedProperty sp_RandomizePosition;

        bool cast = false;

        public override void OnInspectorGUI()
        {
            if (sp_Coordinates == null) sp_Coordinates = serializedObject.FindProperty("Coordinates");
            if (sp_RandomizePosition == null) sp_RandomizePosition = serializedObject.FindProperty("RandomizePosition");

            UnityEditor.EditorGUILayout.HelpBox("  Simple component to randomize object position when game starts!", UnityEditor.MessageType.Info);

            serializedObject.Update();
            Color bgc = GUI.backgroundColor;

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            GUILayout.Space(4f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            EditorGUILayout.BeginHorizontal();
            FGUI_Inspector.FoldHeaderStart(ref Get._EditorShow, "Random Coords to choose from", null);
            if (cast) GUI.backgroundColor = Color.green;
            if (GUILayout.Button("O", GUILayout.Width(22))) { cast = true; }
            GUI.backgroundColor = bgc;
            if (GUILayout.Button("+", GUILayout.Width(22))) { Get.AddNewCoord(); /*serializedObject.ApplyModifiedProperties(); serializedObject.Update();*/ EditorUtility.SetDirty(Get); }
            EditorGUILayout.EndHorizontal();

            if (Get._EditorShow)
            {
                if (Get.Coordinates.Count == 0)
                {
                    EditorGUILayout.HelpBox("Hit '+' button to add new position/rotation, adjust target coords through scene view", MessageType.None);
                }
                else
                {
                    for (int i = 0; i < sp_Coordinates.arraySize; i++)
                    {
                        SerializedProperty sp = sp_Coordinates.GetArrayElementAtIndex(i);
                        if (sp != null)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("[" + i + "]", GUILayout.Width(20));
                            EditorGUILayout.PropertyField(sp.FindPropertyRelative("position"));
                            if (GUILayout.Button("X", GUILayout.Width(22))) { Get.Coordinates.RemoveAt(i); EditorUtility.SetDirty(Get); return; }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(sp.FindPropertyRelative("rotation"));
                        }
                        FGUI_Inspector.DrawUILine(0.5f, 0.3f, 1, 4);
                    }
                }
            }

            EditorGUILayout.EndVertical();

            if (Get.Coordinates.Count > 0)
            {
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(sp_RandomizePosition);
                SerializedProperty spp = sp_RandomizePosition.Copy();
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
                spp.Next(false); EditorGUILayout.PropertyField(spp);
            }

            serializedObject.ApplyModifiedProperties();

            if (Get.Coordinates.Count > 0)
                if (GUILayout.Button("Test Randomize Coordinates"))
                {
                    Get.RandomizeCoords();
                }
        }

        private void OnSceneGUI()
        {
            if (Selection.activeGameObject != Get.gameObject)
            {
                return;
            }

            Color preH = Handles.color;
            Handles.matrix = Get.transform.localToWorldMatrix;


            for (int i = 0; i < Get.Coordinates.Count; i++)
            {
                var c = Get.Coordinates[i];
                Quaternion r = Quaternion.Euler(c.rotation);
                c.position = FEditor_TransformHandles.PositionHandle(c.position, r, 0.75f * Get.GizmosSize, false, false);
                c.rotation = FEditor_TransformHandles.RotationHandle(r, c.position, 0.875f * Get.GizmosSize, false).eulerAngles;
            }


            Handles.matrix = Matrix4x4.identity;

            if (cast)
            {
                MeshCollider m = Get.GetComponentInChildren<MeshCollider>();
                MeshRenderer rr = Get.GetComponentInChildren<MeshRenderer>();

                if (rr || m)
                {
                    if (m == null)
                        m = rr.gameObject.AddComponent<MeshCollider>();

                    RaycastHit hit = GetCameraRayMeshHit(m);
                    if (hit.transform)
                    {
                        Handles.DrawWireDisc(hit.point, hit.normal, 0.2f);

                        if (Event.current.type == EventType.MouseDown)
                        {
                            PoseStamp.Coords c = new PoseStamp.Coords();
                            c.position = Get.transform.InverseTransformPoint(hit.point);
                            c.rotation = FEngineering.QToLocal(Get.transform.rotation, Quaternion.FromToRotation(Vector3.up, hit.normal)).eulerAngles;
                            Get.Coordinates.Add(c);
                            cast = false;
                        }
                    }

                    GameObject.DestroyImmediate(m);
                }


            }


            Handles.color = preH;
        }




        RaycastHit GetCameraRayMeshHit(MeshCollider collider)
        {
            RaycastHit hit = new RaycastHit();
            if (Event.current == null) return hit;
            if (SceneView.currentDrawingSceneView.camera == null) return hit;

            Camera sceneCam = SceneView.currentDrawingSceneView.camera;
            Vector3 mousePosition = Event.current.mousePosition;
            mousePosition.y = sceneCam.pixelHeight - mousePosition.y;

            Ray ray = sceneCam.ScreenPointToRay(mousePosition);

            collider.Raycast(ray, out hit, Mathf.Infinity);

            // Shere radius cast
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == collider.transform)
                    return hits[i];
            }

            return hit;
        }

    }
#endif

}