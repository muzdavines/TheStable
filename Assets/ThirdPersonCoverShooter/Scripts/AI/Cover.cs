using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes a single cover relative to an AI actor.
    /// </summary>
    public struct CoverItem
    {
        public float Distance;
        public int Direction;
        public Cover Cover;
        public Vector3 Position;
    }

    /// <summary>
    /// Builds and maintains a list of covers near a position.
    /// </summary>
    public class CoverCache
    {
        public List<CoverItem> Items = new List<CoverItem>();

        /// <summary>
        /// Calculates distances of every cover to the given position. Sorts covers by distance.
        /// </summary>
        public void ResetDistance(Vector3 observer)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                item.Distance = Vector3.Distance(observer, item.Position);
                Items[i] = item;
            }

            Items.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        /// <summary>
        /// Finds covers near the position in a given radius. Calculates distances and sorts them.
        /// </summary>
        public void Reset(Vector3 observer, float maxDistance, bool detailedPositions = true)
        {
            Items.Clear();

            var count = Physics.OverlapSphereNonAlloc(observer, maxDistance, Util.Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var collider = Util.Colliders[i];

                if (!collider.isTrigger)
                    continue;

                var cover = CoverSearch.GetCover(collider.gameObject);

                if (cover == null)
                    continue;

                if (cover.IsTall && detailedPositions)
                {
                    if (cover.OpenLeft)
                        consider(cover, cover.LeftCorner(0, -0.3f), -1, observer, maxDistance);

                    if (cover.OpenRight)
                        consider(cover, cover.RightCorner(0, -0.3f), 1, observer, maxDistance);
                }
                else
                    consider(cover, cover.ClosestPointTo(observer, 0.3f, 0.3f), 0, observer, maxDistance);
            }

            Items.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        private void consider(Cover cover, Vector3 position, int direction, Vector3 observer, float maxDistance)
        {
            if (float.IsNaN(position.x) || float.IsNaN(position.z))
                return;

            CoverItem item = new CoverItem();
            item.Cover = cover;
            item.Position = position;
            item.Position.y = cover.Bottom;
            item.Distance = Vector3.Distance(observer, item.Position);
            item.Direction = direction;

            var distanceToObserver = Vector3.Distance(observer, item.Position);

            if (distanceToObserver > maxDistance)
                return;

            var areThereOthers = false;

            const float threshold = 3;

            if (cover.IsTall)
            {
                if (!AIUtil.IsCoverPositionFree(cover, item.Position, threshold, null))
                    areThereOthers = true;
            }
            else
            {
                var hasChangedPosition = false;

                Vector3 side;

                if (Vector3.Dot((item.Position - observer).normalized, cover.Right) > 0)
                    side = cover.Right;
                else
                    side = cover.Left;

                do
                {
                    hasChangedPosition = false;

                    if (!AIUtil.IsCoverPositionFree(cover, item.Position, threshold, null))
                    {
                        var next = item.Position + side * 0.5f;

                        if (cover.IsInFront(next, false))
                        {
                            item.Position = next;
                            hasChangedPosition = true;
                        }
                        else
                            areThereOthers = true;
                    }
                }
                while (hasChangedPosition);
            }

            if (areThereOthers)
                return;

            Items.Add(item);
        }
    }
}
