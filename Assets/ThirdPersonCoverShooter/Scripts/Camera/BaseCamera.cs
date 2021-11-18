using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// A camera that is pointed at a CharacterMotor.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class BaseCamera : CharacterCamera
    {
        /// <summary>
        /// Target character motor.
        /// </summary>
        [Tooltip("Target character motor.")]
        public CharacterMotor Target;

        /// <summary>
        /// Executed on every camera target change.
        /// </summary>
        public Action TargetChanged;

        private CharacterMotor _cachedTarget;

        private MonoBehaviour _targetComponent;

        /// <summary>
        /// Get cached component of the target object.
        /// </summary>
        public T TargetComponent<T>() where T : MonoBehaviour
        {
            if (_cachedTarget != Target)
                updateTarget();

            if (Target == null)
                return null;

            if (_targetComponent == null || !(_targetComponent is T))
                _targetComponent = Target != null ? Target.GetComponent<T>() : null;

            return _targetComponent as T;
        }

        protected virtual void Update()
        {
            if (Target != _cachedTarget)
                updateTarget();
        }

        private void updateTarget()
        {
            _cachedTarget = Target;
            _targetComponent = null;

            SendMessage("OnTargetChange", SendMessageOptions.DontRequireReceiver);
            if (TargetChanged != null) TargetChanged.Invoke();
        }

        /// <summary>
        /// Asks the camera to call UpdateAfterCamera on the given controller after the camera does it's update.
        /// </summary>
        public override void DeferUpdate(ICharacterController controller)
        {
        }

        /// <summary>
        /// Update performed after the character motor does it's thing.
        /// </summary>
        public override void UpdateForCharacterMotor()
        {
        }
    }
}
