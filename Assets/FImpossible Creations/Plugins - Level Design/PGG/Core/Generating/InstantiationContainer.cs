using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Class which is helping handling instantiated objects by FieldModificators' Spawners
    /// </summary>
    [System.Serializable]
    public class InstantiationContainer
    {
        public FieldSetup Setup;
        public ModificatorsPack Pack;
        public FieldModification Mod;
        public Transform Transform;

        public InstantiationContainer(ModificatorsPack pack)
        {
            Pack = pack;
            Transform = new GameObject(pack.name + " : Container").transform;
        }

        public InstantiationContainer(FieldModification mod)
        {
            Mod = mod;
            Pack = mod.ParentPack;
            Transform = new GameObject(mod.name + " : Container").transform;
        }
    }

}
