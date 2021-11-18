using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CoverShooter {
    public abstract class BaseWeapon : MonoBehaviour {
        /// <summary>
        /// Animations and related assets to be used with this weapon.
        /// </summary>
        [Tooltip("Animations and related assets to be used with this weapon.")]
        public WeaponType Type = WeaponType.Pistol;

        /// <summary>
        /// Damage done by a melee attack.
        /// </summary>
        [Tooltip("Damage done by an attack.")]
        public float Damage = 20;

        public virtual void InitWeapon(MissionCharacter c) {

        }

        public Move currentMove;
    }
}