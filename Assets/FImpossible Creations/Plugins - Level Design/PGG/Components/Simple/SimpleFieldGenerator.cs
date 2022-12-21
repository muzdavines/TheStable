#if UNITY_EDITOR
using FIMSpace.FEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Simple Field Generator", 101)]
    public class SimpleFieldGenerator : MonoBehaviour
    {
        public bool GenrateOnGameStart = false;
        public bool RandomSeed = true;
        public int Seed = 0;

        [Space(3)]
        public FieldSetup FieldPreset;
        public Vector3Int FieldSizeInCells = new Vector3Int(5, 0, 4);
        public bool CenterOrigin = false;

        [SerializeField] [HideInInspector] public InstantiatedFieldInfo Generated;
        [HideInInspector] public UnityEvent RunAfterGenerating;

        private void Start()
        {
            if (GenrateOnGameStart)
            {
                Generate();
            }
        }

        public void Generate()
        {
            Generate(null);
        }

        public void Generate(List<SpawnInstruction> guides)
        {
            if (RandomSeed) Seed = FGenerators.GetRandom(-99999, 99999);
            ClearGenerated();

            if (FieldPreset == null) return;

            Vector3Int origin = Vector3Int.zero;
            if (CenterOrigin) origin = new Vector3Int(-FieldSizeInCells.x / 2, 0, -FieldSizeInCells.z / 2);
            Generated = IGeneration.GenerateFieldObjectsRectangleGrid(FieldPreset, FieldSizeInCells, Seed, transform, true, guides, true, origin);
            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
        }

        public void ClearGenerated()
        {
            if (Generated != null)
                if (Generated.Instantiated != null)
                {
                    for (int i = 0; i < Generated.Instantiated.Count; i++)
                        if (Generated.Instantiated[i] != null)
                            FGenerators.DestroyObject(Generated.Instantiated[i]);
                }

        }


        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (FieldPreset == null) return;

            Color preColor = GUI.color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            Vector3 origin = Vector3.zero;
            if (CenterOrigin) origin = new Vector3(-FieldSizeInCells.x / 2f, 0, -FieldSizeInCells.z / 2f);
            Vector3 presetSize = FieldPreset.GetCellUnitSize();

            for (int x = 0; x < FieldSizeInCells.x; x++)
            {
                for (int y = 0; y <= FieldSizeInCells.y; y++)
                    for (int z = 0; z < FieldSizeInCells.z; z++)
                    {
                        Vector3 genPosition = Vector3.Scale(presetSize, new Vector3(x, y, z)) + origin * presetSize.x;
                        Gizmos.DrawWireCube(genPosition, new Vector3(presetSize.x, presetSize.x * 0.2f, presetSize.x));
                    }
            }

            Gizmos.color = preColor;
            Gizmos.matrix = Matrix4x4.identity;
        }

        #endregion

    }

    #region Drawing 'Generate' and 'Clear' buttons inside inspector window


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(SimpleFieldGenerator))]
    public class ExampleSimpleFieldGeneratorEditor : UnityEditor.Editor
    {
        public SimpleFieldGenerator Get { get { if (_get == null) _get = (SimpleFieldGenerator)target; return _get; } }
        private SimpleFieldGenerator _get;
        bool displayEvent = false;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("This component is just simple generator for choosed 'Field Setup', you should use 'GRID PAINTER' for more customized generation!", UnityEditor.MessageType.Info);

            FGUI_Inspector.LastGameObjectSelected = Get.gameObject;

            DrawDefaultInspector();

            GUILayout.Space(4);
            if (GUILayout.Button("Generate")) Get.Generate();
            if (Get.Generated.Instantiated != null) if (Get.Generated.Instantiated.Count > 0) if (GUILayout.Button("Clear Generated")) Get.ClearGenerated();


            displayEvent = UnityEditor.EditorGUILayout.Foldout(displayEvent, "Event After Generating", true);
            if (displayEvent) UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RunAfterGenerating"));
        }
    }
#endif


    #endregion

}