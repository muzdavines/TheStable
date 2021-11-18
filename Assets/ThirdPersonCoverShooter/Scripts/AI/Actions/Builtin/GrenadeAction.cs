using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Throws a grenade.
    /// </summary>
    [Serializable]
    public class GrenadeAction : AIAction
    {
        /// <summary>
        /// Can the action be targeted at a location and not actors.
        /// </summary>
        public override bool CanTargetGround { get { return true; } }

        /// <summary>
        /// Can the action target enemies of the executor directly.
        /// </summary>
        public override bool CanTargetEnemy { get { return true; } }

        /// <summary>
        /// Radius of the target sphere when the action is displayed in the UI.
        /// </summary>
        public override float UIRadius { get { return Grenade != null ? Grenade.ExplosionRadius : 0; } }

        /// <summary>
        /// Will the action require it's executor to move if targeted at the given position.
        /// </summary>
        public override bool WillMoveForPosition(Vector3 target)
        {
            var grenade = Grenade;
            if (grenade == null)
                return false;

            return Vector3.Distance(_actor.transform.position, target) > Distance + grenade.ExplosionRadius * 0.75f;
        }

        /// <summary>
        /// Color of the action when presented in the UI.
        /// </summary>
        public override Color UIColor
        {
            get
            {
                if (AlwaysUseDefaultColor)
                    return DefaultColor;

                var grenade = Grenade;
                if (grenade == null)
                    return DefaultColor;

                var preview = grenade.ExplosionPreview;
                if (preview == null)
                    return DefaultColor;

                var renderer = preview.GetComponent<Renderer>();
                if (renderer == null) renderer = preview.GetComponentInChildren<Renderer>();
                if (renderer == null)
                    return DefaultColor;

                var color = renderer.material.color;
                color.a = 1;

                return color;
            }
        }

        /// <summary>
        /// Potential grenade that will be thrown.
        /// </summary>
        public Grenade Grenade
        {
            get
            {
                if (Prefab != null)
                    return Prefab;
                else
                {
                    var motor = _motor;

                    if (motor == null && _actor != null)
                        motor = _actor.GetComponent<CharacterMotor>();

                    if (motor != null)
                        return motor.PotentialGrenade;
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Associated UI color. Used if there is no preview explosion color on the grenade.
        /// </summary>
        [Tooltip("Associated UI color. Used if there is no preview explosion color on the grenade.")]
        public Color DefaultColor = Color.red;

        /// <summary>
        /// Should the UI color always be set to default instead of looking for one inside the grenade prefab.
        /// </summary>
        [Tooltip("Should the UI color always be set to default instead of looking for one inside the grenade prefab.")]
        public bool AlwaysUseDefaultColor = false;

        /// <summary>
        /// Throw distance.
        /// </summary>
        public float Distance = 10;

        /// <summary>
        /// Grenade that will be copied and thrown.
        /// </summary>
        public Grenade Prefab;

        protected CharacterMotor _motor;

        private bool _isAnimating;
        private bool _isMoving;

        /// <summary>
        /// Should the AI perform the action on the given target. 
        /// </summary>
        public override bool IsNeededFor(Actor target)
        {
            var grenade = Grenade;

            if (grenade is ToxicGrenade)
            {
                var buff = target.GetComponent<DamageBuff>();
                if (buff != null && buff.enabled)
                    return buff.Multiplier > 1;
                else
                    return true;
            }
            else if (grenade is SmokeGrenade)
            {
                var buff = target.GetComponent<VisionBuff>();
                if (buff != null && buff.enabled)
                    return buff.Multiplier > 1;
                else
                    return true;
            }
            else
                return true;
        }

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
        /// Catches the animator event during the throw animation and may perform the action. Returns true if that is the case.
        /// </summary>
        public override bool OnThrow()
        {
            return true;
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
                _motor.InputThrowGrenade(_targetPosition, Prefab != null ? Prefab.gameObject : null);
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
                _isAnimating = false;
        }
    }
}
