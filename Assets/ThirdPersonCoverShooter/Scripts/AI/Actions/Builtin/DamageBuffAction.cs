using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Applies a DamageBuff to a target actor.
    /// </summary>
    [Serializable]
    public class DamageBuffAction : AnimatedActorAction
    {
        /// <summary>
        /// Can the action target allies of the executor directly.
        /// </summary>
        public override bool CanTargetAlly { get { return Multiplier > 1; } }

        /// <summary>
        /// Can the action target the executor themselves.
        /// </summary>
        public override bool CanTargetSelf { get { return Multiplier > 1; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public override bool CanTargetEnemy { get { return Multiplier < 1; } }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected override string Process { get { return "Support"; } }

        /// <summary>
        /// Damage multiplier.
        /// </summary>
        public float Multiplier = 2;

        /// <summary>
        /// Duration of the buff.
        /// </summary>
        public float Duration = 6;

        /// <summary>
        /// Should an outline be displayed on actors with the buff active.
        /// </summary>
        public bool Outlines = true;

        /// <summary>
        /// Will the AI only consider helping allies under cover.
        /// </summary>
        [Tooltip("Will the AI only consider helping allies under cover.")]
        public bool AutoHelpOnlyCovered = true;

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public override bool IsNeededFor(Actor target)
        {
            var buff = target.GetComponent<DamageBuff>();
            if (buff != null && buff.enabled) return false;
            return target.Threat != null && target.Threat.IsAlive && (!AutoHelpOnlyCovered || target.Cover != null);
        }

        /// <summary>
        /// Performs the action on target actor specified earlier.
        /// </summary>
        protected override void Perform()
        {
            var buff = _targetActor.GetComponent<DamageBuff>();

            if (buff == null || buff.enabled)
                buff = _targetActor.gameObject.AddComponent<DamageBuff>();

            buff.Duration = Duration;
            buff.Multiplier = Multiplier;
            buff.Outline = Outlines;
            buff.OutlineColor = Color;
            buff.Launch();
        }
    }
}
