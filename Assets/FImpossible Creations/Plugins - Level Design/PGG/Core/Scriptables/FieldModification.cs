using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// It can be project asset as well as sub-asset
    /// </summary>
    [CreateAssetMenu(fileName = "FieldMod_", menuName = "FImpossible Creations/Procedural Generation/Grid Field Single Modification", order = 101)]
    public partial class FieldModification : ScriptableObject
    {
        public Transform TemporaryContainer;
        public bool Enabled = true;

        [Tooltip("Tag which can be used in rules by field modificators\nIf you need more tags use ',' commas")]
        public string ModTag = "";

        [Tooltip("Drawing also box on preview - useful for walls with one directional faces")]
        public bool DrawMeshAndBox = false;

        public enum ECombineSet { AsParentPack, ForceNotCombine, ForceCombine, ForceCombineAndSetStatic }
        [Tooltip("Switch mesh combine mode for all spawners of this modificator")]
        public ECombineSet Combine = ECombineSet.AsParentPack;

        public bool RunEmittersIfContains = false; // TODO -> Field or generator side

        public enum EModificationMode { CustomPrefabs, ObjectsStamp, ObjectMultiEmitter }
        public EModificationMode DrawSetupFor = EModificationMode.CustomPrefabs;


        public OStamperSet OStamp;
        [HideInInspector] public bool DrawObjectStamps = true;
        public OStamperMultiSet OMultiStamp;

        public List<PrefabReference> PrefabsList = new List<PrefabReference>();
        [HideInInspector] public bool DrawMultiObjectStamps = true;

        public List<FieldSpawner> Spawners = new List<FieldSpawner>();
        public List<FieldSpawner> SubSpawners = new List<FieldSpawner>();

        public FieldModification VariantOf;

        public bool _editor_drawStamp = true;

        public bool _editor_drawSpawners = true;
        public bool _editor_drawGlobalRules = false;
        public bool _editor_drawModPackRules = false;
        public int _editor_shareSelected = 0;

        [Tooltip("Optional")]
        public FieldSetup ParentPreset;
        public ModificatorsPack ParentPack;

        //private void OnEnable() { if ((int)DrawSetupFor > 2) DrawSetupFor = EModificationMode.CustomPrefabs; UnityEditor.EditorUtility.SetDirty(this); }
        //private void OnValidate() { if ((int)DrawSetupFor > 2) DrawSetupFor = EModificationMode.CustomPrefabs; UnityEditor.EditorUtility.SetDirty(this); }

        public virtual string GetMenuName(Type type)
        {
            string name = type.ToString();
            name = name.Replace("FIMSpace.Generating.Rules.", "");
            return name.Replace('.', '/');
        }


        public static List<Type> GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try { types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray()); }
                catch (ReflectionTypeLoadException) { }
            }

            return types;
        }


        public virtual void OnSceneGUI(FieldCell cell, FieldSetup preset, FGenGraph<FieldCell, FGenPoint> grid, SpawnData spawn)
        {
            Vector3 wPos = preset.GetCellWorldPosition(cell);
            Quaternion wRot = Quaternion.Euler(spawn.RotationOffset);
            //Gizmos.DrawSphere(wPos, preset.CellSize * 0.2f);
            Gizmos.DrawLine(wPos, wPos + wRot * (Vector3.forward) * preset.CellSize / 2f);
        }


        public void AddAsset(ScriptableObject target)
        {
            if (target != null)
            {
                target.name = target.GetType().Name + "-[" + this.name + "]";
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.AddObjectToAsset(target, this);
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void RemoveAsset(ScriptableObject target)
        {
#if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.IsSubAsset(target))
            {
#if UNITY_2018_1_OR_NEWER
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(target);
#endif
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public void DuplicateSpawner(FieldSpawner spawner)
        {
            FieldSpawner copy = spawner.Copy();
            Spawners.Add(copy);
        }


        public bool RequiresScaledGraphs()
        {
            if (Spawners == null) return false;
            for (int s = 0; s < Spawners.Count; s++)
            {
                if (FGenerators.CheckIfIsNull(Spawners[s])) continue;
                if (Spawners[s].OnScalledGrid != 1) return true;
            }

            return false;
        }


#if UNITY_EDITOR

        public GUIContent[] GetToSpawnNames()
        {
            int tgtCount = GetPRSpawnOptionsCount() + 2;
            if (DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp) tgtCount += 1;

            int off = 2; if (DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp) off = 3;
            GUIContent[] spawnersC = new GUIContent[tgtCount];

            if (off == 2) spawnersC[0] = new GUIContent("Emitter");

            spawnersC[off - 2] = new GUIContent("Empty");
            spawnersC[off - 1] = new GUIContent("Random");

            if (GetPRSpawnOptionsCount() > 0)
            {
                string name = GetPRSpawnOptionName(0);
                spawnersC[off] = new GUIContent(name);

                for (int i = off + 1; i < tgtCount; i++)
                {
                    name = GetPRSpawnOptionName(i - off);
                    spawnersC[i] = new GUIContent(name);
                }
            }

            return spawnersC;
        }

        public string[] GetMultiSpawnNames()
        {
            int tgtCount = GetPRSpawnOptionsCount();
            string[] spawnersC = new string[tgtCount];

            if (GetPRSpawnOptionsCount() > 0)
            {
                string name = GetPRSpawnOptionName(0);
                spawnersC[0] = (name);

                for (int i = 1; i < tgtCount; i++)
                {
                    name = GetPRSpawnOptionName(i);
                    spawnersC[i] = (name);
                }
            }

            return spawnersC;
        }

        /// <summary> If provided rule exists in some of the modificator's spawners </summary>
        public bool OwnsRule(SpawnRuleBase rule)
        {
            for (int s = 0; s < Spawners.Count; s++)
            {
                for (int r = 0; r < Spawners[s].Rules.Count; r++)
                {
                    if (Spawners[s].Rules[r] == rule) return true;
                }
            }

            return false;
        }

        public int[] GetToSpawnIndexes()
        {
            int tgtCount = GetPRSpawnOptionsCount() + 2;
            if (DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp) tgtCount += 1;

            int off = 2; if (DrawSetupFor == FieldModification.EModificationMode.ObjectsStamp) off = 3;
            int[] spawnersC = new int[tgtCount];

            if (off == 3) spawnersC[0] = -3;

            spawnersC[off - 2] = -2;
            spawnersC[off - 1] = -1;

            if (GetPRSpawnOptionsCount() > 0)
            {
                //string name = GetPRSpawnOptionName(0);
                spawnersC[off] = 0;

                for (int i = off + 1; i < tgtCount; i++)
                {
                    //name = GetPRSpawnOptionName(i - off);
                    spawnersC[i] = i - off;
                    //UnityEngine.Debug.Log("Get " + name);
                }
            }

            return spawnersC;
        }



        public static void AddEditorEvent(System.Action ac)
        {
            EditorEvents.Add(ac);
        }

        static List<System.Action> EditorEvents = new List<System.Action>();
        public static void UseEditorEvents()
        {
            for (int i = 0; i < EditorEvents.Count; i++)
            {
                if (EditorEvents[i] != null) EditorEvents[i].Invoke();
            }

            EditorEvents.Clear();
        }

#endif

    }
}