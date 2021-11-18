using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets up motion vector generation mode to camera.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleCameraMotionBlur : MonoBehaviour
    {
        private void OnEnable()
        {
            ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
        }
    }
}
