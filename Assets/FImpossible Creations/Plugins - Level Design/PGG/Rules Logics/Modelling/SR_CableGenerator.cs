#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_CableGenerator : SR_TileGenerateNodeBase
    {
        public override string TitleName() { return "Cable Mesh Generator"; }
        public override string Tooltip() { return "Node generating cable mesh model objects procedurally"; }

        [Tooltip("It will call generating new cables mesh each time when node is called - it will take much more time and will use more RAM but shape of cables will be fully randomized over whole generated structure")]
        [HideInInspector] public bool RandomizeEachTime = false;

        [PGG_SingleLineSwitch("OffsetMode", 0, "", 140, 4)]
        public Material TargetMaterial;
        [HideInInspector] public ESR_Measuring OffsetMode = ESR_Measuring.Units;
        public List<Vector3> Points = new List<Vector3>();

        // Main Variables
        [Space(4)] public float Radius = 0.02f;
        public float Loose = 0.15f;
        [Range(0f, 1f)] public float Hanging = 0f;

        public TileCableGenerator.CableMeshSettings MeshSettings;
        public TileCableGenerator.CableTexturingSettings TexturingSettings;
        public TileCableGenerator.CableClonerSettings ClonerSettings;
        public TileCableGenerator.CableRandomizationSettings RandomizationSettings;
        public TileCableGenerator.CableAttachementSettings AttachementSettings;


        [SerializeField, HideInInspector] bool _Foldout_Editor = false;
        [SerializeField, HideInInspector] bool _DisplayTopHelpbox = true;

        private Mesh generatedBaseMesh = null;

        private Mesh cablesMesh = null;


        protected override GameObject GenerateTile()
        {
            generatedBaseMesh = TileCableGenerator.GenerateFullCablesMesh(Points, Loose, Hanging, Radius, MeshSettings, TexturingSettings, ClonerSettings, RandomizationSettings, AttachementSettings);

            GameObject generatedTile = new GameObject("Cables_" + generatedBaseMesh.vertexCount);
            generatedTile.transform.position = new Vector3(10000, -10000, 10000);
            generatedTile.hideFlags = HideFlags.HideAndDontSave;

            MeshFilter filt = generatedTile.AddComponent<MeshFilter>();
            filt.sharedMesh = generatedBaseMesh;

            MeshRenderer rend = generatedTile.AddComponent<MeshRenderer>();
            bool attachements = false;
            if (AttachementSettings.Mesh != null) attachements = true;

            if (attachements == false)
            {
                rend.sharedMaterial = GetMaterial();
            }
            else
            {
                Material[] mats = new Material[2];
                mats[0] = GetMaterial();
                mats[1] = AttachementSettings.Material;
                if (mats[1] == null) mats[1] = mats[0];
                rend.sharedMaterials = mats;
            }

            return generatedTile;
        }




        #region Editor GUI

#if UNITY_EDITOR

        SerializedProperty sp_RandomizeEachTime = null;

        /// <summary> Shortcut for SetDirty node </summary>
        protected void D() { EditorUtility.SetDirty(this); }

        int seed = 0;
        public override void NodeBody(SerializedObject so)
        {

            if (_DisplayTopHelpbox)
            {
                EditorGUILayout.HelpBox(" Replacing object to spawn with Cable Design", MessageType.Info);
                Rect r = GUILayoutUtility.GetLastRect();
                if (GUI.Button(r, "", EditorStyles.label)) { _DisplayTopHelpbox = false; }
            }


            if (RandomizationSettings.RandomizeLoose != Vector2.one && RandomizationSettings.RandomizeTrails != Vector2.zero)
            {
                if (sp_RandomizeEachTime == null) sp_RandomizeEachTime = so.FindProperty("RandomizeEachTime");
                EditorGUILayout.PropertyField(sp_RandomizeEachTime);
            }
            else
            {
                if (RandomizeEachTime)
                {
                    RandomizeEachTime = false;
                    D();
                }
            }

            //_EditorDrawReplacePrefabToSpawnToggle(so);

            base.NodeBody(so);

            if (Loose < 0f) { Loose = 0f; D(); }
            if (Radius < 0.001f) { Radius = 0.001f; D(); }
            if (Hanging > 0f && MeshSettings.LengthSubdivs < 8) { MeshSettings.LengthSubdivs = 8; D(); }

            if (ClonerSettings != null)
            {
                if (ClonerSettings.InstancesCount.x < 0) { ClonerSettings.InstancesCount.x = 1; D(); }
                if (ClonerSettings.InstancesCount.y < 0) { ClonerSettings.InstancesCount.y = 1; D(); }
                if (ClonerSettings.InstancesCount.z < 0) { ClonerSettings.InstancesCount.z = 1; D(); }
            }


            GUILayout.Space(3);

            if (Points == null || Points.Count == 0)
            {
                Points.Add(new Vector3(0f, 0f, -0.5f));
                Points.Add(new Vector3(0f, 0f, 0.5f));
                D();
            }

        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            base.NodeFooter(so, mod);

            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();

            bool isRMB = false;
            if (Event.current != null) isRMB = Event.current.button == 1;

            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Generating new cable preview + changing random seed if using randomization"), FGUI_Resources.ButtonStyle, GUILayout.Height(19), GUILayout.Width(22)))
            {
                seed += 1;
                FGenerators.SetSeed(seed);
                //FDebug.StartMeasure();
                cablesMesh = TileCableGenerator.GenerateFullCablesMesh(Points, Loose, Hanging, Radius, MeshSettings, TexturingSettings, ClonerSettings, RandomizationSettings, AttachementSettings);
                //FDebug.EndMeasureAndLog("Mesh Gen Ticks: ");

                #region Generating Mesh file

                if (isRMB)
                {
                    string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Mesh File", "Cable", "asset", "Enter name of file");

                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(cablesMesh, path);
                        AssetDatabase.SaveAssets();
                        var toPing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                        if (toPing) EditorGUIUtility.PingObject(toPing);
                        AssetDatabase.Refresh();
                    }
                }

                #endregion
            }

            if (_Foldout_Editor) GUI.backgroundColor = new Color(0.1f, 1f, 0.1f, 1f);
            //if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_Foldout_Editor, false), EditorStyles.label, GUILayout.Width(20))) { _Foldout_Editor = !_Foldout_Editor; }
            string f = FGUI_Resources.GetFoldSimbol(_Foldout_Editor, false);
            if (GUILayout.Button(f + "        Draw Cable Editor       " + f, FGUI_Resources.ButtonStyle))
            {
                _Foldout_Editor = !_Foldout_Editor;
            }
            //if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_Foldout_Editor, false), EditorStyles.label, GUILayout.Width(20))) { _Foldout_Editor = !_Foldout_Editor; }
            if (_Foldout_Editor) GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            //if (GUILayout.Button(f + "        SCene       " + f, FGUI_Resources.ButtonStyle))
            //{
            //    GameObject prev = new GameObject("Cable Scene Preview");
            //    prev.AddComponent<MeshFilter>().sharedMesh = GetMesh();
            //    prev.AddComponent<MeshRenderer>().sharedMaterial = GetMaterial();
            //}

            if (_Foldout_Editor)
            {

                //if (cablesMesh == null)
                {
                    FGenerators.SetSeed(seed);
                    cablesMesh = TileCableGenerator.GenerateFullCablesMesh(Points, Loose, Hanging, Radius, MeshSettings, TexturingSettings, ClonerSettings, RandomizationSettings, AttachementSettings);
                }

                Rect r = GUILayoutUtility.GetLastRect();
                r.height = 290;
                r.position += new Vector2(0, 28);

                PreviewWindow(r, cablesMesh, GetMaterial(), AttachementSettings.Material);
                GUILayout.Space(300);
            }

            GUILayout.Space(4);
        }


#endif

        #endregion



        Material _defMaterial = null;
        Material GetMaterial()
        {
            if (TargetMaterial != null) return TargetMaterial;
            if (_defMaterial == null) _defMaterial = new Material(Shader.Find("Diffuse"));
            return _defMaterial;
        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CellInfluence(preset, mod, cell, ref spawn, grid, restrictDirection);

            if (RandomizeEachTime)
            {
                Action<GameObject> randomizeMesh =
                (o) =>
                {
                    MeshFilter filt = o.GetComponent<MeshFilter>();
                    filt.sharedMesh = TileCableGenerator.GenerateFullCablesMesh(Points, Loose, Hanging, Radius, MeshSettings, TexturingSettings, ClonerSettings, RandomizationSettings, AttachementSettings);
                };

                spawn.OnGeneratedEvents.Add(randomizeMesh);
            }
        }

    }

}