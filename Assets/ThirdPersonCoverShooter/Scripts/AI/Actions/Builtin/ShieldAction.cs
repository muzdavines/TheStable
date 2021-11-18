using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Enables the target object. The target is regarded to be a shield by the AI.
    /// </summary>
    [Serializable]
    public class ShieldAction : InstantAction
    {
        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor { get { return Color; } }

        /// <summary>
        /// Can the action target the executor themselves.
        /// </summary>
        public override bool CanTargetSelf { get { return true; } }

        /// <summary>
        /// Shield object to be activated.
        /// </summary>
        [Tooltip("Shield object to be activated.")]
        public GameObject Shield;

        /// <summary>
        /// Associated UI color.
        /// </summary>
        [Tooltip("Associated UI color.")]
        public Color Color = Color.blue;

        public ShieldAction()
        {
            Cooldown = 12;
        }

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public override bool IsNeededFor(Actor target)
        {
            return target == _actor && _actor.Threat != null && _actor.Threat.IsAlive;
        }

        /// <summary>
        /// Performs the action.
        /// </summary>
        protected override void Perform()
        {
            if (Shield != null)
            {
                if (Shield.activeSelf)
                    Shield.SetActive(false);

                Shield.SetActive(true);
            }
        }
    }
}
