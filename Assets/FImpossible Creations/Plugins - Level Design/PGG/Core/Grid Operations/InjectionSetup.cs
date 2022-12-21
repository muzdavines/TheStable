using UnityEngine;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class for helping setting up injections for Field Setups
    /// </summary>
    [System.Serializable]
    public class InjectionSetup
    {
        public enum EInjectTarget : int
        {
            [Tooltip("Running this modificator on whole grid additionaly (runs even when disabled by field setup)")]
            Modificator = 0,
            [Tooltip("Running this modificator on whole grid additionaly (not runs mods disabled by field setup)")]
            Pack = 1,
            [Tooltip("Don't run this modificator but get access to field variables by it's parent FieldSetup")]
            ModOnlyForAccessingVariables = 2,
            [Tooltip("Don't run this pack but get access to pack variables")]
            PackOnlyForAccessingVariables = 3,
        }

        public EInjectTarget Inject = EInjectTarget.Modificator;
        public FieldModification Modificator;
        public ModificatorsPack ModificatorsPack;

        public enum EGridCall { Post, Pre }
        public EGridCall Call = EGridCall.Post;
        public bool OverrideVariables = false;

        [HideInInspector] public List<FieldVariable> Overrides;

        public InjectionSetup(FieldModification mod, EGridCall call)
        {
            Modificator = mod;
            Call = call;
        }

        public void AddOverride(FieldVariable var)
        {
            if (Overrides == null) Overrides = new List<FieldVariable>();
            Overrides.Add(var);
        }

    }
    
}
