using FIMSpace.Generating.Checker;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PlanHelper
    {


        internal HelperRect FindPlaceFor(HelperRect rect, float wallsSeparation, bool checkLimit = true)
        {
            if (InteriorRects.Count != 0)
            {
                var room = GetSmallestBoundsAlignment(rect, wallsSeparation, checkLimit);
                if (room.HelperBool == true) InteriorRects.Add(room);
                rect = room;
            }
            else
            {
                InteriorRects.Add(rect);
                rect.HelperBool = true;
            }

            return rect;
        }

        public Bounds MeasureBounding(List<HelperRect> rects)
        {
            Bounds bounds = new Bounds();
            for (int i = 0; i < rects.Count; i++) bounds.Encapsulate(rects[i].Bound);
            return bounds;
        }

        public Bounds MeasureBounding()
        {
            return MeasureBounding(InteriorRects);
        }


    }

}