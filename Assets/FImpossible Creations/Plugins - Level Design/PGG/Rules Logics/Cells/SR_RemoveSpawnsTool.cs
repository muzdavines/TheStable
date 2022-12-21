#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules.Helpers;

namespace FIMSpace.Generating.Rules.Cells
{
    public class SR_RemoveSpawnsTool : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Remove Spawns Tool"; }
        public override string Tooltip() { return "Removing desired spawn if some conditions are met, multiple conditions can be defined within this single rule"; }

        public List<RemoveInstruction> Removing = new List<RemoveInstruction>();


#if UNITY_EDITOR
        private SerializedProperty sp_list;
        private int selectedElement = 0;
#if UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] void OnReload() { removeDisplayed = 0; } // Just to fix strange Unity error
#endif
        [System.NonSerialized] int removeDisplayed = 0; // Just to fix strange Unity error
        public override bool EditorIsLoading() { return removeDisplayed < 4; }
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (Event.current.type == EventType.Layout) removeDisplayed += 1;
            if (Event.current.type == EventType.Repaint)
            {
                GUIIgnore.Clear(); //
            }

            sp_list = so.FindProperty("Removing");

            if (removeDisplayed > 3)
            {
                if (Event.current.type == EventType.Repaint) GUIIgnore.Add("Removing");
            }
            else if (sp_list != null)
            {
                if (sp_list.isExpanded == false) { sp_list.isExpanded = true; }
                if (removeDisplayed > 1)
                    for (int i = 0; i < sp_list.arraySize; i++) { if (sp_list.GetArrayElementAtIndex(i) == null) continue; sp_list.GetArrayElementAtIndex(i).isExpanded = true; }
            }

            base.NodeFooter(so, mod);

            if (sp_list != null)
            {
                if (sp_list.arraySize == 0 && Removing.Count == 0) Removing.Add(new RemoveInstruction());

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Remove Instructions (" + sp_list.arraySize + ")", GUILayout.Width(154));
                GUILayout.FlexibleSpace();

                if (sp_list.arraySize > 1)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowLeft), GUILayout.Width(20))) selectedElement--;
                    GUILayout.Label((selectedElement + 1) + " / " + (sp_list.arraySize), FGUI_Resources.HeaderStyle, GUILayout.Width(40));
                    if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_ArrowRight), GUILayout.Width(20))) selectedElement++;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(6);

                    if (selectedElement > sp_list.arraySize - 1) selectedElement = 0;
                    if (selectedElement < 0) selectedElement = sp_list.arraySize - 1;
                }

                if (GUILayout.Button("+", GUILayout.Width(20))) Removing.Add(new RemoveInstruction());
                if (sp_list.arraySize > 1) if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        Removing.RemoveAt(selectedElement);
                        if (selectedElement > Removing.Count - 1) selectedElement = 0;
                        if (selectedElement < 0) selectedElement = Removing.Count - 1;
                        return;
                    }

                GUILayout.EndHorizontal();
                if (selectedElement >= sp_list.arraySize) selectedElement = 0;

                if (removeDisplayed > 6)
                    if (selectedElement < sp_list.arraySize)
                    {
                        SerializedProperty sp_r = sp_list.GetArrayElementAtIndex(selectedElement);
                        GUILayout.Space(4);
                        RemoveInstruction.DrawGUI(sp_r, Removing[selectedElement]);
                    }

            }
            else
            {
                UnityEngine.Debug.Log("Cant find prop Removing ");
            }

            so.ApplyModifiedProperties();
        }
#endif


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            for (int i = 0; i < Removing.Count; i++)
            {
                Removing[i].ProceedRemoving(OwnerSpawner, ref thisSpawn, cell, grid);
            }
        }


    }
}