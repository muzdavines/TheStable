using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Adjusts AISight when active.
    /// </summary>
    [RequireComponent(typeof(AISight))]
    public class VisionBuff : BaseBuff
    {
        /// <summary>
        /// View distance multiplier.
        /// </summary>
        [Tooltip("View distance multiplier.")]
        public float Multiplier = 2;

        private AISight _sight;
        private float _applied;
        private float _previous;

        private void Awake()
        {
            _sight = GetComponent<AISight>();
        }

        protected override void Begin()
        {
            _applied = Multiplier;
            _previous = _sight.Distance;
            _sight.Distance *= Multiplier;
        }

        protected override void End()
        {
            if (_applied < -float.Epsilon || _applied > float.Epsilon)
                _sight.Distance /= _applied;
            else
                _sight.Distance = _previous;
        }
    }
}
