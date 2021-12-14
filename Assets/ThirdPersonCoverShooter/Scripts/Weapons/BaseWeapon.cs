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

        public virtual void InitWeapon(MissionCharacter c, Weapon w) {
            myCharacter = c;
            myWeapon = w;

        }

        public Move currentMove;
        public MissionCharacter myCharacter;
        public Weapon myWeapon;
    }
}