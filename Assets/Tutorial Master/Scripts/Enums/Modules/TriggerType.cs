using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to specify how will ActiveStage go to another stage of the tutorial
    /// </summary>
    [Serializable]
    public enum TriggerType
    {
        /// <summary>
        /// Pressing any key will activate a trigger
        /// </summary>
        AnyKeyPress,

        /// <summary>
        /// Pressing a specific KeyPress will activate a trigger
        /// </summary>
        KeyPress,

        /// <summary>
        /// Pressing a specific Input (legacy input system) will activate a trigger
        /// </summary>
        Input,

        /// <summary>
        /// Whenever a specified UnityEvent gets invoked, a trigger will be activated
        /// </summary>
        UnityEventInvoke,

        /// <summary>
        /// Pressing a UGUI Button will activate a trigger
        /// </summary>
        UGUIButtonClick,

        /// <summary>
        /// Trigger will be activated after a timer runs out
        /// </summary>
        Timer,

        /// <summary>
        /// Nothing will activate a trigger unless it's done manually.
        /// </summary>
        None
    }
}