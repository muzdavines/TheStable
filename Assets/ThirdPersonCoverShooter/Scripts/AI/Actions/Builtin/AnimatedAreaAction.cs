using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An action that is performed on an area.
    /// </summary>
    [Serializable]
    public abstract class AnimatedAreaAction : EffectAction
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
        /// Can the action be targeted at a location and not actors.
        /// </summary>
        public override bool CanTargetGround { get { return true; } }

        /// <summary>
        /// Radius of the target sphere when the action is displayed in the UI.
        /// </summary>
        public override float UIRadius { get { return Radius; } }

        /// <summary>
        /// Associated UI color.
        /// </summary>
        [Tooltip("Associated UI color.")]
        public Color Color = Color.white;

        /// <summary>
        /// Distance to the target at which the action is performed.
        /// </summary>
        [Tooltip("Distance to the target at which the action is performed.")]
        public float Distance = 1;

        /// <summary>
        /// Effect radius.
        /// </summary>
        [Tooltip("Effect radius.")]
        public float Radius = 6;

        protected CharacterMotor _motor;

        private bool _isAnimating;
        private bool _isMoving;

        /// <summary>
        /// Starts the action. Returns true if the action should continue running until finished.
        /// </summary>
        protected override bool Start()
        {
            _motor = _actor.GetComponent<CharacterMotor>();
            _isMoving = false;
            return true;
        }

        /// <summary>
        /// Will the action require it's executor to move if targeted at the given position.
        /// </summary>
        public override bool WillMoveForPosition(Vector3 target)
        {
            return Vector3.Distance(_actor.transform.position, target) > Distance + Radius * 0.75f;
        }

        /// <summary>
        /// Updates the action during it's execution. If false is returned the action is stopped.
        /// </summary>
        public override bool Update()
        {
            if (_isAnimating)
                return true;

            if (Vector3.Distance(_actor.transform.position, _targetPosition) < Distance)
            {
                _actor.SendMessage("ToStopMoving");

                MarkCooldown();
                _motor.InputProcess(new CharacterProcess(Process, false, false, false));
                _isAnimating = true;
            }
            else if (!_isMoving)
            {
                _isMoving = true;
                _actor.SendMessage("ToRunTo", _targetPosition);
            }

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
            var count = AIUtil.FindActorsIncludingDead(_targetPosition, Radius);

            for (int i = 0; i < count; i++)
            {
                PlayEffect(AIUtil.Actors[i], AIUtil.Actors[i].transform.position);
                Perform(AIUtil.Actors[i]);
            }

            return true;
        }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected abstract string Process { get; }

        /// <summary>
        /// Performs the action on the specific target. It is called for all target actors near the location.
        /// </summary>
        protected abstract void Perform(Actor target);
    }
}
