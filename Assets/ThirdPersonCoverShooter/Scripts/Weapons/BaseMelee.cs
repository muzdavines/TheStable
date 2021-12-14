using UnityEngine;

namespace CoverShooter
{
    public enum Limb
    {
        RightHand,
        LeftHand,
        RightLeg,
        LeftLeg
    }

    public abstract class BaseMelee : BaseWeapon
    {
       

        /// <summary>
        /// Owning object with a CharacterMotor component.
        /// </summary>
        [HideInInspector]
        public CharacterMotor Character;
        public CharacterHealth charHealth;
        public MoveWeaponType moveWeaponType = MoveWeaponType.Fists;
        /// <summary>
        /// Return true if an attack can be started.
        /// </summary>
        public abstract bool Request();

        /// <summary>
        /// Start the attack.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// End the attack.
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Start scanning for hits during the attack.
        /// </summary>
        public abstract void BeginScan();
        public abstract void BeginScan(Move _move);
        /// <summary>
        /// Stop scanning for hits during the attack.
        /// </summary>
        
        public abstract void EndScan();

        /// <summary>
        /// A significant moment during a melee attack, used to play effects.
        /// </summary>
        public abstract void Moment();
    }
}