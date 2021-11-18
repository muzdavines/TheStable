using UnityEngine;
using UnityEngine.EventSystems;

namespace CoverShooter
{
    /// <summary>
    /// Allows to perform AIActions based on the player input.
    /// </summary>
    public class StrategyInput : MonoBehaviour
    {
        /// <summary>
        /// Actors with the same side will be chosen as controllable targets.
        /// </summary>
        [Tooltip("Actors with the same side will be chosen as controllable targets.")]
        public int Side = 1;

        /// <summary>
        /// Target object that receives commands.
        /// </summary>
        [Tooltip("Target object that receives commands.")]
        public Actor Target;

        /// <summary>
        /// Target marker that projects a texture.
        /// </summary>
        [Tooltip("Target marker that projects a texture.")]
        public GameObject MarkerPrefab;

        /// <summary>
        /// Target marker that draws a target sphere.
        /// </summary>
        [Tooltip("Target marker that draws a target sphere.")]
        public GameObject SpherePrefab;

        /// <summary>
        /// Input is ignored when a disabler is active.
        /// </summary>
        [Tooltip("Input is ignored when a disabler is active.")]
        public GameObject Disabler;

        /// <summary>
        /// Color used to highlight pickable actors.
        /// </summary>
        [Tooltip("Color used to highlight pickable actors.")]
        public Color PickColor = Color.yellow;

        /// <summary>
        /// Color used to highlight selected actors.
        /// </summary>
        [Tooltip("Color used to highlight selected actors.")]
        public Color SelectColor = Color.white;

        /// <summary>
        /// Transparency of the target sphere previews.
        /// </summary>
        [Tooltip("Transparency of the target sphere previews.")]
        public float SphereColorOpacity = 0.3f;

        private Camera _camera;

        private GameObject _marker;
        private GameObject _sphere;

        private Material _markerMaterial;
        private Material _sphereMaterial;

        private CharacterOutline _performerOutline;
        private CharacterOutline _targetOutline;

        private AIAction _forcedAction;

        /// <summary>
        /// UI will ask the player to assign the target for the given action.
        /// </summary>
        public void GiveCommand(AIAction action)
        {
            _forcedAction = action;
        }

        private void Awake()
        {
            _markerMaterial = null;
            _sphereMaterial = null;

            _camera = GetComponent<Camera>();

            if (MarkerPrefab != null)
            {
                _marker = GameObject.Instantiate(MarkerPrefab);
                _marker.transform.SetParent(null, true);

                var projector = _marker.GetComponent<Projector>();
                if (projector != null)
                {
                    _markerMaterial = Material.Instantiate(projector.material);
                    projector.material = _markerMaterial;
                }
            }

            if (SpherePrefab != null)
            {
                _sphere = GameObject.Instantiate(SpherePrefab);
                _sphere.transform.SetParent(null, true);

                var renderer = _sphere.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    _sphereMaterial = Material.Instantiate(renderer.sharedMaterial);
                    renderer.material = _sphereMaterial;
                }
            }
        }

        private void OnDisable()
        {
            hideMarker();
            hideOutline(ref _performerOutline);
            hideOutline(ref _targetOutline);
        }

        private void showSphere(Vector3 target, Color color, float radius)
        {
            if (_sphere == null)
                return;

            if (!_sphere.activeSelf)
                _sphere.SetActive(true);

            _sphere.transform.position = target;
            _sphere.transform.localScale = Vector3.one * radius * 2;

            if (_sphereMaterial != null)
            {
                color.a *= SphereColorOpacity;
                _sphereMaterial.color = color;
            }
        }

        private void hideSphere()
        {
            if (_sphere != null && _sphere.activeSelf)
                _sphere.SetActive(false);
        }

        private void showMarker(Vector3 target, Color color)
        {
            if (_marker == null)
                return;

            if (!_marker.activeSelf)
                _marker.SetActive(true);

            _marker.transform.position = target + Vector3.up * 0.5f;
            if (_markerMaterial != null) _markerMaterial.color = color;
        }

        private void hideMarker()
        {
            if (_marker != null && _marker.activeSelf)
                _marker.SetActive(false);
        }

        private void showOutline(ref CharacterOutline outline, Actor target, Color color)
        {
            if (outline != null)
                hideOutline(ref outline);

            outline = target.GetComponent<CharacterOutline>();

            if (outline == null)
            {
                outline = target.gameObject.AddComponent<CharacterOutline>();
                outline.DisplayDefault = false;
            }

            outline.PushColor(this, color);
        }

        private void hideOutlines()
        {
            hideOutline(ref _performerOutline);
            hideOutline(ref _targetOutline);
        }

        private void hideOutline(ref CharacterOutline outline)
        {
            if (outline == null)
                return;

            outline.PopColor(this);
        }

        private void Update()
        {
            if ((Disabler != null && Disabler.activeSelf) || EventSystem.current.IsPointerOverGameObject())
            {
                hideMarker();
                hideOutline(ref _targetOutline);
                hideSphere();

                if (Target == null)
                    hideOutline(ref _performerOutline);
                else
                    showOutline(ref _performerOutline, Target, SelectColor);

                return;
            }

            var camera = _camera;
            if (camera == null) camera = Camera.main;
            if (camera == null) return;

            var mouse = Input.mousePosition;
            mouse.z = camera.nearClipPlane;

            var near = camera.ScreenToWorldPoint(mouse);

            mouse.z = camera.farClipPlane;
            var far = camera.ScreenToWorldPoint(mouse);

            var hit = Util.GetClosestNonActorHit(near, far, 1);
            var targetActor = AIUtil.FindClosestActorIncludingDead(hit, 0.7f);

            var isMouseDown = Input.GetMouseButtonDown(0);

            if (Target == null)
            {
                if (targetActor != null && targetActor.Side == Side && targetActor.IsAlive)
                {
                    showOutline(ref _performerOutline, targetActor, PickColor);

                    if (isMouseDown)
                    {
                        Target = targetActor;
                        isMouseDown = false;
                    }
                }
                else
                    hideOutline(ref _performerOutline);
            }
            else if (Target != null)
                showOutline(ref _performerOutline, Target, SelectColor);
            else
                hideOutline(ref _performerOutline);

            AIActions targetActions = null;

            if (Target != null)
                targetActions = Target.GetComponent<AIActions>();

            if (targetActions)
            {
                var target = hit;
                AIUtil.GetClosestStandablePosition(ref target);

                var isActor = targetActor != null;
                var isSelf = isActor && targetActor == Target;
                var isAlly = isActor && targetActor.Side == Target.Side;
                var isEnemy = isActor && targetActor.Side != Target.Side;
                var isDead = isActor && !targetActor.IsAlive;
                var isLocation = Vector3.Distance(target, hit) < 0.5f;

                AIAction action = null;
                var isTargetingActor = false;

                if (_forcedAction != null)
                {
                    if (_forcedAction.CanTargetGround && isLocation)
                        action = _forcedAction;
                    else if ((!isDead || !_forcedAction.ShouldIgnoreDead) &&
                             ((isSelf && _forcedAction.CanTargetSelf) ||
                              (isAlly && _forcedAction.CanTargetAlly) ||
                              (isEnemy && _forcedAction.CanTargetEnemy)))
                    {
                        action = _forcedAction;
                        isTargetingActor = true;
                    }
                }
                else
                {
                    for (int i = 0; i < targetActions.Actions.Length; i++)
                    {
                        var a = targetActions.Actions[i];

                        if (a.CanTargetGround && isLocation)
                        {
                            action = a;
                            break;
                        }
                        else if ((!isDead || !a.ShouldIgnoreDead) &&
                                 ((isSelf && a.CanTargetSelf) ||
                                  (isAlly && a.CanTargetAlly) ||
                                  (isEnemy && a.CanTargetEnemy)))
                        {
                            action = a;
                            isTargetingActor = true;
                            break;
                        }
                    }
                }

                if (action != null)
                {
                    if (isTargetingActor)
                    {
                        if (isSelf)
                            hideOutline(ref _performerOutline);

                        showOutline(ref _targetOutline, targetActor, action.UIColor);
                        hideMarker();
                        hideSphere();
                    }
                    else
                    {
                        if (action.UIRadius > 0.001f)
                        {
                            showSphere(target, action.UIColor, action.UIRadius);
                            hideOutline(ref _targetOutline);
                            hideMarker();
                        }
                        else
                        {
                            showMarker(target, action.UIColor);
                            hideOutline(ref _targetOutline);
                            hideSphere();
                        }
                    }
                }

                if (isMouseDown)
                {
                    isMouseDown = false;

                    if (action != null)
                    {
                        if (action.CanTargetGround)
                            targetActions.Execute(action, target);
                        else
                            targetActions.Execute(action, targetActor);

                        if (_forcedAction != null)
                            _forcedAction = null;
                    }

                    Target = null;
                }
                else if (Input.GetMouseButtonDown(1))
                    Target = null;
            }
            else
            {
                hideOutline(ref _targetOutline);
                hideMarker();
                hideSphere();
            }
        }
    }
}
