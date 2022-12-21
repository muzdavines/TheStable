using FIMSpace.Generating.Checker;
using UnityEngine;

namespace FIMSpace.Generating.Planning.GeneratingLogics
{
    /// <summary>
    /// It's always sub-asset -> it's never project file asset
    /// </summary>
    public abstract partial class ShapeGeneratorBase : ScriptableObject
    {
        public virtual string TitleName() { return GetType().Name; }
        public virtual string Tooltip() { string tooltipHelp = "(" + GetType().Name; return tooltipHelp + ")"; }
        public abstract CheckerField3D GetChecker(FieldPlanner planner);

        protected void RefreshPreview(CheckerField3D checker)
        {
            if (FieldPlanner.CurrentGraphPreparingPlanner != null)
                FieldPlanner.CurrentGraphPreparingPlanner.RefreshPreviewWith(checker);
        }

#if UNITY_EDITOR
        [HideInInspector] public bool _editorForceChanged = false;
        //private bool wasGUIInitialized = false;

        /// <summary> Base is empty </summary>
        public virtual void OnGUIModify()
        {

        }

        /// <summary>
        /// Returns true if something changed in GUI - using EditorGUI.BeginChangeCheck();
        /// </summary>
        public virtual void DrawGUI(UnityEditor.SerializedObject so)
        {
            try
            {
                if (so == null) return;
                if (so.targetObject == null) { so.Dispose(); return; }
            }
            catch (System.Exception)
            {
                GUILayout.Label("Something is loading...");
                return;
            }

            try
            {
                so.Update();

                UnityEditor.SerializedProperty sp = so.GetIterator();
                sp.Next(true);
                sp.NextVisible(false);
                bool can = sp.NextVisible(false);

                if (can)
                {
                    do
                    {
                        bool cont = false;
                        if (cont) continue;

                        UnityEditor.EditorGUILayout.PropertyField(sp);
                    }
                    while (sp.NextVisible(false) == true);
                }

            }
            catch (System.Exception e)
            {
                if (e.HResult == -2147024809) return;
                UnityEngine.Debug.LogException(e);
            }


        }
#endif

    }
}
