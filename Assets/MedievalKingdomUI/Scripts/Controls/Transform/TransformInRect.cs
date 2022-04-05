using UnityEngine;
using UnityEngine.EventSystems;

namespace MedievalKingdomUI.Scripts.Controls.Transform
{
    [ExecuteAlways]
    public class TransformInRect : UIBehaviour
    {
        public bool hold;
        public Vector2 ratio = Vector2.one;
        public UnityEngine.Transform child;
        public Canvas canvas;
        protected override void OnRectTransformDimensionsChange()
        {
            Resize();
        }

        protected override void Awake()
        {
            Resize();
        }

        private void Resize()
        {
            if (child == null) return;
            var rectTransform = GetComponent<RectTransform>();
            if (hold)
            {
                var localScale = child.localScale;
                var childSizeBefore = localScale;
                //todo fix rectTransform
                var newSize = rectTransform.rect.size / ratio;
                RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
                localScale = new Vector3(newSize.x, newSize.y, childSizeBefore.y);
                child.localScale = localScale;
                var position = child.localPosition;
                var newPosition = new Vector3
                {
                    x = localScale.x * position.x / childSizeBefore.x,
                    y = localScale.y * position.y / childSizeBefore.y,
                    z = localScale.z * position.z / childSizeBefore.z
                };
                child.localPosition = newPosition;
            }
            else
            {
                var childScale = child.localScale;
                ratio = rectTransform.rect.size / new Vector2(childScale.x, childScale.y);
            }
        }
    }
}
