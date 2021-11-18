using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Builds a path mesh that depicts the approximated flight of a grenade. Manipulated by player controllers.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class PathPreview : MonoBehaviour
    {
        /// <summary>
        /// Width of the line.
        /// </summary>
        [Tooltip("Width of the line.")]
        public float Width = 0.1f;

        /// <summary>
        /// Distance to fade out starting from the origin.
        /// </summary>
        [Tooltip("Distance to fade out starting from the origin.")]
        public float Fade = 2;

        [HideInInspector]
        public Vector3[] Points;

        [HideInInspector]
        public int PointCount;

        private Mesh _mesh;

        private Vector3[] _positions;
        private Color[] _colors;
        private Vector2[] _uv;
        private int[] _indices;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
        }

        private void Update()
        {
            var vertexCount = PointCount * 2;
            var indexCount = (PointCount - 1) * 6;

            if (_positions == null || _positions.Length != vertexCount)
                _positions = new Vector3[vertexCount];

            if (_colors == null || _colors.Length != vertexCount)
                _colors = new Color[vertexCount];

            if (_uv == null || _uv.Length != vertexCount)
                _uv = new Vector2[vertexCount];

            var right = CameraManager.Main == null ? Vector3.right : CameraManager.Main.transform.right;
            var distance = 0f;

            for (int i = 0; i < PointCount; i++)
            {
                if (i > 0)
                    distance += Vector3.Distance(Points[i - 1], Points[i]);

                var p = Points[i];

                _positions[i * 2 + 0] = transform.InverseTransformPoint(p - right * Width * 0.5f);
                _positions[i * 2 + 1] = transform.InverseTransformPoint(p + right * Width * 0.5f);

                _uv[i * 2 + 0] = new Vector2(0, (float)i / (float)(PointCount - 1));
                _uv[i * 2 + 1] = new Vector2(1, (float)i / (float)(PointCount - 1));

                float alpha = 1;

                if (distance < Fade - float.Epsilon)
                    alpha = distance / Fade;

                _colors[i * 2 + 0] = new Color(1, 1, 1, alpha);
                _colors[i * 2 + 1] = new Color(1, 1, 1, alpha);
            }

            if (_indices == null || _indices.Length != indexCount)
            {
                _indices = new int[indexCount];

                for (int i = 0; i < PointCount - 1; i++)
                {
                    _indices[i * 6 + 0] = (i + 0) * 2 + 0;
                    _indices[i * 6 + 1] = (i + 0) * 2 + 1;
                    _indices[i * 6 + 2] = (i + 1) * 2 + 1;

                    _indices[i * 6 + 3] = (i + 1) * 2 + 1;
                    _indices[i * 6 + 4] = (i + 1) * 2 + 0;
                    _indices[i * 6 + 5] = (i + 0) * 2 + 0;
                }
            }

            _mesh.Clear();
            _mesh.vertices = _positions;
            _mesh.colors = _colors;
            _mesh.uv = _uv;
            _mesh.triangles = _indices;
        }
    }
}
