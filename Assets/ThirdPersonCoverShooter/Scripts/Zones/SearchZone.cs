using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes an open area for the AI to check out when searching. By default AI only searches covers, using this component an additional areas can be added.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class SearchZone : Zone<SearchZone>
    {
        /// <summary>
        /// Returns a list of all checkable points inside the zone.
        /// </summary>
        public IEnumerable<Vector3> Points(float threshold)
        {
            float width = Width / threshold;
            float depth = Depth / threshold;

            int wcount;
            int dcount;

            if (width <= 0.5f)
                wcount = 1;
            else
                wcount = (int)(width + 0.5f) + 1;

            if (depth <= 0.5f)
                dcount = 1;
            else
                dcount = (int)(depth + 0.5f) + 1;

            Vector3 position;
            position.y = -Height * 0.5f;

            var w = Width * 0.5f;
            var d = Depth * 0.5f;

            var xstep = Width / (wcount - 1);
            var zstep = Depth / (dcount - 1);

            for (int x = 0; x < wcount; x++)
            {
                if (wcount == 0)
                    position.x = w;
                else
                    position.x = x * xstep - w;

                for (int z = 0; z < dcount; z++)
                {
                    if (dcount == 0)
                        position.z = d;
                    else
                        position.z = z * zstep - d;

                    yield return transform.TransformPoint(position);
                }
            }
        }
    }

    /// <summary>
    /// Maintains a list of search zones inside an area (denoted by a position and radius).
    /// </summary>
    public class SearchZoneCache
    {
        public List<SearchZone> Items = new List<SearchZone>();

        /// <summary>
        /// Creates a list of search zones that are in the area surounding the observer.
        /// </summary>
        public void Reset(Vector3 observer, float maxDistance)
        {
            Items.Clear();
            var count = Physics.OverlapSphereNonAlloc(observer, maxDistance, Util.Colliders, Layers.Zones, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var collider = Util.Colliders[i];
            
                if (!collider.isTrigger)
                    continue;

                var block = SearchZone.Get(collider.gameObject);

                if (block != null)
                    Items.Add(block);
            }
        }
    }
}
