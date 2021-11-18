using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Generates and maintains a mesh depicting field of view for a character motor.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class EnemySight : MonoBehaviour
    {
        /// <summary>
        /// Speed of the field showing up or hiding.
        /// </summary>
        [Tooltip("Speed of the field showing up or hiding.")]
        public float FadeSpeed = 2;

        /// <summary>
        /// Number of vertices on the edge.
        /// </summary>
        [Tooltip("Number of vertices on the edge.")]
        public int Detail = 32;

        /// <summary>
        /// AI whose sight is displayed. The component automatically tries to pick one from the parent object.
        /// </summary>
        [Tooltip("AI whose sight is displayed. The component automatically tries to pick one from the parent object.")]
        public AISight TargetOverride;

        /// <summary>
        /// Should the sight be renderer when away from the camera
        /// </summary>
        [Tooltip("Should the sight be renderer when away from the camera")]
        public bool DisplayWhenAway;

        private Mesh _mesh;
        private float _alpha = 0;
        private bool _hasSetAtLeastOnce = false;

        private Vector3[] _positions;
        private Color[] _colors;
        private Vector2[] _uv;
        private int[] _indices;

        private Transform _cachedTransform;
        private AISight _cachedSight;
        private CharacterMotor _cachedMotor;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
        }

        private void Update()
        {
            var targetTransform = TargetOverride == null ? transform.parent : TargetOverride.transform;
            if (targetTransform == null) return;

            if (_cachedTransform != targetTransform)
            {
                _cachedTransform = targetTransform;
                _cachedSight = _cachedTransform.GetComponent<AISight>();
                _cachedMotor = _cachedTransform.GetComponent<CharacterMotor>();
            }

            if (_cachedSight == null)
                return;

            var character = Characters.Get(_cachedSight.gameObject);

            if (character.Motor.IsAlive && (DisplayWhenAway || character.IsAnyInSight(0)))
                _alpha += Time.deltaTime * FadeSpeed;
            else
                _alpha -= Time.deltaTime * FadeSpeed;

            if (_cachedMotor != null)
            {
                var target = _cachedMotor.AimTarget;
                target.y = transform.position.y;

                transform.LookAt(target);
            }

            _alpha = Mathf.Clamp01(_alpha);
            _mesh.Clear();

            var vertexCount = Detail * 3;
            var indexCount = Detail * 3;

            if (_positions == null || _positions.Length != vertexCount)
                _positions = new Vector3[vertexCount];

            if (_colors == null || _colors.Length != vertexCount)
                _colors = new Color[vertexCount];

            if (_uv == null || _uv.Length != vertexCount)
                _uv = new Vector2[vertexCount];

            var wasEdited = false;

            if (_alpha >= 1f / 255f)
            {
                wasEdited = true;

                var fov = _cachedSight.FieldOfView * _alpha;
                var distance = _cachedSight.Distance;

                for (int i = 0; i < Detail; i++)
                {
                    float a0 = fov * ((float)i / (float)Detail - 0.5f) + 90;
                    float a1 = fov * ((float)(i + 1) / (float)Detail - 0.5f) + 90;

                    _positions[i * 3 + 0] = Vector3.zero;
                    _positions[i * 3 + 1] = new Vector3(Mathf.Cos(a1 * Mathf.Deg2Rad), 0, Mathf.Sin(a1 * Mathf.Deg2Rad)) * distance;
                    _positions[i * 3 + 2] = new Vector3(Mathf.Cos(a0 * Mathf.Deg2Rad), 0, Mathf.Sin(a0 * Mathf.Deg2Rad)) * distance;
                }

                for (int i = 0; i < _colors.Length; i++)
                    _colors[i] = new Color(1, 1, 1, _alpha);

                for (int i = 0; i < _uv.Length; i++)
                    _uv[i] = new Vector2((_positions[i].x / distance) * 0.5f + 0.5f, (_positions[i].z / distance) * 0.5f + 0.5f);

                if (_indices == null || _indices.Length != indexCount)
                {
                    _indices = new int[indexCount];

                    for (int i = 0; i < _indices.Length; i++)
                        _indices[i] = i;
                }
            }

            if (wasEdited || !_hasSetAtLeastOnce)
            {
                _hasSetAtLeastOnce = true;

                _mesh.vertices = _positions;
                _mesh.colors = _colors;
                _mesh.uv = _uv;
                _mesh.triangles = _indices;
            }
        }
    }
}
