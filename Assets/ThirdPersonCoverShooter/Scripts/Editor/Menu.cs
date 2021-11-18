using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    public class Menu
    {
        [MenuItem("CoverShooter/Find and setup player")]
        static void FindAndSetupPlayer(MenuCommand command)
        {
            var mobile = GameObject.FindObjectOfType<MobileController>();
            var thirdPerson = GameObject.FindObjectOfType<ThirdPersonController>();

            foreach (var camera in GameObject.FindObjectsOfType<ThirdPersonCamera>())
            {
                Undo.RecordObject(camera, "Camera target");
                camera.Target = thirdPerson.GetComponent<CharacterMotor>();
            }

            foreach (var camera in GameObject.FindObjectsOfType<MobileCamera>())
            {
                Undo.RecordObject(camera, "Camera target");
                camera.Target = mobile.GetComponent<CharacterMotor>();
            }

            foreach (var control in GameObject.FindObjectsOfType<TouchMovement>())
            {
                Undo.RecordObject(control, "Touch target");
                control.Controller = mobile;
            }

            foreach (var control in GameObject.FindObjectsOfType<TouchAiming>())
            {
                Undo.RecordObject(control, "Touch target");
                control.Controller = mobile;
            }

            foreach (var control in GameObject.FindObjectsOfType<CrouchTouch>())
            {
                Undo.RecordObject(control, "Touch target");
                control.Controller = mobile;
            }

            foreach (var ammo in GameObject.FindObjectsOfType<GunAmmo>())
                if (ammo.Motor == null || ammo.Motor.GetComponent<MobileController>() || ammo.Motor.GetComponent<ThirdPersonController>())
                {
                    if (mobile != null)
                    {
                        Undo.RecordObject(ammo, "Ammo target");
                        ammo.Motor = mobile.GetComponent<CharacterMotor>();
                    }
                    else if (thirdPerson != null)
                    {
                        Undo.RecordObject(ammo, "Ammo target");
                        ammo.Motor = thirdPerson.GetComponent<CharacterMotor>();
                    }
                }

            foreach (var bar in GameObject.FindObjectsOfType<AmmoBar>())
            {
                var previous = bar.Motor == null ? null : bar.Motor.GetComponent<CharacterInventory>();

                Undo.RecordObject(bar, "Ammo target");

                if (mobile != null)
                    bar.Motor = mobile.GetComponent<CharacterMotor>();
                else
                    bar.Motor = thirdPerson.GetComponent<CharacterMotor>();

                var next = bar.Motor == null ? null : bar.Motor.GetComponent<CharacterInventory>();

                if (next != null && next != previous && previous != null && bar.Target != null)
                {
                    for (int i = 0; i < previous.Weapons.Length; i++)
                        if (previous.Weapons[i].Gun == bar.Target)
                        {
                            if (next.Weapons.Length > i)
                                bar.Target = next.Weapons[i].Gun;

                            break;
                        }
                }
            }

            foreach (var display in GameObject.FindObjectsOfType<EnemyDisplayManager>())
            {
                if (mobile != null)
                {
                    Undo.RecordObject(display, "Enemy display target");
                    display.Player = mobile.GetComponent<Actor>();
                }
                else if (thirdPerson != null)
                {
                    Undo.RecordObject(display, "Enemy display target");
                    display.Player = thirdPerson.GetComponent<Actor>();
                }
            }

            foreach (var health in GameObject.FindObjectsOfType<HealthBar>())
                if (health.Target != null)
                {
                    if (health.Target.GetComponent<MobileController>())
                    {
                        Undo.RecordObject(health, "Health target");
                        health.Target = mobile.gameObject;
                    }
                    else if (health.Target.GetComponent<ThirdPersonController>())
                    {
                        Undo.RecordObject(health, "Health target");
                        health.Target = thirdPerson.gameObject;
                    }
                }
                else
                {
                    if (mobile != null)
                    {
                        Undo.RecordObject(health, "Health target");
                        health.Target = mobile.gameObject;
                    }
                    else if (thirdPerson != null)
                    {
                        Undo.RecordObject(health, "Health target");
                        health.Target = thirdPerson.gameObject;
                    }
                }
        }
    }
}
