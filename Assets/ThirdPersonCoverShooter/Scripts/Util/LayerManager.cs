using UnityEngine;

namespace CoverShooter
{
    public class LayerManager : MonoBehaviour
    {
        /// <summary>
        /// Layer for the ground and level geometry.
        /// </summary>
        public int Geometry = Layers.Geometry;

        /// <summary>
        /// Layer for the cover markers.
        /// </summary>
        public int Cover = Layers.Cover;

        /// <summary>
        /// Layer for objects to be hidden when using scope (usually the player renderer).
        /// </summary>
        public int Scope = Layers.Scope;

        /// <summary>
        /// Layer for all human characters.
        /// </summary>
        public int Character = Layers.Character;

        /// <summary>
        /// Layer for zones.
        /// </summary>
        public int Zones = Layers.Zones;

        private void Awake()
        {
            SetValues();
        }

        public void SetValues()
        {
            Layers.Geometry = Geometry;
            Layers.Cover = Cover;
            Layers.Scope = Scope;
            Layers.Character = Character;
            Layers.Zones = Zones;
        }
    }
}
