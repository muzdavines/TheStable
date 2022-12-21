using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FIMSpace.Hidden
{
    public static class FVector2IntUnity2017SupportExtensions
    {


        //// Access the /x/ or /y/ component using [0] or [1] respectively.
        //public int this[int index]
        //{
        //    get
        //    {
        //        switch (index)
        //        {
        //            case 0: return x;
        //            case 1: return y;
        //            default:
        //                throw new IndexOutOfRangeException(String.Format("Invalid Vector2Int index addressed: {0}!", index));
        //        }
        //    }

        //    set
        //    {
        //        switch (index)
        //        {
        //            case 0: x = value; break;
        //            case 1: y = value; break;
        //            default:
        //                throw new IndexOutOfRangeException(String.Format("Invalid Vector2Int index addressed: {0}!", index));
        //        }
        //    }
        //}

        //public float magnitude { get { return Mathf.Sqrt((float)(x * x + y * y)); } }

        //public int sqrMagnitude { get { return x * x + y * y; } }

        //public static float Distance(Vector2Int a, Vector2Int b)
        //{
        //    float diff_x = a.x - b.x;
        //    float diff_y = a.y - b.y;

        //    return (float)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        //}

        //public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs) { return new Vector2Int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y)); }

        //public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs) { return new Vector2Int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y)); }

        //public static Vector2Int Scale(Vector2Int a, Vector2Int b) { return new Vector2Int(a.x * b.x, a.y * b.y); }

        //public void Scale(Vector2Int scale) { x *= scale.x; y *= scale.y; }

        //public void Clamp(Vector2Int min, Vector2Int max)
        //{
        //    x = Math.Max(min.x, x);
        //    x = Math.Min(max.x, x);
        //    y = Math.Max(min.y, y);
        //    y = Math.Min(max.y, y);
        //}

        //public static implicit operator Vector2(Vector2Int v)
        //{
        //    return new Vector2(v.x, v.y);
        //}

        //public static explicit operator Vector3Int(Vector2Int v)
        //{
        //    return new Vector3Int(v.x, v.y, 0);
        //}

        //public static Vector2Int FloorToInt(Vector2 v)
        //{
        //    return new Vector2Int(
        //        Mathf.FloorToInt(v.x),
        //        Mathf.FloorToInt(v.y)
        //    );
        //}

        //public static Vector2Int CeilToInt(Vector2 v)
        //{
        //    return new Vector2Int(
        //        Mathf.CeilToInt(v.x),
        //        Mathf.CeilToInt(v.y)
        //    );
        //}

        //public static Vector2Int RoundToInt(Vector2 v)
        //{
        //    return new Vector2Int(
        //        Mathf.RoundToInt(v.x),
        //        Mathf.RoundToInt(v.y)
        //    );
        //}

        /// <summary> Doing just "vec = -vec" For Unity 2017 support </summary>
        public static Vector2Int Negate(this Vector2Int v)
        {
            return new Vector2Int(-v.x, -v.y);
        }

        /// <summary> Doing just "vec = -vec" For Unity 2017 support </summary>
        public static Vector3Int Negate(this Vector3Int v)
        {
            return new Vector3Int(-v.x, -v.y, -v.z);
        }

        //public static Vector2Int Plus(this Vector2Int a, Vector2Int b)
        //{
        //    return new Vector2Int(a.x + b.x, a.y + b.y);
        //}

        //public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        //{
        //    return new Vector2Int(a.x - b.x, a.y - b.y);
        //}

        //public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        //{
        //    return new Vector2Int(a.x * b.x, a.y * b.y);
        //}

        //public static Vector2Int operator *(int a, Vector2Int b)
        //{
        //    return new Vector2Int(a * b.x, a * b.y);
        //}

        //public static Vector2Int operator *(Vector2Int a, int b)
        //{
        //    return new Vector2Int(a.x * b, a.y * b);
        //}

        public static Vector2Int Divide(this Vector2Int a, int b)
        {
            return new Vector2Int(a.x / b, a.y / b);
        }

        //public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
        //{
        //    return lhs.x == rhs.x && lhs.y == rhs.y;
        //}

        //public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
        //{
        //    return !(lhs == rhs);
        //}

        //public override bool Equals(object other)
        //{
        //    if (!(other is Vector2Int)) return false;

        //    return Equals((Vector2Int)other);
        //}

        //public bool Equals(Vector2Int other)
        //{
        //    return x == other.x && y == other.y;
        //}

        //public override int GetHashCode()
        //{
        //    return x.GetHashCode() ^ (y.GetHashCode() << 2);
        //}

        ///// *listonly*
        //public override string ToString()
        //{
        //    return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        //}

        //public string ToString(string format)
        //{
        //    return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        //}

        //public string ToString(string format, IFormatProvider formatProvider)
        //{
        //    return String.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
        //}

        //public static Vector2Int zero { get { return s_Zero; } }
        //public static Vector2Int one { get { return s_One; } }
        //public static Vector2Int up { get { return s_Up; } }
        //public static Vector2Int down { get { return s_Down; } }
        //public static Vector2Int left { get { return s_Left; } }
        //public static Vector2Int right { get { return s_Right; } }

        //private static readonly Vector2Int s_Zero = new Vector2Int(0, 0);
        //private static readonly Vector2Int s_One = new Vector2Int(1, 1);
        //private static readonly Vector2Int s_Up = new Vector2Int(0, 1);
        //private static readonly Vector2Int s_Down = new Vector2Int(0, -1);
        //private static readonly Vector2Int s_Left = new Vector2Int(-1, 0);
        //private static readonly Vector2Int s_Right = new Vector2Int(1, 0);
    }


    public static class FVector3IntUnity2017SupportExtensions
    {
        /// <summary> Doing just "vec /= value" For Unity 2017 and 2018 support </summary>
        public static Vector3Int Divide(this Vector3Int a, int b)
        {
            return new Vector3Int(a.x / b, a.y / b, a.z / b);
        }
    }
}