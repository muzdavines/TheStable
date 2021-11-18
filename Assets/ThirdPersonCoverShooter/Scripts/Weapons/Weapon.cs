using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CoverShooter
{
    /// <summary>
    /// Defines character animations used with a weapon.
    /// </summary>
    public enum WeaponType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        Fist,
        Machete
    }

    /// <summary>
    /// Weapon aiming setting.
    /// </summary>
    public enum WeaponAiming
    {
        /// <summary>
        /// Wait for controller input to aim.
        /// </summary>
        input,

        /// <summary>
        /// Always point the gun (if not in cover).
        /// </summary>
        always,

        /// <summary>
        /// Always point the gun (if not in cover) and turn immediately.
        /// </summary>
        alwaysImmediateTurn
    }

    /// <summary>
    /// Description of a weapon/tool held by a CharacterMotor. 
    /// </summary>
    [Serializable]
    public struct WeaponDescription
    {
        /// <summary>
        /// True if Item is null.
        /// </summary>
        public bool IsNull
        {
            get { return RightItem == null; }
        }

        /// <summary>
        /// Link to the right hand weapon.
        /// </summary>
        [Tooltip("Link to the right hand weapon.")]
        [FormerlySerializedAs("Item")]
        public GameObject RightItem;

        /// <summary>
        /// Link to the left hand weapon.
        /// </summary>
        [Tooltip("Link to the left hand weapon.")]
        public GameObject LeftItem;

        public GameObject RightLegItem, LeftLegItem;

        /// <summary>
        /// Link to the holstered right hand weapon object which is made visible when the weapon is not used.
        /// </summary>
        [Tooltip("Link to the holstered right hand weapon object which is made visible when the weapon is not used.")]
        [FormerlySerializedAs("Holster")]
        public GameObject RightHolster;

        /// <summary>
        /// Link to the holstered left hand weapon object which is made visible when the weapon is not used.
        /// </summary>
        [Tooltip("Link to the holstered left hand weapon object which is made visible when the weapon is not used.")]
        public GameObject LeftHolster;

        /// <summary>
        /// Link to the flashlight attached to the weapon.
        /// </summary>
        public Flashlight Flashlight
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedFlashlight;
                else
                {
                    cache();
                    return _cachedFlashlight;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the gun component of the Item.
        /// </summary>
        public BaseGun Gun
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedGun;
                else
                {
                    cache();
                    return _cachedGun;
                }
            }
        }

        public BaseMelee RightLegMelee {
            get {
                if (_cacheItem == RightLegItem)
                    return _cachedRightLegMelee;
                else {
                    cache();
                    return _cachedRightLegMelee;
                }
            }
        }

        public BaseMelee LeftLegMelee {
            get {
                if (_cacheItem == LeftLegItem)
                    return _cachedLeftLegMelee;
                else {
                    cache();
                    return _cachedLeftLegMelee;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the melee component of the Item.
        /// </summary>
        public BaseMelee RightMelee
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedRightMelee;
                else
                {
                    cache();
                    return _cachedRightMelee;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the melee component of the Item.
        /// </summary>
        public BaseMelee LeftMelee
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedLeftMelee;
                else
                {
                    cache();
                    return _cachedLeftMelee;
                }
            }
        }

        /// <summary>
        /// Does the weapon have a melee component on either left or right hand.
        /// </summary>
        public bool HasMelee
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedRightMelee != null || _cachedLeftMelee != null || _cachedRightLegMelee != null || _cachedLeftLegMelee != null;
                else
                {
                    cache();
                    return _cachedRightMelee != null || _cachedLeftMelee != null || _cachedRightLegMelee != null || _cachedLeftLegMelee != null;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the Phone component of the Item.
        /// </summary>
        public Phone Phone
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedPhone;
                else
                {
                    cache();
                    return _cachedPhone;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting the Radio component of the Item.
        /// </summary>
        public Radio Radio
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedRadio;
                else
                {
                    cache();
                    return _cachedRadio;
                }
            }
        }

        /// <summary>
        /// Logical type of the tool attached.
        /// </summary>
        public ToolType ToolType
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedToolType;
                else
                {
                    cache();
                    return _cachedToolType;
                }
            }
        }

        /// <summary>
        /// First attached tool to the object.
        /// </summary>
        public Tool Tool
        {
            get
            {
                if (_cacheItem == RightItem)
                    return _cachedTool;
                else
                {
                    cache();
                    return _cachedTool;
                }
            }
        }

        /// <summary>
        /// Shortcut for getting a custom component attached to the item. The value is cached for efficiency.
        /// </summary>
        public T Component<T>() where T : MonoBehaviour
        {
            if (_cacheItem != RightItem)
                cache();

            if (RightItem == null)
                return null;

            if (_cachedComponent == null || !(_cachedComponent is T))
                _cachedComponent = RightItem.GetComponent<T>();

            return _cachedComponent as T;
        }

        /// <summary>
        /// Shield that is enabled when the weapon is equipped.
        /// </summary>
        [Tooltip("Shield that is enabled when the weapon is equipped.")]
        public GameObject Shield;

        /// <summary>
        /// Should the right and left weapons be swapped when mirroring.
        /// </summary>
        [Tooltip("Should the right and left weapons be swapped when mirroring.")]
        public bool PreferSwapping;

        /// <summary>
        /// Should the character equip both hands if possible.
        /// </summary>
        [Tooltip("Should the character equip both hands if possible.")]
        public bool IsDualWielding;

        /// <summary>
        /// Will the character be prevented from running, rolling, or jumping while the weapon is equipped.
        /// </summary>
        [Tooltip("Will the character be prevented from running, rolling, or jumping while the weapon is equipped.")]
        public bool IsHeavy;

        /// <summary>
        /// Will the character use covers while using the weapon.
        /// </summary>
        [Tooltip("Will the character be prevented from using covers while the weapon is equipped.")]
        public bool PreventCovers;

        /// <summary>
        /// Will the character be prevented from climbing while the weapon is equipped.
        /// </summary>
        [Tooltip("Will the character be prevented from climbing while the weapon is equipped.")]
        public bool PreventClimbing;

        /// <summary>
        /// Is the character prevented from lowering arms when standing too close to a wall.
        /// </summary>
        [Tooltip("Is the character prevented from lowering arms when standing too close to a wall.")]
        public bool PreventArmLowering;

        /// <summary>
        /// Is the character always aiming while the weapon is equipped.
        /// </summary>
        [Tooltip("Is the character always aiming while the weapon is equipped.")]
        public WeaponAiming Aiming;

        private BaseGun _cachedGun;
        private BaseMelee _cachedRightMelee;
        private BaseMelee _cachedLeftMelee;
        private BaseMelee _cachedRightLegMelee;
        private BaseMelee _cachedLeftLegMelee;
        private MonoBehaviour _cachedComponent;

        private Phone _cachedPhone;
        private Radio _cachedRadio;
        private Flashlight _cachedFlashlight;
        private Tool _cachedTool;
        private ToolType _cachedToolType;

        private GameObject _cacheItem;

        private void cache()
        {
            _cacheItem = RightItem;
            _cachedComponent = null;
            _cachedGun = RightItem == null ? null : RightItem.GetComponent<BaseGun>();
            _cachedRightMelee = RightItem == null ? null : RightItem.GetComponent<BaseMelee>();
            _cachedLeftMelee = LeftItem == null ? null : LeftItem.GetComponent<BaseMelee>();
            _cachedLeftLegMelee = LeftLegItem == null ? null : LeftLegItem.GetComponent<BaseMelee>();
            _cachedRightLegMelee = RightLegItem == null ? null : RightLegItem.GetComponent<BaseMelee>();
            _cachedTool = RightItem == null ? null : RightItem.GetComponent<Tool>();
            _cachedPhone = RightItem == null ? null : RightItem.GetComponent<Phone>();
            _cachedRadio = RightItem == null ? null : RightItem.GetComponent<Radio>();
            _cachedFlashlight = RightItem == null ? null : RightItem.GetComponent<Flashlight>();

            if (_cachedFlashlight == null && RightItem != null)
                _cachedFlashlight = RightItem.GetComponentInChildren<Flashlight>();

            if (_cachedPhone != null)
                _cachedToolType = ToolType.phone;
            else if (_cachedRadio != null)
                _cachedToolType = ToolType.radio;
            else if (_cachedFlashlight != null)
                _cachedToolType = ToolType.flashlight;
            else
                _cachedToolType = ToolType.none;
        }

        public bool IsTheSame(ref WeaponDescription other)
        {
            return other.RightItem == RightItem &&
                   other.RightHolster == RightHolster &&
                   other.Shield == Shield &&
                   other.LeftItem == LeftItem &&
                   other.LeftHolster == LeftHolster;
        }

        public static WeaponDescription Default()
        {
            var weapon = new WeaponDescription();
            weapon.IsDualWielding = true;
            weapon.PreferSwapping = true;

            return weapon;
        }
    }
}