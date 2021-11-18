using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Displays an outline around the character mesh.
    /// </summary>
    public class CharacterOutline : MonoBehaviour
    {
        /// <summary>
        /// Default color of the outline, if no outside component has affected it.
        /// </summary>
        [Tooltip("Default color of the outline, if no outside component has affected it.")]
        public Color DefaultColor = Color.yellow;

        /// <summary>
        /// Will the outline be displayed if no other outside component has affected it.
        /// </summary>
        [Tooltip("Will the outline be displayed if no other outside component has affected it.")]
        public bool DisplayDefault = true;

        /// <summary>
        /// Outline size is dependant on the screen size as a fraction of it.
        /// </summary>
        [Tooltip("Outline size is dependant on the screen size as a fraction of it.")]
        public float Width = 0.1f;

        private class OutlineColor
        {
            public Color Value;

            public OutlineColor Previous;
            public OutlineColor Next;
        }

        private Shader _shader;
        private SkinnedMeshRenderer[] _renderers;
        private List<Renderer> _outlines = new List<Renderer>();

        private Dictionary<Object, OutlineColor> _colorMap = new Dictionary<Object, OutlineColor>();
        private OutlineColor _head;

        /// <summary>
        /// Sets a color for the outline. Remembers who pushed the color so that it can be undone and the previous color set.
        /// </summary>
        public void PushColor(Object pusher, Color value)
        {
            PopColor(pusher);

            var color = new OutlineColor();
            color.Value = value;

            if (_head != null)
            {
                color.Next = _head;
                _head.Previous = color;
            }

            _head = color;
            _colorMap[pusher] = color;

            updateState();
        }

        /// <summary>
        /// Removes the color pushed by the given object. 
        /// </summary>
        public void PopColor(Object pusher)
        {
            if (!_colorMap.ContainsKey(pusher))
                return;

            var color = _colorMap[pusher];
            _colorMap.Remove(pusher);

            if (color.Previous != null) color.Previous.Next = color.Next;
            if (color.Next != null) color.Next.Previous = color.Previous;
            if (color == _head) _head = color.Next;

            updateState();
        }

        private void updateState()
        {
            if (_head != null)
            {
                if (!enabled)
                    enabled = true;
            }
            else if (!DisplayDefault && enabled)
                enabled = false;
        }

        private void Awake()
        {
            _shader = Shader.Find("CoverShooter/Outline");
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        private void LateUpdate()
        {
            var color = _head != null ? _head.Value : DefaultColor;

            for (int i = 0; i < _outlines.Count; i++)
                for (int m = 0; m < _outlines[i].sharedMaterials.Length; m++)
                {
                    var material = _outlines[i].sharedMaterials[m];
                    material.SetColor("_Color", color);
                    material.SetFloat("_Width", Width);
                }
        }

        private void OnEnable()
        {
            foreach (var renderer in _renderers)
            {
                var sibling = GameObject.Instantiate(renderer);

                sibling.transform.SetParent(renderer.transform.parent);

                var copy = sibling.GetComponent<SkinnedMeshRenderer>();
                var materials = new Material[renderer.materials.Length];

                for (int i = 0; i < materials.Length; i++)
                    materials[i] = new Material(_shader);

                copy.sharedMaterials = materials;
                _outlines.Add(copy);
            }
        }

        private void OnDisable()
        {
            foreach (var outline in _outlines)
                Destroy(outline.gameObject);

            _outlines.Clear();
        }
    }
}
