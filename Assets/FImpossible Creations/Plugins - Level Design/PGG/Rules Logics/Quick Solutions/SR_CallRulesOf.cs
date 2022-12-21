#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_CallRulesOf : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Call Rules Logics Of"; }
        public override string Tooltip() { return "You can use logics of some other Modificator/Spawner without defining them multiple times\n" + base.Tooltip(); }
        public EProcedureType Type { get { return EProcedureType.Coded; } }
        //public override bool CanBeGlobal() { return false; }
        //public override bool CanBeNegated() { return false; }

        [HideInInspector] public FieldModification CallFrom;
        [HideInInspector] public SR_ModGraph CallModGraph;
        [HideInInspector] public int SpawnerId = 0;
        public enum EResult
        { JustTriggerEvents, AllowOrBreakSpawning }
        [Space(3)]
        [Tooltip("If you want use this logic block to trigger some event or stop spawning parent spawner")]
        public EResult CallResult = EResult.AllowOrBreakSpawning;

        [HideInInspector]
        public List<SpawnRuleBase> IgnoreToCall = new List<SpawnRuleBase>();
        bool internalAllow = true;

        #region Drawing Inspector Window

#if UNITY_EDITOR

        private SerializedProperty sp_CallFrom = null;
        private int[] spawnersIDs = new int[0];
        private string[] spawnersNames = new string[0];
        private bool drawTargetLogics = false;
        public override void NodeBody(SerializedObject so)
        {
            //EditorGUILayout.HelpBox("WARNING: Work In Progress Node, use it wisely.", MessageType.None);

            if (sp_CallFrom == null) sp_CallFrom = so.FindProperty("CallFrom");
            
            if (sp_CallFrom != null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(sp_CallFrom);

                if (EditorGUI.EndChangeCheck())
                {
                    spawnersIDs = new int[0];
                    if (sp_CallFrom.objectReferenceValue != null)
                    {
                        FieldModification mod = sp_CallFrom.objectReferenceValue as FieldModification;
                        CallFrom = mod;
                    }
                }

                if (CallFrom != null)
                {
                    if (CallFrom.Spawners.Count > 0)
                    {
                        if (spawnersIDs.Length == 0)
                        {
                            spawnersIDs = new int[CallFrom.Spawners.Count];
                            spawnersNames = new string[spawnersIDs.Length];
                            IgnoreToCall = new List<SpawnRuleBase>();

                            for (int i = 0; i < spawnersIDs.Length; i++)
                            {
                                if (CallFrom.Spawners[i] == null) continue;
                                spawnersIDs[i] = i;
                                spawnersNames[i] = CallFrom.Spawners[i].Name;
                            }
                        }

                        if (spawnersIDs.Length > 0) SpawnerId = EditorGUILayout.IntPopup(SpawnerId, spawnersNames, spawnersIDs, GUILayout.Width(70));
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No Spawners in Field Modificator!", MessageType.None);
                    }
                }

                EditorGUILayout.EndHorizontal();

                SerializedProperty sp_mg = sp_CallFrom.Copy(); sp_mg.Next(false);
                EditorGUILayout.PropertyField(sp_mg);
            }

            base.NodeBody(so);

            if (CallFrom != null)
            {

                Color bc = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0f, 1f, 0f, 0.75f);
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);
                GUI.backgroundColor = bc;

                if (SpawnerId < CallFrom.Spawners.Count)
                {
                    GUILayout.Space(-3);
                    EditorGUILayout.LabelField("To CALL : " + CallFrom.name, FGUI_Resources.HeaderStyle);
                    GUILayout.Space(3);

                    FieldSpawner spawner = CallFrom.Spawners[SpawnerId];
                    if (spawner != null)
                    {

                        for (int i = 0; i < spawner.Rules.Count; i++)
                        {
                            var rule = spawner.Rules[i];
                            EditorGUILayout.BeginHorizontal();

                            bool selected = !IgnoreToCall.Contains(rule);
                            bool pre = selected;

                            selected = EditorGUILayout.Toggle(selected, GUILayout.Width(18));

                            EditorGUIUtility.labelWidth = 60;
                            EditorGUILayout.ObjectField(selected ? "Enabled" : "Ignored", rule, typeof(SpawnRuleBase), true);
                            EditorGUIUtility.labelWidth = 0;

                            if (selected != pre)
                            {
                                if (selected == false)
                                {
                                    IgnoreToCall.Add(rule);
                                    so.Update();
                                    EditorUtility.SetDirty(this);
                                }
                                else
                                {
                                    IgnoreToCall.Remove(rule);
                                    so.Update();
                                    EditorUtility.SetDirty(this);
                                }
                            }

                            EditorGUILayout.EndHorizontal();
                        }


                        GUILayout.Space(6);
                        if (IgnoreToCall.Count > 0) if (GUILayout.Button("Clear Ignores")) IgnoreToCall.Clear();
                        if (GUILayout.Button(drawTargetLogics ? "Hide Nodes" : "Draw All Target Nodes")) drawTargetLogics = !drawTargetLogics;
                        GUILayout.Space(3);
                        if (drawTargetLogics)
                        {
                            EditorGUILayout.HelpBox("Changing parameters here will change parameters in source FieldModificator too!", MessageType.None);
                            spawner.DrawSpawnerGUIBody();
                        }
                    }
                    else
                        EditorGUILayout.LabelField("Wrong Spawner", FGUI_Resources.HeaderStyle);
                }
                else
                    EditorGUILayout.LabelField("Wrong Spawner", FGUI_Resources.HeaderStyle);


                GUI.backgroundColor = new Color(.5f, 1f, .5f, 0.9f);
                GUI.backgroundColor = bc;
            }
        }

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (CallFrom != null)
            {
                GUILayout.Space(3);
                EditorGUILayout.EndVertical();
                base.NodeFooter(so, mod);
            }
            else
                base.NodeFooter(so, mod);
        }
#endif

        #endregion


        #region Prepare

        // Optimizing -> only one list per rule call on all cells
        private List<SpawnRuleBase> runCall = new List<SpawnRuleBase>();

        public override void ResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset)
        {
            base.ResetRule(grid, preset);
            FieldSpawner spawner = GetSpawner();

            runCall = null;
            if (spawner == null) return;

            runCall = new List<SpawnRuleBase>();
            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                if (spawner.Rules[i] == null) continue;
                if (spawner.Rules[i].Enabled == false) continue;
                if (IgnoreToCall.Contains(spawner.Rules[i])) continue;
                runCall.Add(spawner.Rules[i]);
                spawner.Rules[i].ResetRule(grid, preset);
            }

            if (runCall.Count == 0) runCall = null;
        }

        public override void Refresh()
        {
            base.Refresh();
            CellAllow = true;
            internalAllow = true;
            if (runCall == null) return;
            for (int i = 0; i < runCall.Count; i++) runCall[i].Refresh();
        }

        #endregion


        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            base.PreGenerateResetRule(grid, preset, callFrom);
            if (CallModGraph) CallModGraph.PreGenerateResetRule(grid, preset, callFrom);
        }

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            if (CallModGraph) CallModGraph.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

            if (runCall == null) return;
            if (CellAllow == false) return;

            for (int i = 0; i < runCall.Count; i++)
            {
                runCall[i].CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);

                if ( runCall[i].CellAllow == false)
                {
                    internalAllow = false;
                    break;
                }
            }

            if (CallResult != EResult.JustTriggerEvents) CellAllow = internalAllow;
        }


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CellInfluence(preset, mod, cell, ref spawn, grid);
            if (runCall == null) return;
            if (internalAllow == false) return;

            for (int i = 0; i < runCall.Count; i++)
            {
                runCall[i].CellInfluence(preset, mod, cell, ref spawn, grid);

                if (runCall[i].CellAllow == false)
                {
                    internalAllow = false;
                    break;
                }
            }

            if (CallResult != EResult.JustTriggerEvents) CellAllow = internalAllow;
        }


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (runCall == null) return;
            if (internalAllow == false) return;

            for (int i = 0; i < runCall.Count; i++)
            {
                runCall[i].OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);
            }

            base.OnConditionsMetAction(mod, ref thisSpawn, preset, cell, grid);
            if (CallResult != EResult.JustTriggerEvents) CellAllow = internalAllow;
        }

        protected FieldSpawner GetSpawner()
        {
            if (CallFrom == null) return null;
            if (SpawnerId >= CallFrom.Spawners.Count) return null;
            FieldSpawner spawner = CallFrom.Spawners[SpawnerId];
            return spawner;
        }

    }
}