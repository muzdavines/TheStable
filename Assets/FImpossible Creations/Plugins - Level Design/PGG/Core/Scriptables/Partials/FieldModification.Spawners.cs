using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public partial class FieldModification : ScriptableObject
    {

        public PrefabReference GetPrefabRef(int toSpawn)
        {
            if (DrawSetupFor == EModificationMode.CustomPrefabs)
            {
                if (toSpawn < 0)
                {
                    if (toSpawn == -2) return null;
                    else
                    {
                        if (PrefabsList.Count > 0)
                        return PrefabsList[FGenerators.GetRandom(0, PrefabsList.Count)];
                    }
                }
                else
                    if (toSpawn < PrefabsList.Count)
                    return PrefabsList[toSpawn];
            }
            else if (DrawSetupFor == EModificationMode.ObjectsStamp)
            {
                if (toSpawn < 0)
                    return OStamp.Emit().PrefabReference;
                else
                    if (toSpawn < OStamp.Prefabs.Count)
                    return OStamp.Prefabs[toSpawn];
            }
            else if (DrawSetupFor == EModificationMode.ObjectMultiEmitter)
            {
                if (toSpawn < 0)
                    return OMultiStamp.PrefabsSets[FGenerators.GetRandom(0, OMultiStamp.PrefabsSets.Count)].Emit().PrefabReference;
                else
                    if (toSpawn < OMultiStamp.PrefabsSets.Count)
                    return OMultiStamp.PrefabsSets[toSpawn].Emit().PrefabReference;
            }

            return null;
        }

        //private void OnValidate()
        //{
        //    for (int i = 0; i < Spawners.Count; i++)
        //    {
        //        Spawners[i].UseParentPackageRules = true;
        //        Spawners[i].UseGlobalRules = true;
        //    }
        //}

        public int GetPRSpawnOptionsCount()
        {
            switch (DrawSetupFor)
            {
                case EModificationMode.CustomPrefabs:
                    if (PrefabsList == null) return 0;
                    return PrefabsList.Count;

                case EModificationMode.ObjectsStamp:
                    if (OStamp == null) return 0;
                    return OStamp.Prefabs.Count;

                case EModificationMode.ObjectMultiEmitter:
                    if (OMultiStamp == null) return 0;
                    return OMultiStamp.PrefabsSets.Count;
            }

            return 0;
        }

        public FieldSetup TryGetParentSetup()
        {
            FieldSetup setup = null;
            if (ParentPack != null) setup = ParentPack.ParentPreset;
            if (setup == null) setup = ParentPreset;
            return setup; ;
        }

        public List<FieldVariable> TryGetFieldVariablesList()
        {
            FieldSetup setup = TryGetParentSetup();
            if (setup == null) return null;

            List<FieldVariable> list = new List<FieldVariable>();

            for (int s = 0; s < Spawners.Count; s++)
            {
                for (int r = 0; r < Spawners[s].Rules.Count; r++)
                {
                    var rl = Spawners[s].Rules[r];
                    var rlv = rl.GetVariables();
                    if (rlv == null) continue;
                    if (rlv.Count == 0) continue;

                    for (int rv = 0; rv < rlv.Count; rv++)
                    {
                        if (string.IsNullOrEmpty(rlv[rv].name)) continue;
                        var fielV = setup.GetVariable(rlv[rv].name);

                        if (FGenerators.CheckIfExist_NOTNULL(fielV )) if (!list.Contains(fielV)) list.Add(fielV);
                    }
                }
            }

            return list;
        }


        public string GetPRSpawnOptionName(int id)
        {
            switch (DrawSetupFor)
            {

                case EModificationMode.CustomPrefabs:
                    if (PrefabsList == null) return "";
                    if (id > PrefabsList.Count - 1) return "";
                    if (PrefabsList[id].CoreGameObject == null) return "";
                    return PrefabsList[id].CoreGameObject.name;

                case EModificationMode.ObjectsStamp:
                    if (OStamp == null) return "";
                    if (id > OStamp.Prefabs.Count - 1) return "";
                    if (OStamp.Prefabs[id].CoreGameObject == null) return "";
                    return OStamp.Prefabs[id].CoreGameObject.name;

                case EModificationMode.ObjectMultiEmitter:
                    if (OMultiStamp == null) return "";
                    if (id > OMultiStamp.PrefabsSets.Count - 1) return "";
                    if (OMultiStamp.PrefabsSets[id] == null) return "";
                    return OMultiStamp.PrefabsSets[id].name;
            }

            return "";
        }

        /// <summary> For editor use - drawing sub-spawners list switch </summary>
        public static int _subDraw = 0;

        public void AddSubSpawner()
        {
            _subDraw = 1;
            FieldSpawner sp = new FieldSpawner(-2, DrawSetupFor, this);
            sp.UseParentPackageRules = true;
            sp.UseGlobalRules = false;
            sp.DontInheritRotations = true;
            SubSpawners.Add(sp);
        }


        public void AddSpawner(int stampObject, EModificationMode mode)
        {
            _subDraw = 0;
            FieldSpawner sp = new FieldSpawner(stampObject, mode, this);
            sp.UseParentPackageRules = true;
            sp.UseGlobalRules = true;
            Spawners.Add(sp);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void RemoveSpawner(FieldSpawner spawner)
        {
            for (int i = 0; i < spawner.Rules.Count; i++)
                RemoveAsset(spawner.Rules[i]);

            Spawners.Remove(spawner);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }



#if UNITY_EDITOR
        public void AddContextMenuItems(GenericMenu menu, FieldSpawner spawner)
        {
            List<Type> types = GetDerivedTypes(typeof(SpawnRuleBase));
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                if (type == typeof(SpawnRuleBase)) continue;
                string path = GetMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                string name = path;
                SpawnRuleBase rule = CreateInstance(type) as SpawnRuleBase;
                if (rule != null) name = path.Replace(rule.GetType().Name, "") + rule.TitleName();

                menu.AddItem(new GUIContent(name), false, () =>
                {

                    var rl = rule;
                    var t = spawner;
                    Action ac = new Action(() => { t.AddRule(rl);  });
                    FieldModification.AddEditorEvent(ac);
                });
            }
        }
#endif

    }

}