using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoverShooter
{
    /// <summary>
    /// Displays buffs active on the given target object.
    /// </summary>
    public class BuffBar : MonoBehaviour
    {
        /// <summary>
        /// Object whose buffs are displayed.
        /// </summary>
        [Tooltip("Object whose buffs are displayed.")]
        public GameObject Target;

        /// <summary>
        /// Prototype object that is instantiated for every buff.
        /// </summary>
        [Tooltip("Prototype object that is instantiated for every buff.")]
        public GameObject Prototype;

        /// <summary>
        /// Horizontal space between buffs.
        /// </summary>
        [Tooltip("Horizontal space between buffs.")]
        public float Spacing;

        /// <summary>
        /// Icon displayed for active positive DamageBuffs.
        /// </summary>
        [Tooltip("Icon displayed for active positive DamageBuffs.")]
        public Sprite Damage;

        /// <summary>
        /// Icon displayed for active beneficial ArmorBuff.
        /// </summary>
        [Tooltip("Icon displayed for active beneficial ArmorBuff.")]
        public Sprite Armor;

        /// <summary>
        /// Icon displayed for active negative DamageBuffs.
        /// </summary>
        [Tooltip("Icon displayed for active negative DamageBuffs.")]
        public Sprite Toxic;

        /// <summary>
        /// Icon displayed for active negative VisionBuffs.
        /// </summary>
        [Tooltip("Icon displayed for active negative VisionBuffs.")]
        public Sprite Smoke;

        private List<Image> _images = new List<Image>();
        private int _imageIndex;

        private List<ArmorBuff> _armors = new List<ArmorBuff>();
        private List<DamageBuff> _damages = new List<DamageBuff>();
        private List<VisionBuff> _visions = new List<VisionBuff>();

        private void Update()
        {
            _imageIndex = 0;

            if (Target != null)
            {
                _armors.Clear();
                _damages.Clear();
                _visions.Clear();

                Target.GetComponents(_armors);
                Target.GetComponents(_damages);
                Target.GetComponents(_visions);

                foreach (var armor in _armors)
                    if (armor != null && armor.enabled)
                    {
                        if (armor.Multiplier > 1)
                        {
                        }
                        else
                            addImage(Armor);
                    }

                foreach (var damage in _damages)
                    if (damage != null && damage.enabled)
                    {
                        if (damage.Multiplier > 1)
                            addImage(Damage);
                        else
                            addImage(Toxic);
                    }

                foreach (var vision in _visions)
                    if (vision != null && vision.enabled)
                    {
                        if (vision.Multiplier > 1)
                        {
                        }
                        else
                            addImage(Smoke);
                    }
            }

            if (_imageIndex < _images.Count)
                for (int i = _images.Count - 1; i >= _imageIndex; i--)
                {
                    GameObject.Destroy(_images[i].gameObject);
                    _images.RemoveAt(i);
                }
        }

        private void addImage(Sprite sprite)
        {
            Image image;

            if (_images.Count > _imageIndex)
                image = _images[_imageIndex++];
            else if (Prototype != null)
            {
                var obj = GameObject.Instantiate(Prototype);
                var rect = obj.GetComponent<RectTransform>();
                var original = Prototype.GetComponent<RectTransform>();
                rect.SetParent(Prototype.transform.parent);
                rect.anchoredPosition = original.anchoredPosition;
                rect.anchoredPosition += new Vector2(Spacing * _imageIndex, 0);

                obj.SetActive(true);

                image = obj.GetComponent<Image>();
                _images.Add(image);
                _imageIndex = _images.Count;
            }
            else
                return;

            if (image != null)
                image.sprite = sprite;
        }
    }
}
