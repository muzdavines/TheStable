using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public abstract class ModulePool<TModule> where TModule : Module
    {
        /// <summary>
        /// The custom size of the module pool.
        /// </summary>
        public int CustomPoolSize = 5;

        /// <summary>
        /// If true, the pool size will not be calculated based on number of modules in tutorials.
        /// </summary>
        public bool OverridePoolSize;

        /// <summary>
        /// Stores all modules inside.
        /// </summary>
        public TModule[] Pool;

        /// <summary>
        /// The prefab which will be cloned for this pool.
        /// </summary>
        public GameObject Prefab;

        /// <summary>
        /// The index of the next module that will be allocated.
        /// </summary>
        private int _currentAllocationIndex = 0;

        /// <summary>
        /// The parent transform where module pool is residing
        /// </summary>
        private Transform _defaultTransform;

        /// <summary>
        /// Allocates the module. Module activation must be handled by the caller of this method.
        /// </summary>
        /// <returns>Module Component of the Allocated Module. Returns null if all modules are busy.</returns>
        public TModule AllocateModule()
        {
            if (_currentAllocationIndex > Pool.Length - 1)
            {
                TMLogger.LogWarning("No more modules left in reserve!");
                return null;
            }

            return Pool[_currentAllocationIndex++];
        }

        /// <summary>
        /// Clears the module pool by calling Destroy() for each cloned module (if possible).
        /// </summary>
        /// <param name="pool">The pool that will be cleared.</param>
        public void ClearPool(ref TModule[] pool)
        {
            if (pool == null || pool.Length == 0) return;

            foreach (var module in pool)
            {
                var moduleGameObject = module.gameObject;

                if (moduleGameObject != null)
                {
                    Object.Destroy(moduleGameObject);
                }
            }

            pool = null;
        }

        /// <summary>
        /// Deactivates all active modules in the pool and places them back into parent transform.
        /// </summary>
        public void FreeModulePool()
        {
            for (int i = 0; i < _currentAllocationIndex; i++)
            {
                var module = Pool[i];
                module.Deactivate();
                module.transform.SetParent(_defaultTransform);
            }

            _currentAllocationIndex = 0;
        }

        /// <summary>
        /// Initializes the module pool. Must be called when the scene starts.
        /// </summary>
        /// <param name="tutorials">Reference to the list of tutorials which will be used to calculate the size of this module pool</param>
        /// <param name="parent">The parent in which the pool will be instantiated. This parameter will be ignored if OverridePoolSize is set to true.</param>
        public void Instantiate(ref List<Tutorial> tutorials, Transform parent)
        {
            if (Prefab == null) return;

            if (NullChecker.IsNull(Prefab, string.Format("Unable to instantiate a prefab of type {0} ! No prefab assigned.", typeof(TModule).Name)))
                return;

            _defaultTransform = parent;

            Pool = new TModule[!OverridePoolSize ? CalculateMaxPoolSize(ref tutorials) : CustomPoolSize];

            string moduleName = typeof(TModule).Name;

            for (int i = 0; i < Pool.Length; i++)
            {
                var newModule = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity, _defaultTransform) as GameObject;
                if (newModule == null)
                    continue;

                Pool[i] = newModule.GetComponent<TModule>();
                Pool[i].gameObject.name = string.Format("{0}_{1}", moduleName, i);
            }
        }

        /// <summary>
        /// Calculates the maximum size of this module pool.
        /// </summary>
        /// <param name="tutorials">Reference to the list of tutorials which will be used to calculate the size of this module pool</param>
        /// <returns>An integer representing the size of the module pool.</returns>
        protected abstract int CalculateMaxPoolSize(ref List<Tutorial> tutorials);
    }
}