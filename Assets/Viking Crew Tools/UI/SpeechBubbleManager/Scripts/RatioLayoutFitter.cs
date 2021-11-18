using UnityEngine;
using UnityEngine.UI;

namespace VikingCrew.Tools.UI {
    /// <summary>
    /// This layout group will set its padding based on ratio instead of pixels
    /// 
    /// Only handles ONE child, behaviour for several children is not defined...
    /// </summary>
    [RequireComponent(typeof(ContentSizeFitter))]
    public class RatioLayoutFitter : HorizontalLayoutGroup {
        [Header("Use these to set the ratio [0..1] of child size needed as padding on each side")]
        public Vector2 startPad = Vector2.zero;
        public Vector2 stopPad = Vector2.zero;

        public RectTransform childToFit;

        public override void CalculateLayoutInputHorizontal() {
            float preferredWidth = LayoutUtility.GetPreferredWidth(childToFit);
            float preferredHeight = LayoutUtility.GetPreferredHeight(childToFit);
            padding.left = (int)(preferredWidth * startPad.x / (1 - startPad.x - stopPad.x));
            padding.right = (int)(preferredWidth * stopPad.x / (1 - startPad.x - stopPad.x));
            padding.top = (int)(preferredHeight * startPad.y / (1 - startPad.y - stopPad.y));
            padding.bottom = (int)(preferredHeight * stopPad.y / (1 - startPad.y - stopPad.y));

            base.CalculateLayoutInputHorizontal();
        }
    }
}
