#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.FieldAndGrid
{
    public class SR_IsSameFieldSetup : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Is same Field Setup"; }
        public override string Tooltip() { return "Check if cell contains spawns from other field setup or other modifications package"; }

        public EProcedureType Type { get { return EProcedureType.Rule; } }

        public bool OnlyOnSameFieldSetup = false;
        public FieldSetup OnlyOnSetup = null;
        public ModificatorsPack OnlyOnPackage = null;


#if UNITY_EDITOR
        public override void NodeBody(SerializedObject so)
        {
            GUIIgnore.Clear();
            if (OnlyOnSameFieldSetup) { GUIIgnore.Add("OnlyOnSetup"); GUIIgnore.Add("OnlyOnPackage"); }
            base.NodeBody(so);
        }
#endif

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;

            if (OnlyOnSameFieldSetup)
            {
                for (int i = 0; i < cell.GetSpawnsJustInsideCell().Count; i++)
                {
                    var sp = cell.GetSpawnsJustInsideCell()[i];
                    if (sp.OwnerMod.ParentPreset == null) { UnityEngine.Debug.Log("nyll"); continue; }
                    if (sp.OwnerMod.ParentPreset != preset)
                    {
                        CellAllow = false;
                        return;
                    }
                }
            }
            else
            {
                if (OnlyOnPackage)
                    for (int i = 0; i < cell.GetSpawnsJustInsideCell().Count; i++)
                    {
                        var sp = cell.GetSpawnsJustInsideCell()[i];
                        if (sp.OwnerMod.ParentPack == null) continue;

                        if (sp.OwnerMod.ParentPack != OnlyOnPackage)
                        {
                            CellAllow = false;
                            return;
                        }
                    }

                if (OnlyOnSetup)
                    for (int i = 0; i < cell.GetSpawnsJustInsideCell().Count; i++)
                    {
                        var sp = cell.GetSpawnsJustInsideCell()[i];
                        if (sp.OwnerMod.ParentPreset == null) continue;

                        if (sp.OwnerMod.ParentPreset != OnlyOnSetup)
                        {
                            CellAllow = false;
                            return;
                        }
                    }
            }

        }
    }
}