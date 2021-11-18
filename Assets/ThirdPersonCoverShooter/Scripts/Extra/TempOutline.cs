using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// A basic buff that displays only an outline.
    /// </summary>
    public class TempOutline : BaseBuff
    {
        public TempOutline()
        {
            Outline = true;
        }

        protected override void Begin()
        {
        }

        protected override void End()
        {
            Destroy(this);
        }
    }
}
