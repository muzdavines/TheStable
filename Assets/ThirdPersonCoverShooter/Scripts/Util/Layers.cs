namespace CoverShooter
{
    /// <summary>
    /// Stores object layers.
    /// </summary>
    public static class Layers
    {
        /// <summary>
        /// Layer for the ground and level geometry.
        /// </summary>
        public static int Geometry = 1 << 0;

        /// <summary>
        /// Layer for the cover markers.
        /// </summary>
        public static int Cover = 1 << 8;

        /// <summary>
        /// Layer for objects to be hidden when using scope (usually the player renderer).
        /// </summary>
        public static int Scope = 1 << 9;

        /// <summary>
        /// Layer for all human characters.
        /// </summary>
        public static int Character = 1 << 10;

        /// <summary>
        /// Layer for zones.
        /// </summary>
        public static int Zones = 1 << 11;
    }
}
