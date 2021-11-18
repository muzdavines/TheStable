using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets the AI to attack a target, overrides it's previous target.
    /// </summary>
    [Serializable]
    public class AttackAction : InstantAction
    {
        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor { get { return Color; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public override bool CanTargetEnemy { get { return true; } }

        /// <summary>
        /// Can the action be cancelled if the Timout in AIActions is triggered.
        /// </summary>
        public override bool HasNoTimeout { get { return true; } }

        /// <summary>
        /// Associated UI color.
        /// </summary>
        [Tooltip("Associated UI color.")]
        public Color Color = Color.red;

        public AttackAction()
        {
            Cooldown = 0;
        }

        /// <summary>
        /// Performs the action.
        /// </summary>
        protected override void Perform()
        {
            _actor.SendMessage("ToAttack", _targetActor);
            _actor.SendMessage("ToOverrideThreat", _targetActor);
        }
    }
}
