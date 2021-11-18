using System;
using UnityEngine.Events;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class StageUnityEvent : UnityEvent<Stage> { }

    [Serializable]
    [DataValidator(typeof(StageEventsDataValidator))]
    public class StageEvents
    {
        /// <summary>
        /// Invoked when a new Stage was just entered.
        /// </summary>
        public StageUnityEvent OnStageEnter;

        /// <summary>
        /// Invoked when Stage settings were initialized and began playing.
        /// </summary>
        public StageUnityEvent OnStagePlay;

        /// <summary>
        /// Invoked when Stage has been unloaded.
        /// </summary>
        public StageUnityEvent OnStageExit;
    }
}