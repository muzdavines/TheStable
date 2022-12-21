using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("")]
    public class PGG_Minimap_AutoDestroyWith : MonoBehaviour
    {
        public GameObject ToDestroyWhenBeingDestroyed;

        void OnDestroy()
        {
            if (ToDestroyWhenBeingDestroyed == null) return;
            Destroy(ToDestroyWhenBeingDestroyed);
        }
    }

    #region Editor Class
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PGG_Minimap_AutoDestroyWith))]
    public class PGG_Minimap_AutodestroyWithEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("'To Destroy When Being Destroyed' is also reference to the UI obejct.", UnityEditor.MessageType.None);
            UnityEditor.EditorGUIUtility.labelWidth = 220;
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
            UnityEditor.EditorGUIUtility.labelWidth = 0;
        }
    }
#endif
    #endregion

}