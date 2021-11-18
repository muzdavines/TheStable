using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Allows the AI to use a radio to take calls. Mostly used by AI Backup Call.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(CharacterMotor))]
    public class AIRadio : AIItemBase
    {
        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private bool _wantsToCall;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to take out a radio.
        /// </summary>
        public void ToTakeRadio()
        {
            Equip(_motor, ToolType.radio);
        }

        /// <summary>
        /// Told by the brains to hide radio if it is equipped.
        /// </summary>
        public void ToHideRadio()
        {
            Unequip(_motor, ToolType.radio);
        }

        /// <summary>
        /// Told by the brains to initiate a call.
        /// </summary>
        public void ToCall()
        {
            if (!isActiveAndEnabled)
                return;

            _wantsToCall = Equip(_motor, ToolType.radio);
            if (_wantsToCall) Message("CallResponse");
        }

        /// <summary>
        /// Told by the brains to initiate a radio call. 
        /// </summary>
        public void ToRadioCall()
        {
            if (!isActiveAndEnabled)
                return;

            _wantsToCall = Equip(_motor, ToolType.radio);
            if (_wantsToCall) Message("CallResponse");
        }

        #endregion

        #region Events

        /// <summary>
        /// Registers that a call has been made.
        /// </summary>
        public void OnCallMade()
        {
            _wantsToCall = false;
        }

        #endregion

        #region Behaviour

        protected override void Awake()
        {
            base.Awake();

            _actor = GetComponent<Actor>();
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            if (_wantsToCall && _motor.EquippedWeapon.ToolType == ToolType.radio)
                _motor.InputUseTool();
        }

        #endregion
    }
}
