using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Just change PGG_MinimapHandler 'Z' axis to 'Y' Axis
    /// </summary>
    public class PGG_MinimapHandler_SidescrollXY : PGG_MinimapHandler
    {

        public override Vector2 GetUIPosition(Vector3 worldPos)
        {
            return new Vector2(worldPos.x * DisplayRatio, worldPos.y * DisplayRatio);
        }

        public override Vector3 ClampFollowWorldPosition(Bounds worldBounds, Vector3 followPosition, Vector2 firstAxis, Vector2 secondaryAxis)
        {
            Vector2 xMargins = new Vector2();
            xMargins.x = worldBounds.center.x - worldBounds.extents.x; xMargins.x += firstAxis.x;
            xMargins.y = worldBounds.center.x + worldBounds.extents.x; xMargins.y -= firstAxis.y;

            Vector2 yMargins = new Vector2();
            yMargins.x = worldBounds.center.y - worldBounds.extents.y; yMargins.x += secondaryAxis.x;
            yMargins.y = worldBounds.center.y + worldBounds.extents.y; yMargins.y -= secondaryAxis.y;

            followPosition.x = Mathf.Clamp(followPosition.x, xMargins.x, xMargins.y);
            followPosition.y = Mathf.Clamp(followPosition.y, yMargins.x, yMargins.y);

            return followPosition;
        }

    }

    #region Editor Class
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PGG_MinimapHandler_SidescrollXY))]
    public class PGG_PixelMapHandler_SidescrollXYEditor : PGG_PixelMapHandlerEditor
    {
    }
#endif
    #endregion

}