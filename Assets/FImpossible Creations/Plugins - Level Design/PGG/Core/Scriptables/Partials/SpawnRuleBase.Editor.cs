using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public abstract partial class SpawnRuleBase
    {

#if UNITY_EDITOR
        public virtual void NodeFooter(SerializedObject so, FieldModification mod) { }
        public virtual void NodeBody(SerializedObject so) { }
        public virtual bool EditorDrawPublicProperties() { return true; }
        public virtual bool EditorActiveHeader() { return false; }

        [System.NonSerialized] public SerializedObject _latestSO = null;

        /// <summary>
        /// Needs to toggle '_EditorDebug = true' to work
        /// </summary>
        public virtual void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid) { _DbPreCol = GUI.color; }
#endif

        string _in_head = "";

        public virtual void NodeHeader()
        {
#if UNITY_EDITOR
            string foldout = FGUI_Resources.GetFoldSimbol(_editor_drawRule);
            string tip = Tooltip();
            //if (this is ISpawnProcedureType) tip = ((ISpawnProcedureType)(this)).Type.ToString();
            //tip += "\n" + Tooltip();

            string head = "";

            if (EditorIsLoading()) head = "  Unity is Reloading... (Click Somewhere)  ";
            else
            {
                if (string.IsNullOrEmpty(_in_head) || EditorActiveHeader()) _in_head = "  " + TitleName().Replace("SR_", "") + "  ";
                head = _in_head;
            }

            if (GUILayout.Button(new GUIContent(foldout + head + foldout, tip), FGUI_Resources.HeaderStyle))
            {

                bool rmb = false;

                if (AllowDuplicate())
                    if (FGenerators.IsRightMouseButton())
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Duplicate Rule"), false, () => { OwnerSpawner.DuplicateRule(this); });
                        menu.AddItem(new GUIContent(""), false, () => { });
                        if (PGGUtils.CopyProperties_FindTypeInClipboard(this)) menu.AddItem(new GUIContent("Paste Properties (WIP: check fields after paste)"), false, () => { PGGUtils.CopyProperties_PasteTo(this, false); });
                        if (PGGUtils.CopyProperties_FindTypeInClipboard(this)) menu.AddItem(new GUIContent("Paste All Properties (WIP: Force All)"), false, () => { PGGUtils.CopyProperties_PasteTo(this, true); });
                        menu.AddItem(new GUIContent("Copy All Properties"), false, () => { PGGUtils.CopyProperties(this); });
                        FGenerators.DropDownMenu(menu);
                        rmb = true;
                    }

                if (rmb == false)
                    _editor_drawRule = !_editor_drawRule;

            }
#endif
        }


#if UNITY_EDITOR

        public static bool DrawMultiCellSelector(List<Vector3Int> cells, FieldSpawner spawner, bool yellowIfZero = false)
        {
            Color bc = GUI.backgroundColor;
            if (cells != null) if (cells.Count > 1) { GUI.backgroundColor = new Color(0.61f, 0.61f, 1f, 1f); } else if ( yellowIfZero) if (cells.Count == 0) { GUI.backgroundColor = new Color(1f, 1f, 0.6f, 1f); GUI.color = GUI.backgroundColor; }
            if (GUILayout.Button(new GUIContent(PGGUtils.Tex_Selector, "Open window for advanced neightbour cells selection - node rule will be checked on neightbour cells"), FGUI_Resources.ButtonStyle, GUILayout.Width(19), GUILayout.Height(17))) { CheckCellsSelectorWindow.Init(cells, spawner); return true; }
            if (cells != null) if (cells.Count > 1 || cells.Count == 0) { GUI.backgroundColor = bc; GUI.color = GUI.backgroundColor; }
            return false;
        }

        public static bool DrawMultiCellSelector(CheckCellsSelectorSetup setup, FieldSpawner spawner, bool yellowIfZero = false)
        {
            if (setup == null) setup = new CheckCellsSelectorSetup();
            Color bc = GUI.backgroundColor;
            if (setup.ToCheck != null) if (setup.ToCheck.Count > 1) { GUI.backgroundColor = new Color(0.61f, 0.61f, 1f, 1f); } else if (yellowIfZero) if (setup.ToCheck.Count == 0) { GUI.backgroundColor = new Color(1f, 1f, 0.6f, 1f); GUI.color = GUI.backgroundColor; }
            if (GUILayout.Button(new GUIContent(PGGUtils.Tex_Selector, "Open window for advanced neightbour cells selection - node rule will be checked on neightbour cells"), FGUI_Resources.ButtonStyle, GUILayout.Width(19), GUILayout.Height(17))) { CheckCellsSelectorWindow.Init(null, spawner, setup); return true; }
            if (setup.ToCheck != null) if (setup.ToCheck.Count > 1 || setup.ToCheck.Count == 0) { GUI.backgroundColor = bc; GUI.color = GUI.backgroundColor; }
            return false;
        }

#endif


    }
}