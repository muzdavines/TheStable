using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Tells the AI to stay at a position no matter what.
    /// </summary>
    [Serializable]
    public class HoldPositionAction : InstantAction
    {
        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor { get { return Color; } }

        /// <summary>
        /// Can the action be targeted at a location and not actors.
        /// </summary>
        public override bool CanTargetGround { get { return true; } }

        /// <summary>
        /// Can the action be cancelled if the Timout in AIActions is triggered.
        /// </summary>
        public override bool HasNoTimeout { get { return true; } }

        /// <summary>
        /// Associated UI color.
        /// </summary>
        [Tooltip("Associated UI color.")]
        public Color Color = Color.white;

        public HoldPositionAction()
        {
            Cooldown = 0;
        }

        /// <summary>
        /// Performs the action.
        /// </summary>
        protected override void Perform()
        {
            _actor.SendMessage("ToHoldPosition", _targetPosition);
        }
    }
}
