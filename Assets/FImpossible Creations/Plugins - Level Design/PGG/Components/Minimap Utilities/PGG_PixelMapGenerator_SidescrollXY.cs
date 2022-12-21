using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/PGG/Minimap/PGG Pixel Minimap Sidescroll X-Y Generator")]
    public class PGG_PixelMapGenerator_SidescrollXY : PGG_PixelMapGenerator
    {
        // Change axis to work on pixel position out of world x and y values instead of using world position x and z values

        protected override Vector3 SetSec( Vector3 v, float val)
        {
            v.y = val;
            return v;
        }

        protected override float SecAxis(Vector3 v)
        {
            return v.y;
        }

        protected override float HeightAxis(Vector3 v)
        {
            return v.z;
        }

    }

    #region Editor Class

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(PGG_PixelMapGenerator_SidescrollXY))]
    public class PGG_PixelMapGenerator_SidescrollXYEditor : PGG_PixelMapGeneratorEditor
    {
        protected override float Info_RestrictAxisValue()
        {
            return Get.transform.position.z;
        }

        protected override string Info_RestrictAxisLetter()
        {
            return "Z";
        }
    }

#endif

    #endregion

}