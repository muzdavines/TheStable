using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Allows the AI to take phonecalls and film using a phone. Mostly used by Civilian Brain and AI Follow
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(Actor))]
    public class AIPhone : AIItemBase
    {
        #region Private fields

        private Actor _actor;
        private CharacterMotor _motor;

        private bool _isFilming;
        private bool _wantsToCall;

        #endregion

        #region Commands

        /// <summary>
        /// Told by the brains to start filming.
        /// </summary>
        public void ToStartFilming()
        {
            if (isActiveAndEnabled)
                ToTakePhone();

            _isFilming = true;
        }

        /// <summary>
        /// Told by the brains to stop filming.
        /// </summary>
        public void ToStopFilming()
        {
            _isFilming = false;
        }

        /// <summary>
        /// Told by the brains to take a weapon to arms.
        /// </summary>
        public void ToTakePhone()
        {
            
        }

        /// <summary>
        /// Told by the brains to disarm any weapon.
        /// </summary>
        public void ToHidePhone()
        {
            Unequip(_motor, ToolType.phone);
            _isFilming = false;
        }

        /// <summary>
        /// Told by the brains to initiate a call.
        /// </summary>
        public void ToCall()
        {
            ToStopFilming();
            _wantsToCall = Equip(_motor, ToolType.phone);
            if (_wantsToCall) Message("CallResponse");
        }

        /// <summary>
        /// Told by the brains to initiate a phone call.
        /// </summary>
        public void ToPhoneCall()
        {
            ToStopFilming();
            _wantsToCall = Equip(_motor, ToolType.phone);
            if (_wantsToCall) Message("CallResponse");
        }

        #endregion

        #region Events

        /// <summary>
        /// Registers that the character has made a call.
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

            if (_motor.EquippedWeapon.ToolType == ToolType.phone)
            {
                if (_wantsToCall)
                    _motor.InputUseToolAlternate();
                else if (_isFilming)
                    _motor.InputUseTool();
            }
        }

        #endregion
    }
}
