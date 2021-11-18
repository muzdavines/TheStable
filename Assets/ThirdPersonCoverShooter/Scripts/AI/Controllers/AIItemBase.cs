using UnityEngine;

namespace CoverShooter
{
    public enum InventoryUsage
    {
        autoFind,
        index,
        none
    }

    /// <summary>
    /// Base class for an item management component.
    /// </summary>
    public class AIItemBase : AIBase
    {
        /// <summary>
        /// How should the character inventory (if there is one) be used.
        /// </summary>
        [Tooltip("How should the character inventory (if there is one) be used.")]
        public InventoryUsage InventoryUsage = InventoryUsage.autoFind;

        /// <summary>
        /// Weapon index inside the inventory to use when usage is set to 'index'.
        /// </summary>
        [Tooltip("Weapon index inside the inventory to use when usage is set to 'index'.")]
        public int InventoryIndex = 0;

        private CharacterInventory _inventory;

        protected virtual void Awake()
        {
            _inventory = GetComponent<CharacterInventory>();
        }

        /// <summary>
        /// Equips any weapon if possible.
        /// </summary>
        protected bool EquipWeapon(CharacterMotor motor)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                motor.Weapon = _inventory.Weapons[InventoryIndex];
                motor.IsEquipped = true;
                return true;
            }

            if (InventoryUsage == InventoryUsage.autoFind && _inventory != null)
                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (_inventory.Weapons[i].Gun != null)
                    {
                        InventoryIndex = i;
                        motor.Weapon = _inventory.Weapons[InventoryIndex];
                        motor.IsEquipped = true;
                        return true;
                    }

            if (motor.Weapon.IsNull)
                return false;

            if (motor.Weapon.Gun == null)
                return false;

            motor.IsEquipped = true;
            return true;
        }

        /// <summary>
        /// Equips a weapon of specific kind if possible.
        /// </summary>
        protected bool Equip(CharacterMotor motor, WeaponType type)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                motor.Weapon = _inventory.Weapons[InventoryIndex];
                motor.IsEquipped = true;
                return true;
            }

            if (InventoryUsage == InventoryUsage.autoFind && _inventory != null)
            {
                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (_inventory.Weapons[i].Gun != null && _inventory.Weapons[i].Gun.Type == type)
                    {
                        InventoryIndex = i;
                        motor.Weapon = _inventory.Weapons[InventoryIndex];
                        motor.IsEquipped = true;
                        return true;
                    }

                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (_inventory.Weapons[i].Gun != null)
                    {
                        InventoryIndex = i;
                        motor.Weapon = _inventory.Weapons[InventoryIndex];
                        motor.IsEquipped = true;
                        return true;
                    }
            }

            if (motor.Weapon.IsNull)
                return false;

            if (motor.Weapon.Gun == null)
                return false;

            motor.IsEquipped = true;
            return true;
        }

        /// <summary>
        /// Equips a specific tool if possible. Prefers items without gun scripts.
        /// </summary>
        protected bool Equip(CharacterMotor motor, ToolType tool)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                motor.Weapon = _inventory.Weapons[InventoryIndex];
                motor.IsEquipped = true;
                return true;
            }

            if (InventoryUsage == InventoryUsage.autoFind && _inventory != null)
            {
                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (!_inventory.Weapons[i].IsNull && _inventory.Weapons[i].Gun == null && _inventory.Weapons[i].ToolType == tool)
                    {
                        InventoryIndex = i;
                        motor.Weapon = _inventory.Weapons[InventoryIndex];
                        motor.IsEquipped = true;
                        return true;
                    }

                for (int i = 0; i < _inventory.Weapons.Length; i++)
                    if (!_inventory.Weapons[i].IsNull && _inventory.Weapons[i].ToolType == tool)
                    {
                        InventoryIndex = i;
                        motor.Weapon = _inventory.Weapons[InventoryIndex];
                        motor.IsEquipped = true;
                        return true;
                    }
            }

            if (motor.Weapon.IsNull)
                return false;

            if (motor.Weapon.ToolType != tool)
                return false;

            motor.IsEquipped = true;
            return true;
        }

        /// <summary>
        /// Unequips the item if it is currently used.
        /// </summary>
        protected bool UnequipWeapon(CharacterMotor motor)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                if (_inventory.Weapons[InventoryIndex].IsTheSame(ref motor.Weapon))
                {
                    motor.IsEquipped = false;
                    return true;
                }
                else
                    return false;
            }

            if (motor.Weapon.Gun == null)
                return false;

            motor.IsEquipped = false;
            return true;
        }

        /// <summary>
        /// Unequips the item if it is currently used.
        /// </summary>
        protected bool Unequip(CharacterMotor motor, WeaponType type)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                if (_inventory.Weapons[InventoryIndex].IsTheSame(ref motor.Weapon))
                {
                    motor.IsEquipped = false;
                    return true;
                }
                else
                    return false;
            }

            if (motor.Weapon.Gun == null)
                return false;

            if (motor.Weapon.Gun.Type != type)
                return false;

            motor.IsEquipped = false;
            return true;
        }

        /// <summary>
        /// Unequips the item if it is currently used.
        /// </summary>
        protected bool Unequip(CharacterMotor motor, ToolType tool)
        {
            if (!isActiveAndEnabled)
                return false;

            if (InventoryUsage == InventoryUsage.index &&
                _inventory != null && InventoryIndex >= 0 && InventoryIndex < _inventory.Weapons.Length)
            {
                if (_inventory.Weapons[InventoryIndex].IsTheSame(ref motor.Weapon))
                {
                    motor.IsEquipped = false;
                    return true;
                }
                else
                    return false;
            }

            if (motor.Weapon.IsNull)
                return false;

            if (motor.Weapon.ToolType != tool)
                return false;

            motor.IsEquipped = false;
            return true;
        }

        /// <summary>
        /// Finds an item index of a weapon. Prefers the given type. Returns true if a weapon was found.
        /// </summary>
        private bool autoFind(CharacterMotor motor, WeaponType type)
        {
            if (_inventory == null)
                return false;

            for (int i = 0; i < _inventory.Weapons.Length; i++)
                if (_inventory.Weapons[i].Gun != null && _inventory.Weapons[i].Gun.Type == type)
                {
                    InventoryIndex = i;
                    return true;
                }

            for (int i = 0; i < _inventory.Weapons.Length; i++)
                if (_inventory.Weapons[i].Gun != null)
                {
                    InventoryIndex = i;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Finds an item index of a tool. Prefers items without gun scripts.
        /// </summary>
        private bool autoFind(CharacterMotor motor, ToolType tool)
        {
            if (_inventory == null)
                return false;

            for (int i = 0; i < _inventory.Weapons.Length; i++)
                if (_inventory.Weapons[i].Gun == null && _inventory.Weapons[i].ToolType == tool)
                {
                    InventoryIndex = i;
                    return true;
                }

            for (int i = 0; i < _inventory.Weapons.Length; i++)
                if (_inventory.Weapons[i].ToolType == tool)
                {
                    InventoryIndex = i;
                    return true;
                }

            return false;
        }
    }
}
