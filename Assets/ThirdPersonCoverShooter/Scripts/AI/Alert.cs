using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates alerts.
    /// </summary>
    public class Alert : MonoBehaviour
    {
        /// <summary>
        /// Range of the alert.
        /// </summary>
        [Tooltip("Range of the alert.")]
        public float Range = 20;

        /// <summary>
        /// Should the alert be activate when enabling the object.
        /// </summary>
        [Tooltip("Should the alert be activate when enabling the object.")]
        public bool AutoActivate = true;

        /// <summary>
        /// Is threat regarded as hostile by civilians.
        /// </summary>
        [Tooltip("Is threat regarded as hostile by civilians.")]
        public bool IsHostile;

        [HideInInspector]
        public Actor Generator;

        private Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        /// <summary>
        /// Activates the alert and resets the timer.
        /// </summary>
        public void Activate()
        {
            Alerts.Broadcast(transform.position, Range, IsHostile, _actor == null ? Generator : _actor, _actor != null);
        }

        private void OnEnable()
        {
            if (AutoActivate)
                Activate();
        }
    }

    /// <summary>
    /// Describes an alert to be picked up by an AI (AIListener). Usually treated as a sound.
    /// </summary>
    public struct GeneratedAlert
    {
        /// <summary>
        /// Position of the alert.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Range of the alert.
        /// </summary>
        public float Range;

        /// <summary>
        /// Is threat regarded as hostile by civilians.
        /// </summary>
        public bool IsHostile;

        /// <summary>
        /// Object that generated the alert.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// Is the actor at the position of the alert.
        /// </summary>
        public bool IsDirect;

        public GeneratedAlert(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            Position = position;
            Range = range;
            IsHostile = isHostile;
            Actor = actor;
            IsDirect = isDirect;
        }
    }

    public static class Alerts
    {
        /// <summary>
        /// Broadcasts an alert to all AIListener components in the area surrounding it.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, bool isHostile, Actor actor, bool isDirect)
        {
            if (range <= float.Epsilon)
                return;

            var alert = new GeneratedAlert(position, range, isHostile, actor, isDirect);
            var count = Physics.OverlapSphereNonAlloc(position, range, Util.Colliders, Layers.Character, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var listener = AIListeners.Get(Util.Colliders[i].gameObject);

                if (listener != null && listener.isActiveAndEnabled && Vector3.Distance(listener.transform.position, position) < range * listener.Hearing)
                    listener.Hear(ref alert);
            }
        }
    }
}
