using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Base implementation of all the AI brains. Manages threats.
    /// </summary>
    public class BaseBrain : AIBase
    {
        /// <summary>
        /// Actor component found in the same object.
        /// </summary>
        public Actor Actor
        {
            get { return _actor; }
        }

        /// <summary>
        /// Actor component of the threating character.
        /// </summary>
        public Actor Threat
        {
            get { return _threat; }
        }

        /// <summary>
        /// Threat actor that was told to be forgotten.
        /// </summary>
        public Actor ForgottenThreat
        {
            get { return _forgottenThreat; }
        }

        /// <summary>
        /// Time in seconds since the threat was last seen.
        /// </summary>
        public float ThreatAge
        {
            get { return Time.timeSinceLevelLoad - _lastSeenThreatTime; }
        }

        /// <summary>
        /// Last known position of the assigned threat.
        /// </summary>
        public Vector3 LastKnownThreatPosition
        {
            get { return _lastKnownThreatPosition; }
        }

        /// <summary>
        /// Time since level load that the threat was seen.
        /// </summary>
        public float LastSeenThreatTime
        {
            get { return _lastSeenThreatTime; }
        }

        /// <summary>
        /// Is the threat currently seen.
        /// </summary>
        public bool CanSeeTheThreat
        {
            get { return _canSeeTheThreat && _threat != null && _threat.IsAlive; }
        }

        /// <summary>
        /// Cover that is currently known to be used by the threat.
        /// </summary>
        public Cover ThreatCover
        {
            get { return _threat == null ? null : _threatCover; }
        }

        /// <summary>
        /// Was the last known threat position a position the threat occupied or was it just an indirect explosion.
        /// </summary>
        public bool IsActualThreatPosition
        {
            get { return _isActualThreatPosition && _threat != null; }
        }

        /// <summary>
        /// Was any threat seen at least once.
        /// </summary>
        public bool HasSeenTheEnemy
        {
            get { return _hasSeenTheEnemy && _threat != null; }
        }

        /// <summary>
        /// Was any threat heard at least once.
        /// </summary>
        public bool HasHeardTheEnemy
        {
            get { return _hasHeardTheEnemy && _threat != null; }
        }

        /// <summary>
        /// Location at which the character started.
        /// </summary>
        public Vector3 StartingLocation
        {
            get { return _startingPosition; }
        }

        /// <summary>
        /// Is the AI currently in a dangerous area.
        /// </summary>
        public bool IsInDanger
        {
            get { return _danger > 0; }
        }

        private Actor _actor;
        private Vector3 _startingPosition;

        private Actor _threat;
        private Actor _forgottenThreat;
        private Vector3 _lastKnownThreatPosition;
        private float _lastSeenThreatTime;
        private Cover _threatCover;
        private bool _canSeeTheThreat;
        private bool _isActualThreatPosition;
        private bool _hasSeenTheEnemy;
        private bool _hasHeardTheEnemy;

        private int _danger;

        protected virtual void Awake()
        {
            _actor = GetComponent<Actor>();
            _startingPosition = transform.position;
        }

        /// <summary>
        /// Registers an entrance to a dangerous area.
        /// </summary>
        public void OnEnterDanger()
        {
            _danger++;
        }

        /// <summary>
        /// Registers that the AI is leaving a dangerous area.
        /// </summary>
        public void OnLeaveDanger()
        {
            _danger--;
        }

        /// <summary>
        /// Is told to forget about the current threat.
        /// </summary>
        public virtual void ToForget()
        {
            _forgottenThreat = Threat;
            RemoveThreat();
        }
        
        /// <summary>
        /// Unset the current threat.
        /// </summary>
        protected void RemoveThreat()
        {
            _threat = null;
            _canSeeTheThreat = false;
            Message("OnNoThreat");
        }

        /// <summary>
        /// Sets the threat state to unseen.
        /// </summary>
        protected void UnseeThreat()
        {
            _canSeeTheThreat = false;
        }

        /// <summary>
        /// Updates threat state. Implicitly marks it as an unseen threat.
        /// </summary>
        protected void SetUnseenThreat(bool isHeard, bool isDirect, Actor threat, Vector3 position, Cover threatCover)
        {
            SetThreat(false, isHeard, isDirect, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        /// <summary>
        /// Updates threat state. Implicitly marks it as a visible threat.
        /// </summary>
        protected void SetThreat(Actor threat, Vector3 position, Cover threatCover)
        {
            SetThreat(true, false, true, threat, position, threatCover, Time.timeSinceLevelLoad);
        }

        /// <summary>
        /// Updates threat state. Time is given since level load.
        /// </summary>
        protected void SetThreat(bool isVisible, bool isHeard, bool isActual, Actor threat, Vector3 position, Cover threatCover, float time)
        {
            var lastThreat = _threat;

            _forgottenThreat = null;
            _lastSeenThreatTime = time;
            _lastKnownThreatPosition = position;
            _threatCover = threatCover;
            _threat = threat;
            _canSeeTheThreat = isVisible;
            _isActualThreatPosition = isActual;

            if (isHeard)
                _hasHeardTheEnemy = true;

            if (_canSeeTheThreat)
                _hasSeenTheEnemy = true;

            if (_threat != lastThreat)
            {
                if (_threat != null)
                    Message("OnThreat", _threat);
                else
                    Message("OnNoThreat");
            }

            if (threat != null)
                Message("OnThreatPosition", _lastKnownThreatPosition);
        }
    }
}
