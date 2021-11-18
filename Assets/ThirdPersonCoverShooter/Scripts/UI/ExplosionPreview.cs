using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Displays a sphere depicting an area of grenade explosion. Used by both grenades and player controllers.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    public class ExplosionPreview : MonoBehaviour
    {
        private Renderer _renderer;
        private Material _material;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _material = Material.Instantiate(_renderer.material);
            _renderer.material = _material;
        }

        private void OnRenderObject()
        {
            if (_material != null)
            {
                _material.SetVector("_Center", transform.position);
                _material.SetFloat("_Radius", transform.localScale.x);
            }
        }
    }
}
