using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    [DataValidator(typeof(ArrowModuleSettingsValidator))]
    public class ArrowModuleSettings : ModuleSettings
    {
        /// <summary>
        /// The point direction from which Arrow Module should be pointing from
        /// </summary>
        public PointDirection PointDirection;

        /// <summary>
        /// RectTransform of a target which it will be pointing at (UI Element)
        /// </summary>
        public Transform PointTarget;
    }
}