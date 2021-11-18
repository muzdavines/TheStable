using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Base implementation for all buffs. Manages timed disable and outlines.
    /// </summary>
    public abstract class BaseBuff : MonoBehaviour
    {
        /// <summary>
        /// Duration of the buff.
        /// </summary>
        [Tooltip("Duration of the buff.")]
        public float Duration = 1;

        /// <summary>
        /// Should an outline be displayed during the buff.
        /// </summary>
        [Tooltip("Should an outline be displayed during the buff.")]
        public bool Outline = false;

        /// <summary>
        /// Color displayed on the outline.
        /// </summary>
        [Tooltip("Color displayed on the outline.")]
        public Color OutlineColor = Color.white;

        /// <summary>
        /// Should the buff be auto-launched on awake.
        /// </summary>
        [Tooltip("Should the buff be auto-launched on awake.")]
        public bool AutoLaunch = false;

        private CharacterOutline _outline;
        private bool _isRunning;
        private float _timer;

        protected virtual void Update()
        {
            if (!_isRunning)
                return;

            _timer -= Time.deltaTime;
            if (_timer <= float.Epsilon)
                enabled = false;
        }

        /// <summary>
        /// Starts the buff and timer.
        /// </summary>
        public void Launch()
        {
            if (_isRunning)
                return;

            if (Outline)
            {
                if (_outline == null)
                {
                    _outline = GetComponent<CharacterOutline>();

                    if (_outline == null)
                    {
                        _outline = gameObject.AddComponent<CharacterOutline>();
                        _outline.DisplayDefault = false;
                    }
                }

                _outline.PushColor(this, OutlineColor);
            }

            Begin();
            _timer = Duration;
            _isRunning = true;
            enabled = true;
        }

        private void OnEnable()
        {
            if (AutoLaunch)
                Launch();
        }

        private void OnDisable()
        {
            if (_isRunning)
            {
                _isRunning = false;

                if (_outline != null)
                    _outline.PopColor(this);

                End();
            }
        }

        /// <summary>
        /// Enable the buff.
        /// </summary>
        protected abstract void Begin();

        /// <summary>
        /// Disable the buff.
        /// </summary>
        protected abstract void End();
    }
}
