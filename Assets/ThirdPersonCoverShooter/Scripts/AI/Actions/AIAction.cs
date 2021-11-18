using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes and executes an AI action.
    /// </summary>
    [Serializable]
    public abstract class AIAction : ScriptableObject
    {
        /// <summary>
        /// Can the action be targeted at both enemies and allies.
        /// </summary>
        public bool CanTargetAny
        {
            get { return CanTargetEnemy || CanTargetAlly; }
        }

        /// <summary>
        /// Does the action require a target location to be executed.
        /// </summary>
        public bool NeedsTargetLocation
        {
            get { return CanTargetGround; }
        }

        /// <summary>
        /// Can the action include self.
        /// </summary>
        public bool NeedsOnlySelf
        {
            get { return CanTargetSelf && !NeedsSingleTargetActor; }
        }

        /// <summary>
        /// Is the action always targeted at a single actor.
        /// </summary>
        public bool NeedsSingleTargetActor
        {
            get { return CanTargetAny && !CanTargetMultiple && !NeedsTargetLocation; }
        }

        /// <summary>
        /// Time in seconds left to wait before the action can be used again.
        /// </summary>
        public float Wait
        {
            get { return Cooldown - Mathf.Clamp(Time.timeSinceLevelLoad - _executionTime, 0, Cooldown); }
        }

        /// <summary>
        /// Is the action performed automatically by the AI. Actions without any possible targets (like simple movement) won't be performed.
        /// </summary>
        [Tooltip("Is the action performed automatically by the AI. Actions without any possible targets (like simple movement) won't be performed.")]
        public bool Auto;

        /// <summary>
        /// Time in seconds to wait after activation before the action can be performed again.
        /// </summary>
        [Tooltip("Time in seconds to wait after activation before the action can be performed again.")]
        public float Cooldown = 1;

        protected Actor _actor;
        protected Actor _targetActor;
        protected Vector3 _targetPosition;

        private float _executionTime = -10000;

        /// <summary>
        /// Set's the actor that executes the action.
        /// </summary>
        public void SetupActor(Actor actor)
        {
            _actor = actor;
        }

        /// <summary>
        /// Starts the action by the given executor. Returns true if the action was successfully started.
        /// </summary>
        public bool Execute(Actor actor)
        {
            _actor = actor;
            _targetActor = null;

            return Start();
        }

        /// <summary>
        /// Starts the action by the given executor. Action is targeted at a location. Returns true if the action was successfully started.
        /// </summary>
        public bool Execute(Actor actor, Vector3 position)
        {
            _actor = actor;
            _targetActor = null;
            _targetPosition = position;

            return Start();
        }

        /// <summary>
        /// Starts the action by the given executor. Action is targeted at a target actor. Returns true if the action was successfully started.
        /// </summary>
        public bool Execute(Actor actor, Actor target)
        {
            _actor = actor;
            _targetActor = target;

            return Start();
        }

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public virtual bool IsNeededFor(Actor target)
        {
            return false;
        }

        /// <summary>
        /// Catches the animator event during the execution and may perform the action. Returns true if that is the case.
        /// </summary>
        public virtual bool OnFinishAction()
        {
            return false;
        }

        /// <summary>
        /// Catches the animator event during the throw animation and may perform the action. Returns true if that is the case.
        /// </summary>
        public virtual bool OnThrow()
        {
            return false;
        }

        /// <summary>
        /// Set's the cooldown timer.
        /// </summary>
        protected void MarkCooldown()
        {
            _executionTime = Time.timeSinceLevelLoad;
        }

        /// <summary>
        /// Starts the action. Returns true if the action should continue running until finished.
        /// </summary>
        protected abstract bool Start();

        /// <summary>
        /// Updates the action during it's execution. If false is returned the action is stopped.
        /// </summary>
        public abstract bool Update();

        /// <summary>
        /// Stops the action at the end of it's execution.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Will the action require it's executor to move if targeted at the given actor.
        /// </summary>
        public virtual bool WillMoveForActor(Actor target)
        {
            return false;
        }

        /// <summary>
        /// Will the action require it's executor to move if targeted at the given position.
        /// </summary>
        public virtual bool WillMoveForPosition(Vector3 target)
        {
            return false;
        }

        /// <summary>
        /// Can the action target the executor themselves.
        /// </summary>
        public virtual bool CanTargetSelf { get { return false; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public virtual bool CanTargetEnemy { get { return false; } }

        /// <summary>
        /// Can the action target allies of the executor directly.
        /// </summary>
        public virtual bool CanTargetAlly { get { return false; } } 

        /// <summary>
        /// Should the UI ignore dead actors when picking targets.
        /// </summary>
        public virtual bool ShouldIgnoreDead { get { return true; } }

        /// <summary>
        /// Can the action be targeted at a location and not actors.
        /// </summary>
        public virtual bool CanTargetGround { get { return false; } }

        /// <summary>
        /// Can the action target multiple actors at once.
        /// </summary>
        public virtual bool CanTargetMultiple { get { return false; } }

        /// <summary>
        /// Can the action be cancelled if the Timout in AIActions is triggered.
        /// </summary>
        public virtual bool HasNoTimeout { get { return false; } }

        /// <summary>
        /// Radius of the target sphere when the action is displayed in the UI.
        /// </summary>
        public virtual float UIRadius { get { return 0; } }

        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public abstract Color UIColor { get; }
    }
}
