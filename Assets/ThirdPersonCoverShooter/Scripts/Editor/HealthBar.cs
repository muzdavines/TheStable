using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    public class HealthBarEditor
    {
        [MenuItem("GameObject/UI/Health Bar")]
        static void CreateHealthBar(MenuCommand command)
        {
            const string square = "ThirdPersonCoverShooter/Assets/Textures/square.png";

            var healthBar = new GameObject("Health Bar");
            {
                var transform = healthBar.AddComponent<RectTransform>();
                transform.sizeDelta = new Vector2(320, 30);
            }
            var bar = healthBar.AddComponent<HealthBar>();

            var background = new GameObject("Background");
            {
                var transform = background.AddComponent<RectTransform>();
                transform.anchorMin = new Vector2(0, 0);
                transform.anchorMax = new Vector2(1, 1);

                bar.BackgroundRect = transform;
            }
            {
                var image = background.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(square);
                image.color = Color.black;
            }
            background.transform.parent = healthBar.transform;

            var fill = new GameObject("Fill");
            {
                var transform = fill.AddComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;

                bar.FillRect = transform;
            }
            {
                var image = fill.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(square);
                image.color = Color.red;
            }
            fill.transform.parent = healthBar.transform;

            var name = new GameObject("Name");
            {
                var transform = name.AddComponent<RectTransform>();
                transform.anchorMin = new Vector2(0.1f, -0.5f);
                transform.anchorMax = new Vector2(1, 1.5f);
            }
            {
                var text = name.AddComponent<Text>();
                text.text = "Cowboy";
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleLeft;
                text.fontSize = 36;

                bar.Name = text;
            }
            name.transform.parent = healthBar.transform;

            GameObjectUtility.SetParentAndAlign(healthBar, command.context as GameObject);

            background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            background.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            fill.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            name.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(healthBar, "Create " + healthBar.name);
            Selection.activeObject = healthBar;
        }

        [MenuItem("GameObject/UI/Enemy Health Bar")]
        static void CreateEnemyHealthBar(MenuCommand command)
        {
            const string square = "ThirdPersonCoverShooter/Assets/Textures/square.png";

            var healthBar = new GameObject("Enemy Health Bar");
            {
                var transform = healthBar.AddComponent<RectTransform>();
                transform.sizeDelta = new Vector2(320, 30);
            }
            var bar = healthBar.AddComponent<HealthBar>();
            bar.HideWhenNone = true;

            healthBar.AddComponent<EnemyHealth>();

            var background = new GameObject("Background");
            {
                var transform = background.AddComponent<RectTransform>();
                transform.anchorMin = new Vector2(0, 0);
                transform.anchorMax = new Vector2(1, 1);

                bar.BackgroundRect = transform;
            }
            {
                var image = background.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(square);
                image.color = Color.black;
            }
            background.transform.parent = healthBar.transform;

            var fill = new GameObject("Fill");
            {
                var transform = fill.AddComponent<RectTransform>();
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.one;

                bar.FillRect = transform;
            }
            {
                var image = fill.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(square);
                image.color = Color.red;
            }
            fill.transform.parent = healthBar.transform;

            var name = new GameObject("Name");
            {
                var transform = name.AddComponent<RectTransform>();
                transform.anchorMin = new Vector2(0, -0.5f);
                transform.anchorMax = new Vector2(1, 1.5f);
            }
            {
                var text = name.AddComponent<Text>();
                text.text = "Enemy";
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 36;

                bar.Name = text;
            }
            name.transform.parent = healthBar.transform;

            GameObjectUtility.SetParentAndAlign(healthBar, command.context as GameObject);

            background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            background.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            fill.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            name.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            name.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            Undo.RegisterCreatedObjectUndo(healthBar, "Create " + healthBar.name);
            Selection.activeObject = healthBar;
        }
    }
}
