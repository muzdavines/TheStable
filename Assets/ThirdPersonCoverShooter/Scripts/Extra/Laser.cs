using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates and displays a laser mesh. Set up by a BaseGun component.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class Laser : MonoBehaviour
    {
        /// <summary>
        /// Max length of the laser.
        /// </summary>
        [Tooltip("Max length of the laser.")]
        public float MaxLength = 4.0f;

        /// <summary>
        /// Transparency of the laser, usually managed by the gun.
        /// </summary>
        [Tooltip("Transparency of the laser, usually managed by the gun.")]
        public float Alpha = 1;

        /// <summary>
        /// Managed by the gun or some other component.
        /// </summary>
        [HideInInspector]
        public float Length = 4.0f;

        private Mesh _mesh;
        private Renderer _renderer;

        private float _generatedLength;
        private float _laserIntensity;

        /// <summary>
        /// Setup the laser's origin and target. Adjusts the attached transform.
        /// </summary>
        public void Setup(Vector3 origin, Vector3 target)
        {
            transform.position = origin;
            transform.LookAt(target);

            Length = Vector3.Distance(origin, target);

            if (Length > MaxLength)
                Length = MaxLength;
        }

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<Renderer>();

            var material = _renderer.material == null ? null : Material.Instantiate(_renderer.material);
            _renderer.material = material;
        }

        private void Update()
        {
            if (Alpha < _laserIntensity)
            {
                _laserIntensity -= Time.deltaTime * 8;
                _laserIntensity = Mathf.Clamp(_laserIntensity, Alpha, 1);
            }
            else
            {
                _laserIntensity += Time.deltaTime * 3;
                _laserIntensity = Mathf.Clamp(_laserIntensity, 0, Alpha);
            }

            if (_renderer.material != null)
            {
                _renderer.enabled = Alpha > float.Epsilon;

                if (_renderer.enabled)
                {
                    var color = _renderer.material.color;
                    color.a = _laserIntensity;
                    _renderer.material.color = color;
                }
            }
            else
                _renderer.enabled = Alpha > 0.5f;

            if (_generatedLength == Length)
                return;

            _generatedLength = Length;
            var length = transform.InverseTransformPoint(transform.position + transform.forward * Length).magnitude;

            const int detail = 32;
            const int vertexCount = detail * 2;
            const int indexCount = (detail - 1) * 6;

            var vertices = new Vector3[vertexCount];
            var colors = new Color[vertexCount];
            var uv = new Vector2[vertexCount];
            var triangles = new int[indexCount];

            var v = Length / MaxLength;

            for (int i = 0; i < detail; i++)
            {
                var p = i / (float)(detail - 1);
                var a = Mathf.PI * 2 * p;
                var u = Mathf.Cos(a) * 0.5f + 0.5f;
                var x = Mathf.Cos(a);
                var y = Mathf.Sin(a);
                var p1 = new Vector3(x, y, 0);
                var p2 = new Vector3(x, y, length);

                vertices[i] = p1;
                vertices[i + detail] = p2;
                uv[i] = new Vector2(u, 0);
                uv[i + detail] = new Vector2(u, v);
            }

            var index = 0;

            for (int i = 0; i < detail - 1; i++)
            {
                triangles[index++] = i;
                triangles[index++] = i + 1;
                triangles[index++] = detail + i;

                triangles[index++] = i + 1;
                triangles[index++] = detail + i + 1;
                triangles[index++] = detail + i;
            }

            _mesh.Clear();

            _mesh.vertices = vertices;
            _mesh.colors = colors;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }
    }
}
