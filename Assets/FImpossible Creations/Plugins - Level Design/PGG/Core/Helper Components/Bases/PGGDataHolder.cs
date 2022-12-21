using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    /// <summary>
    /// This is Unity Limitation Walkaround component.
    /// This component is keeping data for PGG components.
    /// Data about grid cells and spawn related info.
    /// </summary>
    public class PGGDataHolder : MonoBehaviour
    {
        public FlexibleGeneratorSetup Setup;
        public MonoBehaviour Owner;
        public List<FlexibleGeneratorSetup> AdditionalSetups = new List<FlexibleGeneratorSetup>();

        internal void Refresh(MonoBehaviour owner)
        {
            Owner = owner;
            if (Setup == null) Setup = new FlexibleGeneratorSetup();
        }
    }


#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(PGGDataHolder))]
    public class PGGDataHolderEditor : UnityEditor.Editor
    {
        public PGGDataHolder Get { get { if (_get == null) _get = (PGGDataHolder)target; return _get; } }
        private PGGDataHolder _get;

        SerializedProperty sp;

        private void OnEnable()
        {
            sp = serializedObject.FindProperty("Owner");
        }

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("   This is Unity Limitation Walkaround component.", UnityEditor.MessageType.Info);
            UnityEditor.EditorGUILayout.HelpBox("This component is keeping data for PGG components.\nData about grid cells and spawn related info.", UnityEditor.MessageType.None);
            UnityEditor.EditorGUILayout.HelpBox("What is it for: Keeping big lists in the component makes editor inspector view laggy, even if lists are not displayed, keeping all in separated game object makes main component inspector view drawn without additional CPU weight.", UnityEditor.MessageType.None);

            GUILayout.Space(3);
            EditorGUILayout.PropertyField(sp);
            GUILayout.Space(3);

            if (Get.Setup == null) return;
            if (Get.Setup.CellsController == null) return;
            if (Get.Setup.InstantiatedInfo == null) return;
            if (Get.Setup.InstantiatedInfo.Instantiated == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Setups in data holder: " + (Get.AdditionalSetups.Count + 1), EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Cells: " + Get.Setup.CellsController.CellsCount);
            EditorGUILayout.LabelField("Waiting To Instantiate: " + Get.Setup.CellsController.ToSpawnCount);
            EditorGUILayout.LabelField("Instantiated: " + Get.Setup.InstantiatedInfo.Instantiated.Count);
            EditorGUILayout.EndVertical();
        }

    }
#endif


}