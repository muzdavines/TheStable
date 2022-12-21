using UnityEngine;

namespace FIMSpace.Generating
{

    /// <summary>
    /// Keeping information about generated build planner instance
    /// </summary>
    [AddComponentMenu("")]
    public class BuildPlannerReference : MonoBehaviour
    {
        public BuildPlannerExecutor ParentExecutor;
        public PGGGeneratorRoot Generator;
        public Planning.FieldPlanner Planner;
        public string PlannerName;
        public int BuildPlannerIndex;
        public int BuildPlannerInstanceID;
        public Bounds GridSpaceBounds;


        public Bounds WorldSpaceBounds { get { return PGG_MinimapUtilities.TransformBounding(GridSpaceBounds, transform.localToWorldMatrix); } }
        public Vector3 GetWorldBoundsCenter { get { return transform.TransformPoint(GridSpaceBounds.center); } }


        public FieldSetup BaseSetup { get { return Generator.PGG_Setup; } }
        public FGenGraph<FieldCell, FGenPoint> Grid { get { return Generator.PGG_Grid; } }


        [Space(5)]
        public bool DrawBoundsGizmos = false;


        #region Editor Gizmos Code
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!DrawBoundsGizmos) return;

            Matrix4x4 preMx = Gizmos.matrix;

            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(GridSpaceBounds.center, GridSpaceBounds.size);

            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.04f);
            Gizmos.DrawCube(GridSpaceBounds.center, GridSpaceBounds.size);

            Gizmos.matrix = preMx;
            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.25f);
            Gizmos.DrawRay(GetWorldBoundsCenter, Vector3.up * 4f);
        }
#endif
        #endregion


    }


    #region Editor Class

#if UNITY_EDITOR

    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BuildPlannerReference))]
    public class BuildPlannerReferenceEditor : UnityEditor.Editor
    {
        public BuildPlannerReference Get { get { if (_get == null) _get = (BuildPlannerReference)target; return _get; } }
        private BuildPlannerReference _get;

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("Keeping information about generated build planner instance. It can be helpful for custom minimap coloring - naming.", UnityEditor.MessageType.Info);

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion


}