using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Heals everyone matching a certain criteria, no matter the distance.
    /// </summary>
    [Serializable]
    public class MassHealAction : AnimatedMassAction
    {
        /// <summary>
        /// Can the action target allies of the executor directly.
        /// </summary>
        public override bool CanTargetAlly { get { return true; } }

        /// <summary>
        /// Can the action target the executor themselves.
        /// </summary>
        public override bool CanTargetSelf { get { return true; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public override bool CanTargetEnemy { get { return false; } }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected override string Process { get { return "MassSupport"; } }

        /// <summary>
        /// Health restored on each target actor.
        /// </summary>
        public float Amount = 100;

        /// <summary>
        /// Should an outline be displayed on a target actor upon heal.
        /// </summary>
        public bool Outlines = true;

        /// <summary>
        /// Duration of the outline on a healed actor.
        /// </summary>
        public float OutlineDuration = 1;

        /// <summary>
        /// Health value of an ally that triggers the AI to use this action.
        /// </summary>
        [Range(0, 1)]
        public float AutoTriggerHealth = 0.6f;

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public override bool IsNeededFor(Actor target)
        {
            var health = target.GetComponent<CharacterHealth>();
            return health != null && (!target.IsAlive || (health.Health < health.MaxHealth * AutoTriggerHealth));
        }

        /// <summary>
        /// Performs the action on the specific target. It is called for all valid target actors in the scene.
        /// </summary>
        protected override void Perform(Actor target)
        {
            if (target.Side != _actor.Side)
                return;

            var motor = target.GetComponent<CharacterMotor>();

            if (!motor.IsAlive)
                motor.InputResurrect();

            motor.SendMessage("Heal", Amount);

            if (Outlines)
            {
                var outline = target.gameObject.AddComponent<TempOutline>();
                outline.OutlineColor = Color;
                outline.Duration = OutlineDuration;
                outline.Launch();
            }
        }
    }
}
