using FIMSpace.Generating.Planning;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {

        internal int CheckCollisionDistanceInDirection(CheckerField toOther, Vector2Int direction, int maxDistance = 25)
        {
            for (int i = 0; i < ChildPos.Count; i++)
            {
                Vector2Int start = WorldPos(i);

                for (int c = 0; c < maxDistance; c++)
                {
                    if (toOther.ContainsWorldPos(start + direction * c)) return c;
                }
            }

            return -1;
        }


        public bool CollidesWith(CheckerField other)
        {
            if (UseBounds == false)
            {
                for (int p = 0; p < ChildPos.Count; p++)
                    //for (int o = 0; o < other.ChildPos.Count; o++)
                    //    if (WorldPos(p) == other.WorldPos(o))
                        if (other.ContainsWorldPos(WorldPos(p)))
                            return true;

                return false;
            }

            for (int i = 0; i < Bounding.Count; ++i)
                for (int o = 0; o < other.Bounding.Count; ++o)
                    if (Bounding[i].Intersects(other.Bounding[o])) return true;

            return false;
        }


        public bool OffsettedCollidesWith(CheckerField other, Vector2Int offset)
        {
            for (int p = 0; p < ChildPos.Count; p++)
                    if (other.ContainsWorldPos(WorldPos(p) + offset))
                        return true;

            //for (int p = 0; p < ChildPos.Count; p++)
            //    for (int o = 0; o < other.ChildPos.Count; o++)
            //        if ((WorldPos(p) + offset) == other.WorldPos(o))
            //            return true;

            return false;
        }

        public bool CollidesWithRadius(CheckerField other, int radius)
        {
            if (radius <= 0)
            {
                return CollidesWith(other);
            }

            for (int p = 0; p < ChildPos.Count; p++)
            {
                //for (int o = 0; o < other.ChildPos.Count; o++)
                {
                    // Checking in radius
                    // Check if other checker squares is in range of this checker squares
                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            // If our square in world space in radius offset
                            if (// Overlaps with
                                other.ContainsWorldPos(WorldPos(p) + new Vector2Int(x, y))) // Other checker's square in world position

                                return true;
                        }
                    }

                }
            }

            return false;
        }


        public bool CollidesWith(Vector2Int worldPos)
        {
            for (int p = 0; p < ChildPos.Count; p++)
                if (ChildPosition(p) + Position == worldPos)
                    return true;

            return false;
        }


        public void RemoveOnesCollidingWith(CheckerField other, bool recalculateBounds = true)
        {
            //List<Vector2Int> toRemove = new List<Vector2Int>();

            //for (int p = 0; p < ChildPos.Count; p++)
            //    for (int o = 0; o < other.ChildPos.Count; o++)
            //        if (WorldPos(p) == other.WorldPos(o))
            //            toRemove.Add(ChildPosition(p));

            //for (int i = 0; i < toRemove.Count; i++)
            //    RemoveLocalPos(toRemove[i]);

            for (int o = 0; o < other.ChildPos.Count; o++)
                Remove(other.WorldPos(o));

            if (recalculateBounds)
            {
                RecalculateMultiBounds();
                other.RecalculateMultiBounds();
            }
        }

    }
}