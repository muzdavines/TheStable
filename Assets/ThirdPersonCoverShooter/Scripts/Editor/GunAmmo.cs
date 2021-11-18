using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    public class GunAmmoEditor
    {
        [MenuItem("GameObject/UI/Gun Ammo")]
        static void CreateGunAmmo(MenuCommand command)
        {
            var display = new GameObject("Gun Ammo");
            {
                var transform = display.AddComponent<RectTransform>();
                transform.sizeDelta = new Vector2(320, 60);
            }
            {
                var text = display.AddComponent<Text>();
                text.text = "Ammo 10";
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleLeft;
                text.fontSize = 36;
            }
            display.AddComponent<GunAmmo>();

            GameObjectUtility.SetParentAndAlign(display, command.context as GameObject);

            Undo.RegisterCreatedObjectUndo(display, "Create " + display.name);
            Selection.activeObject = display;
        }
    }
}
