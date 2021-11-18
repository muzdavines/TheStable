using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Applies ArmorBuff to everyone matching a certain criteria, no matter the distance.
    /// </summary>
    [Serializable]
    public class MassArmorBuffAction : AnimatedMassAction
    {
        /// <summary>
        /// Can the action target allies of the executor directly.
        /// </summary>
        public override bool CanTargetAlly { get { return Multiplier < 1; } }

        /// <summary>
        /// Can the action target the executor themselves.
        /// </summary>
        public override bool CanTargetSelf { get { return Multiplier < 1; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public override bool CanTargetEnemy { get { return Multiplier > 1; } }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected override string Process { get { return "MassSupport"; } }

        /// <summary>
        /// Multiplier of the damage received.
        /// </summary>
        public float Multiplier = 0.5f;

        /// <summary>
        /// Duration of the buff.
        /// </summary>
        public float Duration = 6;

        /// <summary>
        /// Should an outline be displayed on an actor with the buff.
        /// </summary>
        public bool Outline = true;

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
            var buff = target.GetComponent<ArmorBuff>();
            if (buff != null && buff.enabled) return false;

            var health = target.GetComponent<CharacterHealth>();
            return health != null && health.Health < health.MaxHealth * AutoTriggerHealth;
        }

        /// <summary>
        /// Performs the action on the specific target. It is called for all valid target actors in the scene.
        /// </summary>
        protected override void Perform(Actor target)
        {
            var buff = target.GetComponent<ArmorBuff>();

            if (buff == null || buff.enabled)
                buff = target.gameObject.AddComponent<ArmorBuff>();

            buff.Duration = Duration;
            buff.Multiplier = Multiplier;
            buff.Outline = Outline;
            buff.OutlineColor = Color;
            buff.Launch();
        }
    }
}
