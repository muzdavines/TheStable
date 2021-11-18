using UnityEngine;

namespace CoverShooter
{
    public enum HitType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        Fist,
        Machete,
        PistolMelee,
        RifleMelee,
        ShotgunMelee,
        SniperMelee,
        Explosion,
        Leg
    }

    /// <summary>
    /// Description of a bullet hit. Used when passed to OnHit events.
    /// </summary>
    public struct Hit
    {
        public bool IsMelee
        {
            get
            {
                switch (Type)
                {
                    case HitType.Fist:
                    case HitType.Machete:
                    case HitType.PistolMelee:
                    case HitType.RifleMelee:
                    case HitType.ShotgunMelee:
                    case HitType.SniperMelee:
                    
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Position of the hit in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Normal of the hit in world space. Faces outwards from the hit object.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Damage dealt to the impacted object.
        /// </summary>
        public float Damage;

        /// <summary>
        /// Owner of the weapon that caused the hit.
        /// </summary>
        public GameObject Attacker;

        /// <summary>
        /// Object that was hit.
        /// </summary>
        public GameObject Target;

        /// <summary>
        /// Type of the damage.
        /// </summary>
        public HitType Type;

        /// <summary>
        /// Time in seconds between hits that the character will respond to with hurt animations.
        /// </summary>
        public float ReactionDelay;

        public Move move;

        /// <summary>
        /// Create a bullet hit description.
        /// </summary>
        /// <param name="position">Position of the hit in world space.</param>
        /// <param name="normal">Normal of the hit in world space. Faces outwards from the hit object.</param>
        /// <param name="damage">Damage dealt to the impacted object.</param>
        /// <param name="attacker">Owner of the weapon that caused the hit.</param>
        /// <param name="target">Object that was hit.</param>
        /// <param name="type">Type of the damage dealt.</param>
        /// <param name="reactionDelay">Time in seconds between hits that the character will respond to with hurt animations.</param>
        public Hit(Vector3 position, Vector3 normal, float damage, GameObject attacker, GameObject target, HitType type, float reactionDelay, Move thisMove = null)
        {
            Position = position;
            Normal = normal;
            Damage = damage;
            Attacker = attacker;
            Target = target;
            Type = type;
            ReactionDelay = reactionDelay;
            move = thisMove;
        }
    }
}