using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace FIMSpace.Generating
{
    public class PGGNavMeshTest : MonoBehaviour
    {
        public PGGGeneratorBase Generator;
        [Tooltip("If you want navmesh to be shrinked to inside or expanded outside grid cells (too small cells scale can result in empty navmesh!)")]
        public float ScaleCells = 1f;
        public Vector3 NavBounds = new Vector3(200, 40, 200);
        [Header("Optional")]
        public string OnFieldModificatorNames = "";
        [Tooltip("If settings not found then used 'CreateSettings'")]
        public int AgentSettingsIndex = 0;

        [Tooltip("Use only unity static mesh check baking")]
        public bool UseJustUnityBake = false;

        public void GenerateNavMesh()
        {
            if (Generator == null) Generator = GetComponentInChildren<PGGGeneratorBase>();

            NavMesh.RemoveAllNavMeshData();

            NavMeshBuildSettings set;
            if (AgentSettingsIndex < 0 || AgentSettingsIndex >= NavMesh.GetSettingsCount())
                set = NavMesh.CreateSettings();
            else
                set = NavMesh.GetSettingsByIndex(AgentSettingsIndex);

            if (UseJustUnityBake)
            {
#if UNITY_EDITOR
                UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
                UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
#else
                UnityEngine.Debug.Log("[PGG Nav Mesh Bake] Unity Editor Nav Mesh generating is not working on builds!");
                UnityEngine.Debug.LogWarning("[PGG Nav Mesh Bake] Unity Editor Nav Mesh generating is not working on builds!");
#endif

                return;
            }

            List<NavMeshBuildSource> p = new List<NavMeshBuildSource>();

            Vector3 scale = new Vector3(2f, 1f, 2f);
            FieldSetup field = null;

            if (string.IsNullOrEmpty(OnFieldModificatorNames) == false)
            {
                List<Transform> floors = new List<Transform>();

                // In my case I putted all floors in separate FieldModificator called "Floor" so I finding all containers of this objects
                // For corridor and rooms
                for (int g = 0; g < Generator.Generated.Count; g++)
                {
                    var generated = Generator.Generated[g];
                    if (field == null) field = generated.InternalField;
                    if (field == null) { PGGPlanGeneratorBase planGen = Generator as PGGPlanGeneratorBase; if (planGen) field = planGen.BuildPlanPreset.CorridorSetup.FieldSetup; }

                    if (field) scale = field.GetCellUnitSize();

                    for (int c = 0; c < Generator.Generated[g].FieldTransform.transform.childCount; c++)
                    {
                        Transform childContainer = Generator.Generated[g].FieldTransform.transform.GetChild(c);
                        if (childContainer.name.Contains(OnFieldModificatorNames))
                        {
                            floors.Add(childContainer);
                        }
                    }
                }

                // Using all floor objects child game objects to generate nav mesh area
                for (int f = 0; f < floors.Count; f++)
                {
                    foreach (Transform tr in floors[f])
                    {
                        NavMeshBuildSource floor = new NavMeshBuildSource();
                        floor.transform = tr.localToWorldMatrix;
                        floor.shape = NavMeshBuildSourceShape.Box;
                        floor.size = new Vector3(scale.x * ScaleCells, scale.y * 0.1f, scale.z * ScaleCells); // Your game specific size
                        p.Add(floor);
                    }
                }
            }
            else
            {

                for (int g = 0; g < Generator.Generated.Count; g++)
                {
                    var generated = Generator.Generated[g];

                    if (field == null) field = generated.InternalField;
                    if (field == null) { PGGPlanGeneratorBase planGen = Generator as PGGPlanGeneratorBase; if (planGen) field = planGen.BuildPlanPreset.CorridorSetup.FieldSetup; }
                    scale = field.GetCellUnitSize();

                    for (int c = 0; c < generated.Grid.AllApprovedCells.Count; c++)
                    {
                        var grid = generated.Grid;
                        NavMeshBuildSource floor = new NavMeshBuildSource();
                        floor.transform = Matrix4x4.TRS(Generator.transform.position + grid.AllApprovedCells[c].WorldPos(generated.InternalField), Quaternion.identity, Vector3.one);
                        floor.shape = NavMeshBuildSourceShape.Box;
                        floor.size = new Vector3(scale.x * ScaleCells, scale.y * 0.1f, scale.z * ScaleCells); // Your game specific size
                        p.Add(floor);
                    }
                }
            }

            // 200,40,200 bounds is enough for my case
            NavMeshData built = NavMeshBuilder.BuildNavMeshData(set, p, new Bounds(Vector3.zero, NavBounds), Generator.transform.position, Generator.transform.rotation);
            NavMesh.AddNavMeshData(built);
        }
    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGGNavMeshTest))]
    public class PGGNavMeshTestEditor : UnityEditor.Editor
    {
        public PGGNavMeshTest Get { get { if (_get == null) _get = (PGGNavMeshTest)target; return _get; } }
        private PGGNavMeshTest _get;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("Navmesh generating will be improved in next updates", UnityEditor.MessageType.Info);
            UnityEditor.EditorGUILayout.HelpBox("1. Use 'Event After Generating' to call GenerateNavMesh from this component", UnityEditor.MessageType.None);
            UnityEditor.EditorGUILayout.HelpBox("2. Leave 'OnFieldModificatorNames' empty to use just grid cells to generate nav-mesh", UnityEditor.MessageType.None);
            UnityEditor.EditorGUILayout.HelpBox("2. If you want to generate navmesh only on selective places, your field modificator with floor spawner should be named like 'OnFieldModificatorNames' (for exmaple 'Floor') parameter in the component, to use it's cells to generate navmesh", UnityEditor.MessageType.None);

            DrawDefaultInspector();
            GUILayout.Space(4f);

            if (Get.UseJustUnityBake)
            {
                UnityEditor.EditorGUILayout.HelpBox("UseJustUnityBake works only in the Editor, not in build, you can inmplement custom methods in the place in code, using some tutorials examples on the internet", UnityEditor.MessageType.Info);
            }
        }
    }
#endif

}