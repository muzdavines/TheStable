using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Holds the object with changed transparency. Remembers materials to restore them.
    /// </summary>
    struct FadedObject
    {
        public GameObject Object;
        public FadedRenderer[] Renderers;
        public float Fade;
        public bool KeepThisFrame;
    }

    /// <summary>
    /// Holds information about a faded renderer.
    /// </summary>
    struct FadedRenderer
    {
        public Renderer Renderer;
        public Material OriginalMaterial;
        public Material NewMaterial;
        public Color OriginalColor;
    }

    /// <summary>
    /// Objects closer than the target are faded.
    /// </summary>
    public struct FadeTarget
    {
        /// <summary>
        /// Object to ignore when looking for objects to fade.
        /// </summary>
        public GameObject Object;

        /// <summary>
        /// Position the camera is looking at.
        /// </summary>
        public Vector3 Position;

        public FadeTarget(GameObject obj, Vector3 position)
        {
            Object = obj;
            Position = position;
        }
    }

    /// <summary>
    /// Hides objects that are in front of the camera.
    /// </summary>
    public class CameraObjectFader : MonoBehaviour
    {
        /// <summary>
        /// Speed of material transparency changes.
        /// </summary>
        [Tooltip("Speed of material transparency changes.")]
        public float Speed = 3f;

        /// <summary>
        /// Target object transparency when faded.
        /// </summary>
        [Tooltip("Target object transparency when faded.")]
        public float Transparency = 0.1f;

        private bool _hasTarget;
        private Vector3 _targetPosition;
        private GameObject _target;

        private List<FadedObject> _currentObjects = new List<FadedObject>();
        private List<FadedObject> _oldObjects = new List<FadedObject>();
        private RaycastHit[] _hits = new RaycastHit[64];

        /// <summary>
        /// Camera component sets the new target position.
        /// </summary>
        public void SetFadeTarget(FadeTarget target)
        {
            _hasTarget = true;
            _target = target.Object;
            _targetPosition = target.Position;
        }

        private void Update()
        {
            for (int i = 0; i < _currentObjects.Count; i++)
                keep(i, false);

            if (_hasTarget)
            {
                var vector = _targetPosition - transform.position;
                var count = Physics.RaycastNonAlloc(transform.position, vector, _hits, vector.magnitude);

                for (int i = 0; i < count; i++)
                {
                    var hit = _hits[i];
                    var obj = hit.collider.gameObject;

                    if (!hit.collider.isTrigger && (_target == null || !Util.InHiearchyOf(obj, _target)))
                    {
                        // Check if already fading
                        int oldIndex = indexOf(_currentObjects, obj);
                        if (oldIndex >= 0)
                        {
                            keep(oldIndex, true);
                            continue;
                        }

                        // Check if was being removed
                        oldIndex = indexOf(_oldObjects, obj);
                        if (oldIndex >= 0)
                        {
                            _currentObjects.Add(_oldObjects[oldIndex]);
                            keep(_currentObjects.Count - 1, true);
                            _oldObjects.RemoveAt(oldIndex);
                            continue;
                        }

                        // Create and add
                        {
                            FadedObject faded;
                            faded.Fade = 0;
                            faded.Object = obj;
                            faded.KeepThisFrame = true;

                            var gotRenderers = obj.GetComponentsInChildren<MeshRenderer>();
                            faded.Renderers = new FadedRenderer[gotRenderers.Length];

                            for (int k = 0; k < gotRenderers.Length; k++)
                            {
                                FadedRenderer renderer;
                                renderer.Renderer = gotRenderers[k];
                                renderer.OriginalMaterial = Material.Instantiate(renderer.Renderer.material);
                                renderer.NewMaterial = Material.Instantiate(renderer.Renderer.material);
                                renderer.OriginalColor = renderer.OriginalMaterial.color;
                                renderer.Renderer.material = renderer.NewMaterial;
                                renderer.Renderer.gameObject.SendMessage("OnFade", SendMessageOptions.DontRequireReceiver);

                                faded.Renderers[k] = renderer;
                            }

                            _currentObjects.Add(faded);
                        }
                    }
                }
            }

            for (int i = _currentObjects.Count - 1; i >= 0; i--)
                if (!_currentObjects[i].KeepThisFrame)
                {
                    _oldObjects.Add(_currentObjects[i]);
                    _currentObjects.RemoveAt(i);
                }

            for (int i = 0; i < _currentObjects.Count; i++)
            {
                var value = _currentObjects[i];
                Util.Lerp(ref value.Fade, 1.0f, Speed);
                fade(value.Renderers, value.Fade);

                _currentObjects[i] = value;
            }

            for (int i = _oldObjects.Count - 1; i >= 0; i--)
            {
                var value = _oldObjects[i];
                Util.Lerp(ref value.Fade, 0, Speed);
                fade(value.Renderers, value.Fade);

                if (value.Fade > float.Epsilon)
                    _oldObjects[i] = value;
                else
                {
                    foreach (var renderer in _oldObjects[i].Renderers)
                    {
                        renderer.Renderer.material = renderer.OriginalMaterial;
                        renderer.Renderer.gameObject.SendMessage("OnUnfade", SendMessageOptions.DontRequireReceiver);
                    }

                    _oldObjects.RemoveAt(i);
                }
            }
        }

        private void keep(int index, bool value)
        {
            var temp = _currentObjects[index];
            temp.KeepThisFrame = value;
            _currentObjects[index] = temp;
        }

        private int indexOf(List<FadedObject> list, GameObject obj)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Object == obj)
                    return i;

            return -1;
        }

        private void fade(FadedRenderer[] array, float value)
        {
            foreach (var renderer in array)
            {
                var original = renderer.OriginalColor;
                var faded = original;
                faded.a *= Transparency;

                renderer.Renderer.material.color = Color.Lerp(original, faded, value);
            }
        }
    }
}
