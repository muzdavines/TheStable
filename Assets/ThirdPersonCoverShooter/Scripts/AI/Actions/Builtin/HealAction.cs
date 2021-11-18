using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Heals a single actor.
    /// </summary>
    [Serializable]
    public class HealAction : AnimatedActorAction
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
        protected override string Process { get { return "GiveHeal"; } }

        /// <summary>
        /// Should the UI ignore dead actors when picking targets.
        /// </summary>
        public override bool ShouldIgnoreDead { get { return false; } }

        /// <summary>
        /// Amount of health restored.
        /// </summary>
        public float Amount = 100;

        /// <summary>
        /// Health value of an ally that triggers the AI to use this action.
        /// </summary>
        [Range(0, 1)]
        public float AutoTriggerHealth = 0.6f;

        /// <summary>
        /// Should an outline be displayed on a target actor upon heal.
        /// </summary>
        public bool Outlines = true;

        /// <summary>
        /// Duration of the outline on a healed actor.
        /// </summary>
        public float OutlineDuration = 1;

        /// <summary>
        /// Will the AI only consider helping allies under cover.
        /// </summary>
        [Tooltip("Will the AI only consider helping allies under cover.")]
        public bool AutoHelpOnlyCovered = false;

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public override bool IsNeededFor(Actor target)
        {
            var health = target.GetComponent<CharacterHealth>();
            return health != null && (!target.IsAlive || (health.Health < health.MaxHealth * AutoTriggerHealth)) && (!AutoHelpOnlyCovered || target.Cover != null);
        }

        /// <summary>
        /// Performs the action on target actor specified earlier.
        /// </summary>
        protected override void Perform()
        {
            if (!_targetMotor.IsAlive)
                _targetMotor.InputResurrect();

            _targetMotor.SendMessage("Heal", Amount);

            if (Outlines)
            {
                var outline = _targetActor.gameObject.AddComponent<TempOutline>();
                outline.OutlineColor = Color;
                outline.Duration = OutlineDuration;
                outline.Launch();
            }
        }
    }
}
