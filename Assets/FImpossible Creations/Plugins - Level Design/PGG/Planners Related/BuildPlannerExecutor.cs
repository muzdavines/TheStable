using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Planning;
using UnityEngine.Events;

namespace FIMSpace.Generating
{
    /// <summary>
    /// All code of this component is placed in the partial class files!
    /// </summary>
    [AddComponentMenu("FImpossible Creations/PGG/Build Planner Executor", 0)]
    public partial class BuildPlannerExecutor : MonoBehaviour
    {

        [Tooltip("Build Planner Preset to be executed")]
        public BuildPlannerPreset BuildPlannerPreset;

        public PlannerPreparation PlannerPrepare { get { return _plannerPrepare; } }
        [HideInInspector, SerializeField] private PlannerPreparation _plannerPrepare;
        public List<GameObject> Generated { get { return _generated; } }
        [HideInInspector, SerializeField] private List<GameObject> _generated = new List<GameObject>();


        [Tooltip("Generating Build Planner's scheme asynchronously (without lags)\nBeware! Some of the presets will not support this.")] public bool Async = false;
        [Tooltip("Triggering generation with use of 'Flexible Generators' to call rules or instantiate target objects in coroutine.")] public bool FlexibleGen = false;
        public int Seed = 0;
        public bool RandomSeed = true;

        [Tooltip("Generating preview and objects on game playmode Start()")]
        public bool GenerateOnStart = false;

        [Tooltip("Call custom unity event after executor finishes generating lyout and objects")]
        public UnityEvent RunAfterGenerating;


        void Start()
        {
            if (GenerateOnStart)
            {
                if (BuildPlannerPreset != null) Generate();
            }
        }


        #region User Utility Methods

        public void User_SetInstancesCount(string plannerName, int instances)
        {
            string nameLower = plannerName.ToLower();
            for (int i = 0; i < BuildPlannerPreset.BasePlanners.Count; i++)
            {
                if (BuildPlannerPreset.BasePlanners[i].name.ToLower().StartsWith(nameLower))
                {
                    _plannerPrepare.FieldSetupCompositions[i].Instances = Mathf.Max(1, instances);
                    return;
                }
            }
        }

        #endregion


    }

}