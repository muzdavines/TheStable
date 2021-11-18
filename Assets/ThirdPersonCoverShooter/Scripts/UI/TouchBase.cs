using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoverShooter
{
    /// <summary>
    /// Base implementation for character touch controls.
    /// </summary>
    public abstract class TouchBase : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// Current value of the controls.
        /// </summary>
        public Vector2 Delta { get { return _delta; } }

        /// <summary>
        /// Current magnitude of input.
        /// </summary>
        public float Magnitude { get { return _magnitude; } }

        /// <summary>
        /// Is the current input cancelled.
        /// </summary>
        public bool IsCancelled { get { return _isCancelled; } }

        /// <summary>
        /// Mobile controller that will be given the input.
        /// </summary>
        [Tooltip("Mobile controller that will be given the input.")]
        public MobileController Controller;

        /// <summary>
        /// Link to the object that is always shown.
        /// </summary>
        [Tooltip("Link to the object that is always shown.")]
        public GameObject Control;

        /// <summary>
        /// Link to the object that is only shown when pressed.
        /// </summary>
        [Tooltip("Link to the object that is only shown when pressed.")]
        public GameObject Selection;

        /// <summary>
        /// Link to the object that is only shown when pressed and represents the center.
        /// </summary>
        [Tooltip("Link to the object that is only shown when pressed and represents the center.")]
        public GameObject Center;

        /// <summary>
        /// Relative size of the screen area at the same side as the touch control.
        /// </summary>
        [Tooltip("Relative size of the screen area at the same side as the touch control.")]
        public float ScreenAreaSize = 0.5f;

        /// <summary>
        /// Distance from the control to it's center circle. Value is relative to the screen height.
        /// </summary>
        [Tooltip("Distance from the control to it's center circle. Value is relative to the screen height.")]
        public float CenterDistance = 0.1f;

        /// <summary>
        /// Cancel input when the distance from the control to the center is lesser than the value. Value is relative to the screen height.
        /// </summary>
        [Tooltip("Cancel input when the distance from the control to the center is lesser than the value. Value is relative to the screen height.")]
        public float CancelDistance = 0.05f;

        private Vector2 _delta = Vector2.zero;
        private Vector2 _center = Vector2.zero;
        private float _magnitude = 0;
        private bool _isPressed;
        private bool _isActive;
        private Vector2 _pixel;
        private int _touchId;
        private bool _isCancelled;
        private Vector2 _previousMousePosition;
        private bool _previousMouseDown;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed)
                return;

            if (isInside(eventData.position))
            {
                _isPressed = true;
                _isActive = false;
                _touchId = eventData.pointerId;

                if (eventData.delta.magnitude > 1)
                    processDelta(eventData.position, eventData.delta);
            }
        }

        protected virtual void Update()
        {
            var wasPressed = _isPressed;
            _isPressed = false;

            if (!wasPressed)
                _center = new Vector2(transform.position.x, transform.position.y);

            var mouseDelta = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - _previousMousePosition;
            _previousMousePosition = Input.mousePosition;

            var wasMouseDown = _previousMouseDown;
            _previousMouseDown = Input.GetMouseButton(0);

            int newTouchId = -10;

            if (Application.isMobilePlatform)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);

                    if (process(touch.fingerId, touch.position, touch.deltaPosition, wasPressed, touch.phase == TouchPhase.Began))
                    {
                        newTouchId = touch.fingerId;
                        break;
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                var mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                if (process(-1, mouse, mouseDelta, wasPressed, !wasMouseDown))
                    newTouchId = -1;
            }

            _touchId = newTouchId;

            if (!_isPressed)
            {
                _delta = Vector2.zero;
                _isActive = false;
            }

            if (_isPressed && _isActive)
            {
                if (Control != null)
                    Control.transform.position = new Vector3(_pixel.x, _pixel.y, Control.transform.position.z);

                if (Selection != null)
                {
                    Selection.transform.position = new Vector3(_pixel.x, _pixel.y, Selection.transform.position.z);

                    if (!Selection.activeSelf)
                        Selection.SetActive(true);
                }

                if (Center != null)
                {
                    Center.transform.position = new Vector3(_center.x, _center.y, Center.transform.position.z);

                    if (!Center.activeSelf)
                        Center.SetActive(true);
                }                
            }
            else
            {
                if (Control != null)
                    Control.transform.position = transform.position;

                if (Selection != null && Selection.activeSelf)
                    Selection.SetActive(false);

                if (Center != null && Center.activeSelf)
                    Center.SetActive(false);
            }
        }

        private bool process(int id, Vector2 pixel, Vector2 delta, bool wasPressed, bool canBegin)
        {
            var isMe = wasPressed && _touchId == id;
            var isBusy = id < 0 ? EventSystem.current.IsPointerOverGameObject() : EventSystem.current.IsPointerOverGameObject(id);
            var canBeginPressing = canBegin && !wasPressed && !isBusy && isInside(pixel);

            if (isMe || canBeginPressing)
            {
                if (!_isActive)
                {
                    if (delta.magnitude > 1)
                        processDelta(pixel, delta);
                    else
                        _isPressed = true;
                }
                else
                    processPixel(pixel);

                return true;
            }
            else
                return false;
        }

        private bool isInside(Vector2 pixel)
        {
            if (transform.position.x < Screen.width * 0.5f)
            {
                if (pixel.x > Screen.width * ScreenAreaSize)
                    return false;
            }
            else
            {
                if (pixel.x < Screen.width - Screen.width * ScreenAreaSize)
                    return false;
            }

            return true;
        }

        private void processDelta(Vector2 pixel, Vector2 delta)
        {
            _pixel = pixel;
            _delta = delta.normalized;
            _isPressed = true;
            _isActive = true;
            _isCancelled = false;
            _center = _pixel - _delta * CenterDistance * Screen.height;
        }

        private void processPixel(Vector2 pixel)
        {
            _pixel = pixel;

            _delta = pixel - _center;
            _isPressed = true;
            _isActive = true;

            var dist = _delta.magnitude;

            if (dist > float.Epsilon)
                _delta.Normalize();

            float centerOffset = CenterDistance * Screen.height;
            float cancelOffset = CancelDistance * Screen.height;

            _isCancelled = dist < cancelOffset;
            _magnitude = 0;

            if (dist > centerOffset)
            {
                _center = _pixel - _delta * centerOffset;

                if (!_isCancelled)
                    _magnitude = 1;
            }
            else if (!_isCancelled)
                _magnitude = (dist - cancelOffset) / (centerOffset - cancelOffset);
        }
    }
}
