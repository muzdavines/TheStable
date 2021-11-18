using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to store Top, Left, Bottom and Right float-type values.
    /// </summary>
    [Serializable]
    public class Borders
    {
        #region Members

        /// <summary>
        /// Top of the Border.
        /// </summary>
        public float Top;

        /// <summary>
        /// Left of the Border.
        /// </summary>
        public float Left;

        /// <summary>
        /// Bottom of the Border.
        /// </summary>
        public float Bottom;

        /// <summary>
        /// Right of the Border
        /// </summary>
        public float Right;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Borders"/> class.
        /// </summary>
        /// <param name="top">The top of the border.</param>
        /// <param name="left">The left of the border.</param>
        /// <param name="bottom">The bottom of the border.</param>
        /// <param name="right">The right of the border.</param>
        public Borders(float top, float left, float bottom, float right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        #region Operators

        public static Borders operator +(Borders a, Borders b)
        {
            return new Borders(
                    a.Top + b.Top,
                    a.Left + b.Left,
                    a.Bottom + b.Bottom,
                    a.Right + b.Right
                );
        }

        public static Borders operator -(Borders a, Borders b)
        {
            return new Borders(
                a.Top - b.Top,
                a.Left - b.Left,
                a.Bottom - b.Bottom,
                a.Right - b.Right
            );
        }

        public static Borders operator -(Borders a)
        {
            return new Borders(
                -a.Top,
                -a.Left,
                -a.Bottom,
                -a.Right
            );
        }

        public static Borders operator *(Borders a, float d)
        {
            return new Borders(
                a.Top * d,
                a.Left * d,
                a.Bottom * d,
                a.Right * d
            );
        }

        public static Borders operator *(float d, Borders a)
        {
            return new Borders(
                a.Top * d,
                a.Left * d,
                a.Bottom * d,
                a.Right * d
            );
        }

        public static Borders operator /(Borders a, float d)
        {
            return new Borders(
                a.Top / d,
                a.Left / d,
                a.Bottom / d,
                a.Right / d
            );
        }

        public static Borders operator /(float d, Borders a)
        {
            return new Borders(
                a.Top / d,
                a.Left / d,
                a.Bottom / d,
                a.Right / d
            );
        }

        #endregion

        #region Overridables

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var borders = obj as Borders;

            if (borders == null)
                return false;

            return Top.Equals(borders.Top)
                   && Bottom.Equals(borders.Bottom)
                   && Left.Equals(borders.Left)
                   && Right.Equals(borders.Right);
        }
        
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Top: {0}, Bottom: {1}, Left: {2}, Right: {3}", Top, Bottom, Left, Right);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -481391125;
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            return hashCode;
        }
        
        #endregion
    }
}