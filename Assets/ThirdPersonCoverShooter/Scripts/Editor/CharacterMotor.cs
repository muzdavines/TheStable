using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(CharacterMotor))]
    [CanEditMultipleObjects]
    public class CharacterMotorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

#pragma warning disable CS0618 // Type or member is obsolete

            bool canUpgrade = false;

            foreach (var object_ in targets)
            {
                var motor = (CharacterMotor)object_;

                if (motor.Weapons != null && motor.Weapons.Length > 0)
                {
                    canUpgrade = true;
                    break;
                }
            }

            if (canUpgrade)
                if (GUILayout.Button("Upgrade Weapon List"))
                {
                    Undo.RecordObjects(targets, "Upgrade Weapon List");

                    foreach (var object_ in targets)
                    {
                        var motor = (CharacterMotor)object_;

                        if (motor.Weapons == null || motor.Weapons.Length == 0)
                        {
                            motor.Weapons = null;
                            continue;
                        }

                        if (motor.Weapons.Length == 1)
                        {
                            if (motor.Weapon.IsNull || motor.Weapon.IsTheSame(ref motor.Weapons[0]))
                            {
                                if (motor.Weapon.IsNull)
                                    motor.Weapon = motor.Weapons[0];
                            }
                            else
                            {
                                var inventory = motor.GetComponent<CharacterInventory>();
                                if (inventory == null)
                                    inventory = motor.gameObject.AddComponent<CharacterInventory>();

                                if (inventory.Weapons == null)
                                    inventory.Weapons = new WeaponDescription[1];
                                else
                                {
                                    var array = new WeaponDescription[inventory.Weapons.Length + 1];
                                    for (int i = 0; i < inventory.Weapons.Length; i++)
                                        array[i] = inventory.Weapons[i];
                                    inventory.Weapons = array;
                                }

                                inventory.Weapons[inventory.Weapons.Length - 1] = motor.Weapons[0];
                            }
                        }
                        else
                        {
                            var inventory = motor.GetComponent<CharacterInventory>();
                            if (inventory == null)
                                inventory = motor.gameObject.AddComponent<CharacterInventory>();

                            if (inventory.Weapons == null)
                                inventory.Weapons = new WeaponDescription[motor.Weapons.Length];
                            else
                            {
                                var array = new WeaponDescription[inventory.Weapons.Length + motor.Weapons.Length];
                                for (int i = 0; i < inventory.Weapons.Length; i++)
                                    array[i] = inventory.Weapons[i];
                                inventory.Weapons = array;
                            }

                            for (int i = 0; i < motor.Weapons.Length; i++)
                                inventory.Weapons[inventory.Weapons.Length - motor.Weapons.Length + i] = motor.Weapons[i];

                            if (motor.Weapon.IsNull && inventory.Weapons.Length == motor.Weapons.Length)
                                motor.Weapon = inventory.Weapons[0];
                        }

                        motor.Weapons = null;

#pragma warning restore CS0618 // Type or member is obsolete
                    }

                }
        }
    }
}
