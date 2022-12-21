#if UNITY_EDITOR

using FIMSpace.Generating.Rules;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    public static class PGGInspectorUtilities
    {

        public static Texture2D _Tex_FieldIcon { get { if (_tex_FieldIcon == null) { _tex_FieldIcon = Resources.Load<Texture2D>("SPR_FieldDesigner"); } return _tex_FieldIcon; } }

        private static Texture2D _tex_FieldIcon;
        public static void DrawFieldSetupInheritToggle(ref bool toggle, string tooltip = "Use FieldSetup parameter to modify target rule value")
        {
            EditorGUIUtility.labelWidth = 18;
            toggle = EditorGUILayout.Toggle(new GUIContent(_Tex_FieldIcon, tooltip), toggle, GUILayout.Height(20), GUILayout.Width(38));
            EditorGUIUtility.labelWidth = 0;
        }
        public static void DrawFieldSetupInheritToggle(SerializedProperty sp, string tooltip = "Use FieldSetup parameter to modify target rule value")
        {
            EditorGUIUtility.labelWidth = 18;
            sp.boolValue = EditorGUILayout.Toggle(new GUIContent(_Tex_FieldIcon, tooltip), sp.boolValue, GUILayout.Height(20), GUILayout.Width(38));
            EditorGUIUtility.labelWidth = 0;
        }

        public static void DrawDetailsSwitcher(ref ESR_Details details, int width = 68, string tooltip = "Select if you want to use Tags, SpawnStigma or CellData")
        {
            EditorGUIUtility.labelWidth = 8;
            details = (ESR_Details)EditorGUILayout.EnumPopup(new GUIContent(" ", tooltip), details, GUILayout.Height(20), GUILayout.Width(width));
            EditorGUIUtility.labelWidth = 0;
        }

        const string menuItemLogPGGWarnings = "Window/FImpossible Creations/Level Design/Log PGG Warnings";
        public static bool LogPGGWarnings = false;

#if UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void SimulationModeRefreshOnLoad()
        { ToggleSimulationModeValidate(); }
#endif

        [MenuItem(menuItemLogPGGWarnings, false, 100000)]
        public static void ToggleSimulationMode()
        {  TogglePGGWarningLogs = !TogglePGGWarningLogs; }

        [MenuItem(menuItemLogPGGWarnings, true, 100000)]
        public static bool ToggleSimulationModeValidate()
        { LogPGGWarnings = EditorPrefs.GetBool("PGGWarnings", false); UnityEditor.Menu.SetChecked(menuItemLogPGGWarnings, TogglePGGWarningLogs); return true; }

        public static bool TogglePGGWarningLogs
        {
            get
            {
                if (LogPGGWarnings == false) LogPGGWarnings = EditorPrefs.GetBool("PGGWarnings", false);
                return LogPGGWarnings;
            }
            set
            {
                if (value != LogPGGWarnings)
                {
                    LogPGGWarnings = value;
                    EditorPrefs.SetBool("PGGWarnings", value);
                }
            }
        }

    }
}

#endif