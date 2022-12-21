#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.QuickSolutions
{
    public class SR_CallSubSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Call Sub Spawner"; }
        public override string Tooltip() { return "Use this node to call sub spawner stored in this field modification"; }
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override bool CanBeGlobal() { return false; }

        FieldSpawner targetSubSpawner = null;

        [HideInInspector] public int CallSpawner = -1;
        
        [Tooltip("Inherit this spawner final coordinates onto sub-spawner execution")]
        [HideInInspector] public bool InheritCoords = false;

        [Tooltip("Call Sub-Spawner after all rules and modificators")]
        [HideInInspector]public bool PostCall = false;
        
        [Tooltip("Include extra spawns operations if using 'Call on each side' with Wall Placer or 'Run on repetition' with Get Coordinates nodes")]
        [HideInInspector] public bool UseTemps = false;

        [HideInInspector] public FieldModification SubSpawnersOf = null;


        public FieldSpawner toCall
        {
            get
            {
                if (CallSpawner < 0) return null;
                if (OwnerSpawner == null) return null;
                if (parentMod == null) return null;
                if (CallSpawner >= parentMod.SubSpawners.Count) return null;
                return parentMod.SubSpawners[CallSpawner];
            }
        }

        public FieldModification parentMod
        {
            get
            {
                if (SubSpawnersOf != null) return SubSpawnersOf;
                if (OwnerSpawner.Parent != null) return OwnerSpawner.Parent;
                return null;
            }
        }

        void RefreshSpawner()
        {
            targetSubSpawner = toCall;
        }

        public override void Refresh()
        {
            base.Refresh();
            RefreshSpawner();

            if (targetSubSpawner == null) return;
            var spawner = targetSubSpawner;

            if (spawner.Rules == null) return;
            for (int i = 0; i < spawner.Rules.Count; i++)
            {
                var rl = spawner.Rules[i]; if (rl == null) continue;
                rl.Refresh();
            }
        }


        #region Editor Related

#if UNITY_EDITOR

        int[] _mod_spawners = null;
        GUIContent[] _mod_spawnerNames = null;
        SerializedProperty sp = null;

        void CheckSpawnersPopupList(bool hardReload = false)
        {
            if (OwnerSpawner == null) return;
            if (parentMod == null) return;
            if (parentMod.SubSpawners == null) return;
            if (parentMod.SubSpawners.Count == 0) return;

            bool reload = false;
            if (hardReload) reload = true;
            if (!reload) if (_mod_spawners == null) reload = true;
            if (!reload) if (_mod_spawners.Length != parentMod.SubSpawners.Count + 1) reload = true;

            if (reload)
            {
                _mod_spawners = new int[parentMod.SubSpawners.Count + 1];
                _mod_spawnerNames = new GUIContent[_mod_spawners.Length];

                _mod_spawners[0] = -1;
                _mod_spawnerNames[0] = new GUIContent("None");

                for (int i = 1; i < _mod_spawners.Length; i++)
                {
                    int spawnerIndex = i - 1;
                    _mod_spawners[i] = spawnerIndex;
                    _mod_spawnerNames[i] = new GUIContent(parentMod.SubSpawners[spawnerIndex].Name);
                }
            }
        }


        public override void NodeBody(SerializedObject so)
        {
            base.NodeBody(so);

            CheckSpawnersPopupList();

            if (parentMod.SubSpawners.Count == 0)
            {
                EditorGUILayout.HelpBox("No Sub-Spawners!", MessageType.None);
                if (GUILayout.Button("Add first sub-spawner")) { parentMod.AddSubSpawner(); }
            }
            else
            {
                if (_mod_spawners == null)
                {
                    EditorGUILayout.HelpBox("Failed to reload references, click/hover cursor here to trigger refresh", MessageType.None);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 90;
                    CallSpawner = EditorGUILayout.IntPopup(new GUIContent("Call Spawner:"), CallSpawner, _mod_spawnerNames, _mod_spawners);
                    EditorGUIUtility.labelWidth = 0;

                    if (GUILayout.Button("Show Sub-Spawneres", GUILayout.MaxWidth(150))) { FieldModification._subDraw = 1; }
                    if (GUILayout.Button(new GUIContent("+", "Add next sub-spawner and draw sub spawners list"), GUILayout.Width(22))) { parentMod.AddSubSpawner(); }

                    EditorGUILayout.EndHorizontal();

                }
            }

            if (sp == null) sp = so.FindProperty("InheritCoords");
            if (sp != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PropertyField(sp);
                SerializedProperty spc = sp.Copy();
                EditorGUIUtility.labelWidth = 70;
                spc.Next(false); EditorGUILayout.PropertyField(spc);
                spc.Next(false); EditorGUILayout.PropertyField(spc);
                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = 186;
                EditorGUIUtility.fieldWidth = 24;
                spc.Next(false);
                EditorGUILayout.PropertyField(spc, new GUIContent("(Optional) Call Sub-Spawner of:", "Call sub-spawners of other field modificator"));
                EditorGUIUtility.labelWidth = 0;
                EditorGUIUtility.fieldWidth = 0;
            }

        }

#endif

        #endregion


        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            if (targetSubSpawner == null) return;


            if (!PostCall) ApplySpawnerCall(cell, thisSpawn, grid, preset);
            else
            {
                SpawnData spawn = thisSpawn;
                if (OwnerSpawner.OnPostCallEvents == null) OwnerSpawner.OnPostCallEvents = new System.Collections.Generic.List<System.Action>();

                OwnerSpawner.OnPostCallEvents.Add(
                    () =>
                    {
                        ApplySpawnerCall(cell, spawn, grid, preset);
                    });
            }


            if (UseTemps)
                if (OwnerSpawner != null) // Search temp spawns
                {
                    var tempSpawns = OwnerSpawner.GetExtraSpawns();

                    if (tempSpawns != null)
                    {
                        for (int t = 0; t < tempSpawns.Count; t++)
                        {
                            var tSpawn = tempSpawns[t];

                            if (!PostCall) ApplySpawnerCall(cell, tSpawn, grid, preset, true);
                            else
                            {
                                SpawnData spawn = tSpawn;
                                if (OwnerSpawner.OnPostCallEvents == null) OwnerSpawner.OnPostCallEvents = new System.Collections.Generic.List<System.Action>();
                                OwnerSpawner.OnPostCallEvents.Add(() => { ApplySpawnerCall(cell, spawn, grid, preset, true); });
                            }
                        }
                    }
                }

        }


        void ApplySpawnerCall(FieldCell cell, SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, bool isTemp = false)
        {
            if (targetSubSpawner == null) return;
            if (targetSubSpawner.Enabled == false) return;

            var data = targetSubSpawner.RunSpawnerOnCell(parentMod, preset, cell, grid, Vector3.zero, null, true);

            if (data != null)
                if (InheritCoords)
                {
                    data.Offset += spawn.Offset;
                    data.DirectionalOffset += spawn.DirectionalOffset;
                    data.RotationOffset += spawn.RotationOffset;
                    data.LocalRotationOffset += spawn.LocalRotationOffset;
                }
        }

        public override void PreGenerateResetRule(FGenGraph<FieldCell, FGenPoint> grid, FieldSetup preset, FieldSpawner callFrom)
        {
            RefreshSpawner();
            if (targetSubSpawner == null) return;
            if (targetSubSpawner.Rules == null) return;

            for (int i = 0; i < targetSubSpawner.Rules.Count; i++)
            {
                var rl = targetSubSpawner.Rules[i]; if (rl == null) continue;
                rl.PreGenerateResetRule(grid, preset, targetSubSpawner);
            }
        }


    }
}