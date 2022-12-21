#if UNITY_EDITOR
using UnityEditor;
#endif

using FIMSpace.Generating.Rules.Helpers;
using UnityEngine;

namespace FIMSpace.Generating.Rules.Cells
{
    public partial class SR_RemoveSpawn : SpawnRuleBase, ISpawnProcedureType
    {
        public EProcedureType Type { get { return EProcedureType.OnConditionsMet; } }
        public override string TitleName() { return "Remove Spawn"; }
        public override string Tooltip() { return "Removing desired spawn if some conditions are met"; }

        public RemoveInstruction Remove;

#if UNITY_EDITOR

#if UNITY_2019_4_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)] void OnReload() { removeDisplayed = 0; } // Just to fix strange Unity error
#endif
        [System.NonSerialized] int removeDisplayed = 0; // Just to fix strange Unity error
        public override bool EditorIsLoading() { return removeDisplayed < 4; }
        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {
            if (Event.current.type == EventType.Layout)
                removeDisplayed += 1;

            if (Event.current.type == EventType.Repaint)
            {
                GUIIgnore.Clear(); //
            }

            if (removeDisplayed > 4)
            {
                if (Event.current.type == EventType.Repaint) GUIIgnore.Add("Remove");
            }

            base.NodeFooter(so, mod);

            var sp = so.FindProperty("Remove");

            if (sp != null) RemoveInstruction.DrawGUI(sp, Remove);

            so.ApplyModifiedProperties();
        }
#endif

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            Remove.ProceedRemoving(OwnerSpawner, ref thisSpawn, cell, grid);
        }

    }
}