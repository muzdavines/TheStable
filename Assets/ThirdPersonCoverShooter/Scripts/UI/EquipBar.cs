using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// When pressed makes a character pick that weapon.
    /// </summary>
    [ExecuteInEditMode]
    public class EquipBar : PressButton
    {
        /// <summary>
        /// Gun oject that is selected.
        /// </summary>
        [Tooltip("Gun oject that is selected.")]
        public GameObject Target;

        /// <summary>
        /// Motor which will be assigned the weapon when pressed.
        /// </summary>
        [Tooltip("Motor which will be assigned the weapon when pressed.")]
        public CharacterMotor Motor;

        /// <summary>
        /// Determines if the bar is hidden when there is no target.
        /// </summary>
        [Tooltip("Determines if the bar is hidden when there is no target.")]
        public bool HideWhenNone = false;

        /// <summary>
        /// Link to the object that draws the background of the ammo bar.
        /// </summary>
        [Tooltip("Link to the object that draws the background of the ammo bar.")]
        public RectTransform BackgroundRect;

        /// <summary>
        /// Link to the icon objects.
        /// </summary>
        [Tooltip("Link to the icon objects.")]
        public RectTransform[] Icons;

        /// <summary>
        /// Link to the object that will be used to display a gun's name.
        /// </summary>
        [Tooltip("Link to the object that will be used to display a gun's name.")]
        public Text Name;

        /// <summary>
        /// Color to use on elements when the gun is selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is selected.")]
        public Color ActiveColor = Color.white;

        /// <summary>
        /// Color to use on elements when the gun is not selected.
        /// </summary>
        [Tooltip("Color to use on elements when the gun is not selected.")]
        public Color InactiveColor = new Color(1, 1, 1, 0.6f);

        protected override void OnPress()
        {
            if (Motor != null)
            {
                var inventory = Motor.GetComponent<CharacterInventory>();

                for (int i = 0; i < inventory.Weapons.Length; i++)
                    if (inventory.Weapons[i].RightItem == Target ||
                        inventory.Weapons[i].LeftItem == Target)
                    {
                        Motor.InputCancelGrenade();
                        Motor.Weapon = inventory.Weapons[i];
                        Motor.IsEquipped = true;
                        break;
                    }
            }
        }

        private void LateUpdate()
        {
            if (Target != null)
                if (Name != null)
                    Name.text = Target.name;

            if (Application.isPlaying)
            {
                var isVisible = !HideWhenNone || Target != null;
                updateElement(BackgroundRect, isVisible);
                updateElement(Name, isVisible);

                if (Icons != null)
                    for (int i = 0; i < Icons.Length; i++)
                        updateElement(Icons[i], isVisible);
            }
        }

        private void updateElement(RectTransform obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);

            if (isVisible)
            {
                var image = obj.GetComponent<Image>();

                if (Motor != null)
                    image.color = ((Motor.EquippedWeapon.RightItem == Target || Motor.EquippedWeapon.LeftItem == Target) && !Motor.HasGrenadeInHand) ? ActiveColor : InactiveColor;
            }
        }

        private void updateElement(Text obj, bool isVisible)
        {
            if (obj == null)
                return;

            if (obj.gameObject.activeSelf != isVisible)
                obj.gameObject.SetActive(isVisible);
        }
    }
}