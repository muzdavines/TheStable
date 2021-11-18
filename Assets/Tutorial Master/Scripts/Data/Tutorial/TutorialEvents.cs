using System;
using UnityEngine.Events;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    public class TutorialUnityEvent : UnityEvent<Tutorial> { }

    /// <summary>
    /// Stores all UnityEvents for Tutorial.
    /// </summary>
    [Serializable]
    public class TutorialEvents
    {
        /// <summary>
        /// Invoked when Tutorial is about to be initialized.
        /// </summary>
        public TutorialUnityEvent OnTutorialEnter;

        /// <summary>
        /// Invoked when Tutorial has started.
        /// </summary>
        public TutorialUnityEvent OnTutorialStart;

        /// <summary>
        /// Invoked after Tutorial has ended.
        /// </summary>
        public TutorialUnityEvent OnTutorialEnd;
    }
}