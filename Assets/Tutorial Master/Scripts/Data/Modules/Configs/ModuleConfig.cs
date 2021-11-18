using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to store and associate module settings with a MonoBehaviour module.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <typeparam name="TSettings">The type of the module settings.</typeparam>
    [Serializable]
    public abstract class ModuleConfig<TModule, TSettings>
        where TModule : Module
        where TSettings : ModuleSettings
    {
        private GameObject _currentlyInstantiatedGameObject;

        /// <summary>
        /// The unique identifier for this module config.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The module which will be used.
        /// </summary>
        public TModule Module;

        /// <summary>
        /// A custom module prefab which will be instantiated.
        /// </summary>
        public GameObject ModulePrefab;

        /// <summary>
        /// Holds the settings of the module.
        /// </summary>
        public TSettings Settings;

        /// <summary>
        /// If set to true, this module will be used.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Set true if you want to override the module pool prefab.
        /// </summary>
        public bool OverridePrefab;

        /// <summary>
        /// The parent TutorialMasterManager instance of this Module Config.
        /// </summary>
        [SerializeField] 
        protected TutorialMasterManager ParentManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleConfig{TModule, TModuleSettings}"/> class and generates a language key for localization.
        /// </summary>
        protected ModuleConfig(TutorialMasterManager manager)
        {
            ParentManager = manager;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Activates this Module and assigns a language key to it.
        /// </summary>
        /// <param name="languageKey">The language key.</param>
        public void Activate(string languageKey = "")
        {
            if (OverridePrefab)
            {
                if (NullChecker.IsNull(ModulePrefab, "Unable to activate. No overriden prefab is supplied!"))
                    return;

                _currentlyInstantiatedGameObject = Object.Instantiate(ModulePrefab, Vector3.zero, Quaternion.identity);
                Module = _currentlyInstantiatedGameObject.GetComponent<TModule>();
            }
            else
            {
                Module = GetModuleFromPool();
            }

            if (NullChecker.IsNull(Module, "Unable to activate! Module is null!"))
                return;

            Module.Activate(ParentManager, Settings, languageKey);
        }

        /// <summary>
        /// Deactivates this Module.
        /// </summary>
        public void Deactivate()
        {
            if (NullChecker.IsNull(Module, "Unable to deactivate! Module is null!"))
                return;

            Module.Deactivate();

            if (OverridePrefab)
                Object.Destroy(_currentlyInstantiatedGameObject);
        }

        /// <summary>
        /// Updates this Module's localized content to a new language.
        /// </summary>
        /// <param name="languageKey">Unique key of a language which will be set for this Module.</param>
        public void SetLanguage(string languageKey)
        {
            if (NullChecker.IsNull(Module, "Unable to set language! Module is null!"))
                return;

            Module.SetLanguage(languageKey);
        }

        /// <summary>
        /// Tries to get an instance of Module from a pool.
        /// </summary>
        protected abstract TModule GetModuleFromPool();
    }
}