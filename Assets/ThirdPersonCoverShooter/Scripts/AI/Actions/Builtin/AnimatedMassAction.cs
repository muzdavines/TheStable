using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An action that is performed on everyone matching a criteria, no matter the distance.
    /// </summary>
    [Serializable]
    public abstract class AnimatedMassAction : EffectAction
    {
        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor { get { return Color; } }

        /// <summary>
        /// Can the action target multiple actors at once.
        /// </summary>
        public override bool CanTargetMultiple { get { return true; } }

        /// <summary>
        /// Associated UI color.
        /// </summary>
        [Tooltip("Associated UI color.")]
        public Color Color = Color.white;

        protected CharacterMotor _motor;

        private bool _isAnimating;

        /// <summary>
        /// Starts the action. Returns true if the action should continue running until finished.
        /// </summary>
        protected override bool Start()
        {
            _motor = _actor.GetComponent<CharacterMotor>();
            return true;
        }

        /// <summary>
        /// Updates the action during it's execution. If false is returned the action is stopped.
        /// </summary>
        public override bool Update()
        {
            if (_isAnimating)
                return true;

            _actor.SendMessage("ToStopMoving");

            MarkCooldown();
            _motor.InputProcess(new CharacterProcess(Process, false, false, false));
            _isAnimating = true;

            return true;
        }

        /// <summary>
        /// Stops the action at the end of it's execution.
        /// </summary>
        public override void Stop()
        {
            if (_isAnimating)
            {
                _motor.InputProcessEnd();
                _isAnimating = false;
            }
        }

        /// <summary>
        /// Catches the animator event during the execution and may perform the action. Returns true if that is the case.
        /// </summary>
        public override bool OnFinishAction()
        {
            for (int i = 0; i < Actors.Count; i++)
            {
                var actor = Actors.Get(i);

                var isAlly = actor.Side == _actor.Side;
                var isSelf = actor == _actor;

                if (isSelf && !CanTargetSelf)
                    continue;

                if (actor.isActiveAndEnabled && ((CanTargetAlly && isAlly) || (CanTargetEnemy && !isAlly) || (CanTargetSelf && isSelf)))
                {
                    PlayEffect(actor, actor.transform.position);
                    Perform(actor);
                }
            }

            return true;
        }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected abstract string Process { get; }

        /// <summary>
        /// Performs the action on the specific target. It is called for all valid target actors in the scene.
        /// </summary>
        protected abstract void Perform(Actor target);
    }
}
