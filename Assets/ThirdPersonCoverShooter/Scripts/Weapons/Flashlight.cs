using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Turns on or off Unity's Light component.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class Flashlight : Tool
    {
        /// <summary>
        /// Is the flashlight turned on.
        /// </summary>
        public bool IsTurnedOn
        {
            get { return _isOn || _light.enabled; }
        }

        private bool _isOn;
        private Light _light;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        public override void ContinuousUse(CharacterMotor character, bool isAlternate)
        {
            _isOn = true;
        }

        private void Update()
        {
            _light.enabled = _isOn;
            _isOn = false;
        }
    }
}
