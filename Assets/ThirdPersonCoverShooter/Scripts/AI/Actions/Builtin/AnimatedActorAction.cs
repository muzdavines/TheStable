using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// An action that moves the actor towards the target and executes an animation.
    /// </summary>
    [Serializable]
    public abstract class AnimatedActorAction : EffectAction
    {
        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor { get { return Color; } }

        /// <summary>
        /// Will the action require it's executor to move if targeted at the given actor.
        /// </summary>
        public override bool WillMoveForActor(Actor target)
        {
            return Vector3.Distance(_actor.transform.position, target.transform.position) > Distance;
        }

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

        protected CharacterMotor _motor;
        protected CharacterMotor _targetMotor;

        private bool _isAnimating;
        private bool _hasMoveTarget;
        private Vector3 _moveTarget;

        private bool _enteredTargetIntoAProcess = false;

        /// <summary>
        /// Does the animation stop the targeted actor.
        /// </summary>
        protected bool StopsTargetActor
        {
            get { return false; }
        }

        /// <summary>
        /// Starts the action. Returns true if the action should continue running until finished.
        /// </summary>
        protected override bool Start()
        {
            _motor = _actor.GetComponent<CharacterMotor>();
            _targetMotor = _targetActor.GetComponent<CharacterMotor>();

            _actor.SendMessage("ToDisarm");

            if (_targetActor.Side == _actor.Side && StopsTargetActor)
            {
                _enteredTargetIntoAProcess = true;
                _targetActor.SendMessage("ToEnterProcess", true);
                _targetActor.SendMessage("ToStopMoving");
            }

            _hasMoveTarget = false;
            return true;
        }

        /// <summary>
        /// Updates the action during it's execution. If false is returned the action is stopped.
        /// </summary>
        public override bool Update()
        {
            if (_isAnimating)
                return true;

            if (Vector3.Distance(_actor.transform.position, _targetActor.transform.position) < Distance)
            {
                _actor.SendMessage("ToStopMoving");

                if (StopsTargetActor)
                    _targetMotor.InputProcess(new CharacterProcess(null, true, false, false));

                MarkCooldown();
                _motor.InputProcess(new CharacterProcess(Process, false, false, false));
                _isAnimating = true;
            }
            else
            {
                if (_hasMoveTarget && Vector3.Distance(_moveTarget, _targetActor.transform.position) > Distance)
                    _hasMoveTarget = false;

                if (!_hasMoveTarget)
                {
                    _hasMoveTarget = true;
                    _moveTarget = _targetActor.transform.position;
                    _actor.SendMessage("ToRunTo", _moveTarget);
                }
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

            if (StopsTargetActor)
                _targetMotor.InputProcessEnd();

            if (_enteredTargetIntoAProcess)
            {
                _enteredTargetIntoAProcess = false;
                _targetActor.SendMessage("ToExitProcess");
            }
        }

        /// <summary>
        /// Catches the animator event during the execution and may perform the action. Returns true if that is the case.
        /// </summary>
        public override bool OnFinishAction()
        {
            PlayEffect(_targetActor, _targetActor.transform.position);
            Perform();
            return true;
        }

        /// <summary>
        /// Name of the process given to the character motor.
        /// </summary>
        protected abstract string Process { get; }

        /// <summary>
        /// Performs the action on target actor specified earlier.
        /// </summary>
        protected abstract void Perform();
    }
}
