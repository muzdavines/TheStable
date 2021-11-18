using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Collection of modifiers for a face.
    /// </summary>
    [Serializable]
    public struct FaceSettings
    {
        [Range(0, 100)]
        public float A;

        [Range(0, 100)]
        public float Angry;

        [Range(0, 100)]
        public float CloseEyes;

        [Range(0, 100)]
        public float E;

        [Range(0, 100)]
        public float M;

        [Range(0, 100)]
        public float Aim;

        [Range(0, 100)]
        public float AngryMouth;

        [Range(0, 100)]
        public float Happy;

        [Range(0, 100)]
        public float HappyEyes;

        [Range(0, 100)]
        public float O;
    }

    /// <summary>
    /// Reacts to character events and modifies blend shapes in a mesh. The set of faces can be expanded by modifying the script or making a new version of it.
    /// </summary>
    public class CharacterFace : MonoBehaviour, ICharacterGunListener, ICharacterZoomListener, ICharacterHealthListener
    {
        /// <summary>
        /// Link to an object with a SkinnedMeshRenderer to apply faces to.
        /// </summary>
        [Tooltip("Link to an object with a SkinnedMeshRenderer to apply faces to.")]
        public GameObject Mesh;

        /// <summary>
        /// Default character face.
        /// </summary>
        [Tooltip("Default character face.")]
        public FaceSettings Default;

        /// <summary>
        /// Face settings when the character is zooming but not firing.
        /// </summary>
        [Tooltip("Face settings when the character is zooming but not firing.")]
        public FaceSettings Zoom;

        /// <summary>
        /// Face settings when a gun trigger is pressed.
        /// </summary>
        [Tooltip("Face settings when a gun trigger is pressed.")]
        public FaceSettings Fire;

        /// <summary>
        /// Face settings when the character is dead.
        /// </summary>
        [Tooltip("Face settings when the character is dead.")]
        public FaceSettings Dead;

        private FaceSettings _face;
        private FaceSettings _interpolatedFace;

        private bool _isFiring;
        private bool _isZoomed;
        private bool _isAlive = true;

        public void OnStartGunFire()
        {
            _isFiring = true;
        }

        public void OnStopGunFire()
        {
            _isFiring = false;
        }

        public void OnZoom()
        {
            _isZoomed = true;
        }

        public void OnUnzoom()
        {
            _isZoomed = false;
        }

        public void OnScope()
        {
        }

        public void OnUnscope()
        {
        }

        public void OnDead()
        {
            _isAlive = false;
        }

        public void OnResurrect()
        {
            _isAlive = true;
        }

        private void Update()
        {
            if (!_isAlive)
                _face = Dead;
            else if (_isFiring)
                _face = Fire;
            else if (_isZoomed)
                _face = Zoom;
            else
                _face = Default;

            var d = 8;
            Util.Lerp(ref _interpolatedFace.A, _face.A, d);
            Util.Lerp(ref _interpolatedFace.Angry, _face.Angry, d);
            Util.Lerp(ref _interpolatedFace.CloseEyes, _face.CloseEyes, d);
            Util.Lerp(ref _interpolatedFace.E, _face.E, d);
            Util.Lerp(ref _interpolatedFace.M, _face.M, d);
            Util.Lerp(ref _interpolatedFace.Aim, _face.Aim, d);
            Util.Lerp(ref _interpolatedFace.AngryMouth, _face.AngryMouth, d);
            Util.Lerp(ref _interpolatedFace.Happy, _face.Happy, d);
            Util.Lerp(ref _interpolatedFace.HappyEyes, _face.HappyEyes, d);
            Util.Lerp(ref _interpolatedFace.O, _face.O, d);

            if (Mesh == null)
                return;
            
            var renderer = Mesh.GetComponent<SkinnedMeshRenderer>();

            if (renderer == null)
                return;

            renderer.SetBlendShapeWeight(0, _interpolatedFace.A);
            renderer.SetBlendShapeWeight(1, _interpolatedFace.Angry);
            renderer.SetBlendShapeWeight(2, _interpolatedFace.CloseEyes);
            renderer.SetBlendShapeWeight(3, _interpolatedFace.E);
            renderer.SetBlendShapeWeight(4, _interpolatedFace.M);
            renderer.SetBlendShapeWeight(5, _interpolatedFace.Aim);
            renderer.SetBlendShapeWeight(6, _interpolatedFace.AngryMouth);
            renderer.SetBlendShapeWeight(7, _interpolatedFace.Happy);
            renderer.SetBlendShapeWeight(8, _interpolatedFace.HappyEyes);
            renderer.SetBlendShapeWeight(9, _interpolatedFace.O);
        }
    }
}