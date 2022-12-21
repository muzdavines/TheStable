using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{

    public partial class FieldModificationEditor
    {
        public FieldModification Get { get { if (_get == null) _get = (FieldModification)target; return _get; } }
        private FieldModification _get;


        public static void AddPrefabsContextMenuItems(GenericMenu menu, FieldModification Get)
        {
            if (Get.DrawSetupFor == FieldModification.EModificationMode.CustomPrefabs)
            {
                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random"), false, () =>
                { Get.AddSpawner(-1, Get.DrawSetupFor); });


                if (Get.PrefabsList != null)
                {
                    for (int i = 0; i < Get.PrefabsList.Count; i++)
                    {
                        var sobj = Get.PrefabsList[i];
                        if (sobj == null) continue;
                        int ind = i;

                        if (Get.PrefabsList[i].GameObject == null) continue;

                        menu.AddItem(new GUIContent(Get.PrefabsList[i].GameObject.name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }
                }

            }
            else if (Get.DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp)
            {

                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random"), false, () =>
                { Get.AddSpawner(-1, FieldModification.EModificationMode.ObjectsStamp); });

                menu.AddItem(new GUIContent("Emitter"), false, () =>
                { Get.AddSpawner(-3, FieldModification.EModificationMode.ObjectsStamp); });

                if (Get.OStamp != null)
                    for (int i = 0; i < Get.OStamp.Prefabs.Count; i++)
                    {
                        var sobj = Get.OStamp.Prefabs[i];
                        if (sobj == null) continue;
                        int ind = i;

                        if (Get.OStamp.Prefabs[i].GameObject == null) continue;

                        menu.AddItem(new GUIContent(Get.OStamp.Prefabs[i].GameObject.name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }
            }
            else if (Get.DrawSetupFor == FieldModification.EModificationMode.ObjectMultiEmitter)
            {

                menu.AddItem(new GUIContent("Empty"), false, () =>
                { Get.AddSpawner(-2, FieldModification.EModificationMode.CustomPrefabs); });

                menu.AddItem(new GUIContent("Random Emitter"), false, () =>
                { Get.AddSpawner(-1, FieldModification.EModificationMode.ObjectMultiEmitter); });

                if (Get.OMultiStamp != null)
                    for (int i = 0; i < Get.OMultiStamp.PrefabsSets.Count; i++)
                    {
                        var sobj = Get.OMultiStamp.PrefabsSets[i];
                        if (sobj == null) continue;
                        int ind = i;

                        menu.AddItem(new GUIContent(Get.OMultiStamp.PrefabsSets[i].name), false, () =>
                        {
                            Get.AddSpawner(ind, Get.DrawSetupFor);
                        });
                    }


                //menu.AddItem(new GUIContent("Random Emitter"), false, () =>
                //{ Get.AddSpawner(-2, RoomModification.EModificationMode.ObjectMultiEmitter); });

            }

            menu.AddItem(GUIContent.none, false, () => { });
            menu.AddItem(new GUIContent("Add Sub Spawner (Experimental)"), false, () => { Get.AddSubSpawner(); });
        }



        public static List<UnityEngine.Object> CleanupAndGetUnused(FieldSetup setup, bool checkForSpawnRules = false)
        {
            // Do Initial Cleanup
            var inside = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(setup));

            List<FieldModification> packed = new List<FieldModification>();
            List<FieldModification> inUtils = new List<FieldModification>();
            List<FieldModification> notPacked = new List<FieldModification>();
            List<UnityEngine.Object> unused = new List<UnityEngine.Object>();

            for (int i = 0; i < inside.Length; i++)
            {
                if (inside[i] is FieldModification)
                {
                    FieldModification mod = inside[i] as FieldModification;

                    if (setup.RootPack.FieldModificators.Contains(mod))
                    {
                        packed.Add(mod);
                        continue;
                    }

                    if (setup.UtilityModificators.Contains(mod))
                    {
                        inUtils.Add(mod);
                        continue;
                    }

                    notPacked.Add(mod);

                }
            }


            for (int i = notPacked.Count - 1; i >= 0; i--)
            {
                var mod = notPacked[i];

                for (int c = 0; c < setup.CellsCommands.Count; c++)
                {
                    if (setup.CellsCommands[c].extraMod == mod || setup.CellsCommands[c].TargetModification == mod)
                    {
                        setup.UtilityModificators.Add(mod);
                        notPacked.RemoveAt(i);
                    }
                }
            }


            if (checkForSpawnRules)
            {
                List<SpawnRuleBase> unknownRules = new List<SpawnRuleBase>();

                for (int i = 0; i < inside.Length; i++)
                {
                    if (inside[i] is SpawnRuleBase)
                    {
                        SpawnRuleBase rule = inside[i] as SpawnRuleBase;
                        bool owned = false;

                        for (int p = 0; p < packed.Count; p++)
                            if (packed[p].OwnsRule(rule) == true)
                            {
                                owned = true;
                                break;
                            }

                        if (owned) break;

                        for (int p = 0; p < inUtils.Count; p++)
                            if (inUtils[p].OwnsRule(rule) == true)
                            {
                                owned = true;
                                break;
                            }

                        if (owned) break;

                        unknownRules.Add(rule);

                    }
                }


                for (int i = 0; i < unknownRules.Count; i++)
                {
                    if (unused.Contains(unknownRules[i]) == false) unused.Add(unknownRules[i]);
                }

            }


            for (int i = notPacked.Count - 1; i >= 0; i--)
            {
                var mod = notPacked[i];
                if (unused.Contains(mod) == false) unused.Add(mod);
            }

            return unused;

        }


    }
}