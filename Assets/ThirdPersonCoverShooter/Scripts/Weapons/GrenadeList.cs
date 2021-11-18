using System.Collections.Generic;

namespace CoverShooter
{
    /// <summary>
    /// All currently active grenades.
    /// </summary>
    public static class GrenadeList
    {
        /// <summary>
        /// All currently active grenades.
        /// </summary>
        public static IEnumerable<Grenade> All
        {
            get { return _list; }
        }

        /// <summary>
        /// Number of grenades currently active inside the level.
        /// </summary>
        public static int Count
        {
            get { return _list.Count; }
        }

        private static List<Grenade> _list = new List<Grenade>();

        /// <summary>
        /// Returns a grenade at index. Index must be less than Count.
        /// </summary>
        public static Grenade Get(int index)
        {
            return _list[index];
        }

        /// <summary>
        /// Adds a grenade to the list.
        /// </summary>
        public static void Register(Grenade grenade)
        {
            if (!_list.Contains(grenade))
                _list.Add(grenade);
        }

        /// <summary>
        /// Removes a grenade from the list.
        /// </summary>
        public static void Unregister(Grenade grenade)
        {
            if (_list.Contains(grenade))
                _list.Remove(grenade);
        }
    }
}
