using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An action that is performed instantly with no delay.
    /// </summary>
    [Serializable]
    public abstract class InstantAction : AIAction
    {
        /// <summary>
        /// Starts the action. Returns true if the action should continue running until finished.
        /// </summary>
        protected override bool Start()
        {
            MarkCooldown();
            Perform();
            return false;
        }

        /// <summary>
        /// Updates the action during it's execution. If false is returned the action is stopped.
        /// </summary>
        public override bool Update()
        {
            return false;
        }

        /// <summary>
        /// Stops the action at the end of it's execution.
        /// </summary>
        public override void Stop()
        {
        }

        /// <summary>
        /// Performs the action.
        /// </summary>
        protected abstract void Perform();
    }
}
