using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace com.ootii.Input
{
    /// <summary>
    /// Base class used to abstract how input is retrieved. Inherit from this class 
    /// and then implement the function as needed in order to allow other objects 
    /// to process input using your input component.
    /// </summary>
    [AddComponentMenu("ootii/Input Sources/Unity Input System Source")]
    public class UnityInputSystemSource : MonoBehaviour, IInputSource, IViewActivator
    {

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// Holds onto the PlayerInput object that contains the actions.
        /// </summary>
        public PlayerInput _PlayerInput;
        public virtual PlayerInput PlayerInput
        {
            get { return _PlayerInput; }
            set { _PlayerInput = value; }
        }

#endif

        /// <summary>
        /// Action name that is used to control movement
        /// </summary>
        public string _MoveAction = "Move";
        public string MoveAction
        {
            get { return _MoveAction; }
            set { _MoveAction = value; }
        }

        /// <summary>
        /// Action name that is used to control viewing
        /// </summary>
        public string _LookAction = "Look";
        public string LookAction
        {
            get { return _LookAction; }
            set { _LookAction = value; }
        }

        /// <summary>
        /// Helps users of the input source to determine if
        /// they are processing user input
        /// </summary>
        [Tooltip("Determines if we'll get input from the mouse, keyboard, and gamepad.")]
        public bool _IsEnabled = true;
        public virtual bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }

        /// <summary>
        /// Determines if the Xbox controller is enable. We default this to off since
        /// the editor needs to enable settings.
        /// </summary>
        [Tooltip("Determines we can use the Xbox controller for input.")]
        public bool _IsXboxControllerEnabled = false;
        public virtual bool IsXboxControllerEnabled
        {
            get { return _IsXboxControllerEnabled; }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the camera's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public virtual float InputFromCameraAngle
        {
            get { return 0f; }
            set { }
        }

        /// <summary>
        /// Set by an external object, it tracks the angle of the
        /// user input compared to the avatars's forward direction
        /// Note that this info isn't reliable as objects using it 
        /// before it's set it will get float.NaN.
        /// </summary>
        public virtual float InputFromAvatarAngle
        {
            get { return 0f; }
            set { }
        }

        /// <summary>
        /// Retrieves horizontal movement from the the input
        /// </summary>
        public virtual float MovementX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

#if ENABLE_INPUT_SYSTEM

                if (_PlayerInput != null)
                {
                    InputAction lAction = _PlayerInput.actions.FindAction(_MoveAction);
                    if (lAction != null)
                    {
                        Vector2 lActionValue = lAction.ReadValue<Vector2>();
                        return lActionValue.x;
                    }
                }

                return 0f;

#else

                float lValue = 0f;
                if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) { lValue++; }
                if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) { lValue--; }

                if (_IsXboxControllerEnabled && lValue == 0f)
                {
                    try
                    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        lValue = UnityEngine.Input.GetAxis("MXLeftStickX");
#else
                        lValue = UnityEngine.Input.GetAxis("WXLeftStickX");
#endif
                    }
                    catch { }
                }

                return lValue;

#endif
            }
        }

        /// <summary>
        /// Retrieves vertical movement from the the input
        /// </summary>
        public virtual float MovementY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

#if ENABLE_INPUT_SYSTEM

                if (_PlayerInput != null)
                {
                    InputAction lAction = _PlayerInput.actions.FindAction(_MoveAction);
                    if (lAction != null)
                    {
                        Vector2 lActionValue = lAction.ReadValue<Vector2>();
                        return lActionValue.y;
                    }
                }

                return 0f;

#else

                float lValue = 0f;
                if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) { lValue++; }
                if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) { lValue--; }

                if (_IsXboxControllerEnabled && lValue == 0f)
                {
                    try
                    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                        lValue = UnityEngine.Input.GetAxis("MXLeftStickY"); 
#else
                        lValue = UnityEngine.Input.GetAxis("WXLeftStickY");
#endif
                    }
                    catch { }
                }

                return lValue;

#endif
            }
        }

        /// <summary>
        /// Squared value of MovementX and MovementY (mX * mX + mY * mY)
        /// </summary>
        public virtual float MovementSqr
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

                float lMovementX = MovementX;
                float lMovementY = MovementY;
                return ((lMovementX * lMovementX) + (lMovementY * lMovementY));
            }
        }

        /// <summary>
        /// Retrieves horizontal view movement from the the input
        /// </summary>
        public virtual float ViewX
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

#if ENABLE_INPUT_SYSTEM

                if (_PlayerInput != null)
                {
                    InputAction lAction = _PlayerInput.actions.FindAction(_LookAction);
                    if (lAction != null)
                    {
                        Vector2 lActionValue = lAction.ReadValue<Vector2>();

                        // Account for scaling applied directly in Windows code by old input system.
                        lActionValue *= 0.5f;

                        // Account for sensitivity setting on old Mouse X and Y axes.
                        lActionValue *= 0.1f;

                        return lActionValue.x;
                    }
                }

                Vector2Control lMouseDeltaControl = Mouse.current.delta;
                Vector2 lMouseDelta = lMouseDeltaControl.ReadValue();

                // Account for scaling applied directly in Windows code by old input system.
                lMouseDelta *= 0.5f;

                // Account for sensitivity setting on old Mouse X and Y axes.
                lMouseDelta *= 0.1f;

                return lMouseDelta.x;

#else

                float lValue = UnityEngine.Input.GetAxis("Mouse X");

                // The mouse value is already frame rate independent (since it's basedon position). However, 
                // we need the stick movement to compensate for the frame rate too. We'll make it's value relative
                // to 60FPS (1/60 = 0.01666)
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("MXRightStickX") * (Time.deltaTime / 0.01666f); }
#else
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("WXRightStickX") * (Time.deltaTime / 0.01666f); }
#endif

                return lValue;

#endif
            }
        }

        /// <summary>
        /// Retrieves vertical view movement from the the input
        /// </summary>
        public virtual float ViewY
        {
            get
            {
                if (!_IsEnabled) { return 0f; }

#if ENABLE_INPUT_SYSTEM

                if (_PlayerInput != null)
                {
                    InputAction lAction = _PlayerInput.actions.FindAction(_LookAction);
                    if (lAction != null)
                    {
                        Vector2 lActionValue = lAction.ReadValue<Vector2>();

                        // Account for scaling applied directly in Windows code by old input system.
                        lActionValue *= 0.5f;

                        // Account for sensitivity setting on old Mouse X and Y axes.
                        lActionValue *= 0.1f;

                        return lActionValue.y;
                    }
                }

                Vector2Control lMouseDeltaControl = Mouse.current.delta;
                Vector2 lMouseDelta = lMouseDeltaControl.ReadValue();

                // Account for scaling applied directly in Windows code by old input system.
                lMouseDelta *= 0.5f;

                // Account for sensitivity setting on old Mouse X and Y axes.
                lMouseDelta *= 0.1f;

                return lMouseDelta.y;

#else

                float lValue = UnityEngine.Input.GetAxis("Mouse Y");

                // The mouse value is already frame rate independent (since it's basedon position). However, 
                // we need the stick movement to compensate for the frame rate too. We'll make it's value relative
                // to 60FPS (1/60 = 0.01666)
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("MXRightStickY") * (Time.deltaTime / 0.01666f); }
#else
                if (_IsXboxControllerEnabled && lValue == 0f) { lValue = UnityEngine.Input.GetAxis("WXRightStickY") * (Time.deltaTime / 0.01666f); }
#endif

                return lValue;

#endif
            }
        }

        /// <summary>
        /// Determines if the player can freely look around
        /// </summary>
        public virtual bool IsViewingActivated
        {
            get
            {
                if (!_IsEnabled) { return false; }

                bool lValue = false;

#if ENABLE_INPUT_SYSTEM

                //if (_IsXboxControllerEnabled)
                //{
                //    if (_PlayerInput != null)
                //    {
                //        InputAction lAction = _PlayerInput.actions.FindAction(_LookAction);
                //        if (lAction != null)
                //        {
                //            Vector2 lActionValue = lAction.ReadValue<Vector2>();
                //            lValue = (lActionValue.sqrMagnitude != 0f);
                //        }
                //    }
                //}

                if (!lValue)
                {
                    if (_ViewActivator == 0)
                    {
                        lValue = true;
                    }
                    else if (_ViewActivator == 1)
                    {
                        lValue = Mouse.current.leftButton.isPressed;
                    }
                    else if (_ViewActivator == 2)
                    {
                        lValue = Mouse.current.rightButton.isPressed;
                    }
                    else if (_ViewActivator == 3)
                    {
                        lValue = Mouse.current.leftButton.isPressed;
                        if (!lValue) { lValue = Mouse.current.rightButton.isPressed; }
                    }
                    else if (_ViewActivator == 4)
                    {
                        lValue = Mouse.current.middleButton.isPressed;
                    }
                }

#else

                if (_IsXboxControllerEnabled)
                {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    lValue = (UnityEngine.Input.GetAxis("MXRightStickX") != 0f);
                    if (!lValue) { lValue = (UnityEngine.Input.GetAxis("MXRightStickY") != 0f); }
#else
                    lValue = (UnityEngine.Input.GetAxis("WXRightStickX") != 0f);
                    if (!lValue) { lValue = (UnityEngine.Input.GetAxis("WXRightStickY") != 0f); }
#endif
                }

                if (!lValue)
                {
                    if (_ViewActivator == 0)
                    {
                        lValue = true;
                    }
                    else if (_ViewActivator == 1)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(0);
                    }
                    else if (_ViewActivator == 2)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(1);
                    }
                    else if (_ViewActivator == 3)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(0);
                        if (!lValue) { lValue = UnityEngine.Input.GetMouseButton(1); }
                    }
                    else if (_ViewActivator == 4)
                    {
                        lValue = UnityEngine.Input.GetMouseButton(2);
                    }
                }

#endif

                return lValue;
            }
        }

        /// <summary>
        /// Key or button used to allow view to be activated
        /// 0 = none
        /// 1 = left mouse button
        /// 2 = right mouse button
        /// 3 = left and right mouse button
        /// 4 = middle mouse button
        /// </summary>
        [Tooltip("Determines what button enables viewing.")]
        public int _ViewActivator = 2;
        public int ViewActivator
        {
            get { return _ViewActivator; }
            set { _ViewActivator = value; }
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsJustPressed(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }

#if ENABLE_INPUT_SYSTEM

            if (rKey == KeyCode.Mouse0)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse1)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse2)
            {
                if (Mouse.current.middleButton.wasPressedThisFrame)
                {
                    return true;
                }
            }
            else
            {
                Key lKey = ToKeyFromKeyCode(rKey);
                if (lKey != Key.None)
                {
                    KeyControl lKeyControl = Keyboard.current.allKeys[(int)lKey - 1];
                    if (lKeyControl.wasPressedThisFrame)
                    {
                        return true;
                    }
                }
            }

            return false;

#else
            return UnityEngine.Input.GetKeyDown(rKey);
#endif
        }

        /// <summary>
        /// Test if a specific key is pressed this frame.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustPressed(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return IsJustPressed((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is pressed this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustPressed(string rAction)
        {
            if (!_IsEnabled) { return false; }

#if ENABLE_INPUT_SYSTEM

            if (_PlayerInput == null) { return false; }

            InputAction lAction = _PlayerInput.actions.FindAction(rAction);
            if (lAction != null && lAction.triggered)
            {
                return true;
            }

            return false;

#else

            try
            {
                return UnityEngine.Input.GetButtonDown(rAction);
            }
            catch
            {
                return false;
            }

#endif
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsPressed(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }

#if ENABLE_INPUT_SYSTEM

            if (rKey == KeyCode.Mouse0)
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse1)
            {
                if (Mouse.current.rightButton.isPressed)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse2)
            {
                if (Mouse.current.middleButton.isPressed)
                {
                    return true;
                }
            }
            else
            {
                Key lKey = ToKeyFromKeyCode(rKey);
                if (lKey != Key.None)
                {
                    KeyControl lKeyControl = Keyboard.current.allKeys[(int)lKey - 1];
                    if (lKeyControl.isPressed)
                    {
                        return true;
                    }
                }
            }

            return false;

#else
            return UnityEngine.Input.GetKey(rKey);
#endif
        }

        /// <summary>
        /// Test if a specific key is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsPressed(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return IsPressed((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsPressed(string rAction)
        {
            if (!_IsEnabled) { return false; }

#if ENABLE_INPUT_SYSTEM

            if (_PlayerInput == null) { return false; }

            InputAction lAction = _PlayerInput.actions.FindAction(rAction);
            if (lAction != null)
            {
                if (lAction.controls.Count > 0)
                {
                    if (lAction.controls[0].IsPressed())
                    {
                        return true;
                    }
                }
                else if (lAction.triggered)
                {
                    return true;
                }
            }

            return false;

#else

            try
            {
                bool lValue = UnityEngine.Input.GetButton(rAction);
                if (!lValue) { lValue = (UnityEngine.Input.GetAxis(rAction) != 0f); }

                return lValue;
            }
            catch
            {
                return false;
            }

#endif
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsJustReleased(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }


#if ENABLE_INPUT_SYSTEM

            if (rKey == KeyCode.Mouse0)
            {
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse1)
            {
                if (Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    return true;
                }
            }
            else if (rKey == KeyCode.Mouse2)
            {
                if (Mouse.current.middleButton.wasReleasedThisFrame)
                {
                    return true;
                }
            }
            else
            {
                Key lKey = ToKeyFromKeyCode(rKey);
                if (lKey != Key.None)
                {
                    KeyControl lKeyControl = Keyboard.current.allKeys[(int)lKey - 1];
                    if (lKeyControl.wasReleasedThisFrame)
                    {
                        return true;
                    }
                }
            }

            return false;

#else
            return UnityEngine.Input.GetKeyUp(rKey);
#endif
        }

        /// <summary>
        /// Test if a specific key is released this frame.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustReleased(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return IsJustReleased((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is released this frame.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action just took place</returns>
        public virtual bool IsJustReleased(string rAction)
        {
            if (!_IsEnabled) { return false; }

#if ENABLE_INPUT_SYSTEM

            if (_PlayerInput == null) { return false; }

            InputAction lAction = _PlayerInput.actions.FindAction(rAction);
            if (lAction != null && lAction.controls.Count > 0)
            {
                ButtonControl lButtonControl = lAction.controls[0] as ButtonControl;
                if (lButtonControl != null)
                {
                    return lButtonControl.wasReleasedThisFrame;
                }
            }

            return false;

#else

            try
            {
                return UnityEngine.Input.GetButtonUp(rAction);
            }
            catch
            {
                return false;
            }

#endif
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public virtual bool IsReleased(KeyCode rKey)
        {
            if (!_IsEnabled) { return false; }
            return !IsPressed(rKey);
        }

        /// <summary>
        /// Test if a specific key is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rEnumInput">Input Manager enumerated key to test</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsReleased(int rKey)
        {
            if (!_IsEnabled) { return false; }
            return !IsPressed((KeyCode)rKey);
        }

        /// <summary>
        /// Test if a specific action is not pressed. This is used for continuous checking.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Boolean that determines if the action is taking place</returns>
        public virtual bool IsReleased(string rAction)
        {
            if (!_IsEnabled) { return false; }
            return !IsPressed(rAction);
        }

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rKey">Input Manager enumerated key to test</param>
        /// <returns>Float value as determined by the key</returns>
        public virtual float GetValue(int rKey)
        {
            return 0f;
        }

        /// <summary>
        /// Test for a specific action value.
        /// </summary>
        /// <param name="rAction">Action to test for</param>
        /// <returns>Float value as determined by the action</returns>
        public virtual float GetValue(string rAction)
        {

#if ENABLE_INPUT_SYSTEM

            if (_PlayerInput == null) { return 0f; }

            InputAction lAction = _PlayerInput.actions.FindAction(rAction);
            if (lAction != null)
            {
                if (lAction.expectedControlType.Equals("Vector2"))
                {
                    Vector2 lValue = lAction.ReadValue<Vector2>();
                    if (lValue.x != 0f) { return lValue.x; }
                    return lValue.y;
                }

                return lAction.ReadValue<float>();
            }

            return 0f;

#else

            try
            {
                return UnityEngine.Input.GetAxis(rAction);
            }
            catch
            {
                return 0f;
            }

#endif
        }

#if ENABLE_INPUT_SYSTEM

        /// <summary>
        /// Used to support the conversion from the old KeyCode to the new Key
        /// </summary>
        /// <param name="rKey"></param>
        /// <returns></returns>
        public Key ToKeyFromKeyCode(KeyCode rKey)
        {
            if (rKey == KeyCode.A) { return Key.A; }
            if (rKey == KeyCode.B) { return Key.B; }
            if (rKey == KeyCode.C) { return Key.C; }
            if (rKey == KeyCode.D) { return Key.D; }
            if (rKey == KeyCode.E) { return Key.E; }
            if (rKey == KeyCode.F) { return Key.F; }
            if (rKey == KeyCode.G) { return Key.G; }
            if (rKey == KeyCode.H) { return Key.H; }
            if (rKey == KeyCode.I) { return Key.I; }
            if (rKey == KeyCode.J) { return Key.J; }
            if (rKey == KeyCode.K) { return Key.K; }
            if (rKey == KeyCode.L) { return Key.L; }
            if (rKey == KeyCode.M) { return Key.M; }
            if (rKey == KeyCode.N) { return Key.N; }
            if (rKey == KeyCode.O) { return Key.O; }
            if (rKey == KeyCode.P) { return Key.P; }
            if (rKey == KeyCode.Q) { return Key.Q; }
            if (rKey == KeyCode.R) { return Key.R; }
            if (rKey == KeyCode.S) { return Key.S; }
            if (rKey == KeyCode.T) { return Key.T; }
            if (rKey == KeyCode.U) { return Key.U; }
            if (rKey == KeyCode.V) { return Key.V; }
            if (rKey == KeyCode.W) { return Key.W; }
            if (rKey == KeyCode.X) { return Key.X; }
            if (rKey == KeyCode.Y) { return Key.Y; }
            if (rKey == KeyCode.Z) { return Key.Z; }

            if (rKey == KeyCode.DownArrow) { return Key.DownArrow; }
            if (rKey == KeyCode.UpArrow) { return Key.UpArrow; }
            if (rKey == KeyCode.LeftArrow) { return Key.LeftArrow; }
            if (rKey == KeyCode.RightArrow) { return Key.RightArrow; }

            if (rKey == KeyCode.Alpha1) { return Key.Digit1; }
            if (rKey == KeyCode.Alpha2) { return Key.Digit2; }
            if (rKey == KeyCode.Alpha3) { return Key.Digit3; }
            if (rKey == KeyCode.Alpha4) { return Key.Digit4; }
            if (rKey == KeyCode.Alpha5) { return Key.Digit5; }
            if (rKey == KeyCode.Alpha6) { return Key.Digit6; }
            if (rKey == KeyCode.Alpha7) { return Key.Digit7; }
            if (rKey == KeyCode.Alpha8) { return Key.Digit8; }
            if (rKey == KeyCode.Alpha9) { return Key.Digit9; }
            if (rKey == KeyCode.Alpha0) { return Key.Digit0; }

            if (rKey == KeyCode.None) { return Key.None; }

            if (rKey == KeyCode.Space) { return Key.Space; }
            if (rKey == KeyCode.RightAlt) { return Key.RightAlt; }
            if (rKey == KeyCode.LeftAlt) { return Key.LeftAlt; }
            if (rKey == KeyCode.RightShift) { return Key.RightShift; }
            if (rKey == KeyCode.LeftShift) { return Key.LeftShift; }
            if (rKey == KeyCode.RightCurlyBracket) { return Key.RightBracket; }
            if (rKey == KeyCode.LeftCurlyBracket) { return Key.LeftBracket; }
            if (rKey == KeyCode.RightControl) { return Key.RightCtrl; }
            if (rKey == KeyCode.LeftControl) { return Key.LeftCtrl; }

            if (rKey == KeyCode.BackQuote) { return Key.Backquote; }
            if (rKey == KeyCode.Backslash) { return Key.Backslash; }
            if (rKey == KeyCode.Backspace) { return Key.Backspace; }
            if (rKey == KeyCode.CapsLock) { return Key.CapsLock; }
            if (rKey == KeyCode.Menu) { return Key.ContextMenu; }
            if (rKey == KeyCode.Comma) { return Key.Comma; }
            if (rKey == KeyCode.Colon) { return Key.Semicolon; }
            if (rKey == KeyCode.Period) { return Key.Period; }
            if (rKey == KeyCode.Quote) { return Key.Quote; }
            if (rKey == KeyCode.Question) { return Key.Slash; }
            if (rKey == KeyCode.PageDown) { return Key.PageDown; }
            if (rKey == KeyCode.PageUp) { return Key.PageUp; }

            if (rKey == KeyCode.End) { return Key.End; }
            if (rKey == KeyCode.Return) { return Key.Enter; }
            if (rKey == KeyCode.Escape) { return Key.Escape; }
            if (rKey == KeyCode.Tab) { return Key.Tab; }
            if (rKey == KeyCode.Delete) { return Key.Delete; }
            if (rKey == KeyCode.Home) { return Key.Home; }
            if (rKey == KeyCode.Insert) { return Key.Insert; }
            if (rKey == KeyCode.Minus) { return Key.Minus; }
            if (rKey == KeyCode.Plus) { return Key.Equals; }
            if (rKey == KeyCode.CapsLock) { return Key.CapsLock; }
            if (rKey == KeyCode.Numlock) { return Key.NumLock; }
            if (rKey == KeyCode.ScrollLock) { return Key.ScrollLock; }
            if (rKey == KeyCode.Print) { return Key.PrintScreen; }
            if (rKey == KeyCode.Pause) { return Key.Pause; }

            if (rKey == KeyCode.Keypad1) { return Key.Numpad1; }
            if (rKey == KeyCode.Keypad2) { return Key.Numpad2; }
            if (rKey == KeyCode.Keypad3) { return Key.Numpad3; }
            if (rKey == KeyCode.Keypad4) { return Key.Numpad4; }
            if (rKey == KeyCode.Keypad5) { return Key.Numpad5; }
            if (rKey == KeyCode.Keypad6) { return Key.Numpad6; }
            if (rKey == KeyCode.Keypad7) { return Key.Numpad7; }
            if (rKey == KeyCode.Keypad8) { return Key.Numpad8; }
            if (rKey == KeyCode.Keypad9) { return Key.Numpad9; }
            if (rKey == KeyCode.Keypad0) { return Key.Numpad0; }
            if (rKey == KeyCode.KeypadEnter) { return Key.NumpadEnter; }
            if (rKey == KeyCode.KeypadDivide) { return Key.NumpadDivide; }
            if (rKey == KeyCode.KeypadMultiply) { return Key.NumpadMultiply; }
            if (rKey == KeyCode.KeypadPlus) { return Key.NumpadPlus; }
            if (rKey == KeyCode.KeypadMinus) { return Key.NumpadMinus; }
            if (rKey == KeyCode.KeypadPeriod) { return Key.NumpadPeriod; }
            if (rKey == KeyCode.KeypadEquals) { return Key.NumpadEquals; }

            if (rKey == KeyCode.F1) { return Key.F1; }
            if (rKey == KeyCode.F2) { return Key.F2; }
            if (rKey == KeyCode.F3) { return Key.F3; }
            if (rKey == KeyCode.F4) { return Key.F4; }
            if (rKey == KeyCode.F5) { return Key.F5; }
            if (rKey == KeyCode.F6) { return Key.F6; }
            if (rKey == KeyCode.F7) { return Key.F7; }
            if (rKey == KeyCode.F8) { return Key.F8; }
            if (rKey == KeyCode.F9) { return Key.F9; }
            if (rKey == KeyCode.F10) { return Key.F10; }
            if (rKey == KeyCode.F11) { return Key.F11; }
            if (rKey == KeyCode.F12) { return Key.F12; }

            //AltGr = 54,
            //LeftMeta = 57,
            //LeftWindows = 57,
            //LeftApple = 57,
            //LeftCommand = 57,
            //RightMeta = 58,
            //RightWindows = 58,
            //RightApple = 58,
            //RightCommand = 58,

            //OEM1 = 106,
            //OEM2 = 107,
            //OEM3 = 108,
            //OEM4 = 109,
            //OEM5 = 110,
            //IMESelected = 111

            return Key.None;
        }

#endif


        /// <summary>
        /// Show the options section in the editor
        /// </summary>
        public bool EditorShowOptions = false;
    }

#if false && !ENABLE_LEGACY_INPUT_MANAGER

    /// <summary>
    /// Used if the legacy Unity Manager KeyCode doesn't exist
    /// </summary>
    public enum KeyCode
    {
        //
        // Summary:
        //     Not assigned (never returned as the result of a keystroke).
        None = 0,
        //
        // Summary:
        //     The backspace key.
        Backspace = 8,
        //
        // Summary:
        //     The tab key.
        Tab = 9,
        //
        // Summary:
        //     The Clear key.
        Clear = 12,
        //
        // Summary:
        //     Return key.
        Return = 13,
        //
        // Summary:
        //     Pause on PC machines.
        Pause = 19,
        //
        // Summary:
        //     Escape key.
        Escape = 27,
        //
        // Summary:
        //     Space key.
        Space = 32,
        //
        // Summary:
        //     Exclamation mark key '!'.
        Exclaim = 33,
        //
        // Summary:
        //     Double quote key '"'.
        DoubleQuote = 34,
        //
        // Summary:
        //     Hash key '#'.
        Hash = 35,
        //
        // Summary:
        //     Dollar sign key '$'.
        Dollar = 36,
        //
        // Summary:
        //     Percent '%' key.
        Percent = 37,
        //
        // Summary:
        //     Ampersand key '&'.
        Ampersand = 38,
        //
        // Summary:
        //     Quote key '.
        Quote = 39,
        //
        // Summary:
        //     Left Parenthesis key '('.
        LeftParen = 40,
        //
        // Summary:
        //     Right Parenthesis key ')'.
        RightParen = 41,
        //
        // Summary:
        //     Asterisk key '*'.
        Asterisk = 42,
        //
        // Summary:
        //     Plus key '+'.
        Plus = 43,
        //
        // Summary:
        //     Comma ',' key.
        Comma = 44,
        //
        // Summary:
        //     Minus '-' key.
        Minus = 45,
        //
        // Summary:
        //     Period '.' key.
        Period = 46,
        //
        // Summary:
        //     Slash '/' key.
        Slash = 47,
        //
        // Summary:
        //     The '0' key on the top of the alphanumeric keyboard.
        Alpha0 = 48,
        //
        // Summary:
        //     The '1' key on the top of the alphanumeric keyboard.
        Alpha1 = 49,
        //
        // Summary:
        //     The '2' key on the top of the alphanumeric keyboard.
        Alpha2 = 50,
        //
        // Summary:
        //     The '3' key on the top of the alphanumeric keyboard.
        Alpha3 = 51,
        //
        // Summary:
        //     The '4' key on the top of the alphanumeric keyboard.
        Alpha4 = 52,
        //
        // Summary:
        //     The '5' key on the top of the alphanumeric keyboard.
        Alpha5 = 53,
        //
        // Summary:
        //     The '6' key on the top of the alphanumeric keyboard.
        Alpha6 = 54,
        //
        // Summary:
        //     The '7' key on the top of the alphanumeric keyboard.
        Alpha7 = 55,
        //
        // Summary:
        //     The '8' key on the top of the alphanumeric keyboard.
        Alpha8 = 56,
        //
        // Summary:
        //     The '9' key on the top of the alphanumeric keyboard.
        Alpha9 = 57,
        //
        // Summary:
        //     Colon ':' key.
        Colon = 58,
        //
        // Summary:
        //     Semicolon ';' key.
        Semicolon = 59,
        //
        // Summary:
        //     Less than '<' key.
        Less = 60,
        //
        // Summary:
        //     Equals '=' key.
        Equals = 61,
        //
        // Summary:
        //     Greater than '>' key.
        Greater = 62,
        //
        // Summary:
        //     Question mark '?' key.
        Question = 63,
        //
        // Summary:
        //     At key '@'.
        At = 64,
        //
        // Summary:
        //     Left square bracket key '['.
        LeftBracket = 91,
        //
        // Summary:
        //     Backslash key '\'.
        Backslash = 92,
        //
        // Summary:
        //     Right square bracket key ']'.
        RightBracket = 93,
        //
        // Summary:
        //     Caret key '^'.
        Caret = 94,
        //
        // Summary:
        //     Underscore '_' key.
        Underscore = 95,
        //
        // Summary:
        //     Back quote key '`'.
        BackQuote = 96,
        //
        // Summary:
        //     'a' key.
        A = 97,
        //
        // Summary:
        //     'b' key.
        B = 98,
        //
        // Summary:
        //     'c' key.
        C = 99,
        //
        // Summary:
        //     'd' key.
        D = 100,
        //
        // Summary:
        //     'e' key.
        E = 101,
        //
        // Summary:
        //     'f' key.
        F = 102,
        //
        // Summary:
        //     'g' key.
        G = 103,
        //
        // Summary:
        //     'h' key.
        H = 104,
        //
        // Summary:
        //     'i' key.
        I = 105,
        //
        // Summary:
        //     'j' key.
        J = 106,
        //
        // Summary:
        //     'k' key.
        K = 107,
        //
        // Summary:
        //     'l' key.
        L = 108,
        //
        // Summary:
        //     'm' key.
        M = 109,
        //
        // Summary:
        //     'n' key.
        N = 110,
        //
        // Summary:
        //     'o' key.
        O = 111,
        //
        // Summary:
        //     'p' key.
        P = 112,
        //
        // Summary:
        //     'q' key.
        Q = 113,
        //
        // Summary:
        //     'r' key.
        R = 114,
        //
        // Summary:
        //     's' key.
        S = 115,
        //
        // Summary:
        //     't' key.
        T = 116,
        //
        // Summary:
        //     'u' key.
        U = 117,
        //
        // Summary:
        //     'v' key.
        V = 118,
        //
        // Summary:
        //     'w' key.
        W = 119,
        //
        // Summary:
        //     'x' key.
        X = 120,
        //
        // Summary:
        //     'y' key.
        Y = 121,
        //
        // Summary:
        //     'z' key.
        Z = 122,
        //
        // Summary:
        //     Left curly bracket key '{'.
        LeftCurlyBracket = 123,
        //
        // Summary:
        //     Pipe '|' key.
        Pipe = 124,
        //
        // Summary:
        //     Right curly bracket key '}'.
        RightCurlyBracket = 125,
        //
        // Summary:
        //     Tilde '~' key.
        Tilde = 126,
        //
        // Summary:
        //     The forward delete key.
        Delete = 127,
        //
        // Summary:
        //     Numeric keypad 0.
        Keypad0 = 256,
        //
        // Summary:
        //     Numeric keypad 1.
        Keypad1 = 257,
        //
        // Summary:
        //     Numeric keypad 2.
        Keypad2 = 258,
        //
        // Summary:
        //     Numeric keypad 3.
        Keypad3 = 259,
        //
        // Summary:
        //     Numeric keypad 4.
        Keypad4 = 260,
        //
        // Summary:
        //     Numeric keypad 5.
        Keypad5 = 261,
        //
        // Summary:
        //     Numeric keypad 6.
        Keypad6 = 262,
        //
        // Summary:
        //     Numeric keypad 7.
        Keypad7 = 263,
        //
        // Summary:
        //     Numeric keypad 8.
        Keypad8 = 264,
        //
        // Summary:
        //     Numeric keypad 9.
        Keypad9 = 265,
        //
        // Summary:
        //     Numeric keypad '.'.
        KeypadPeriod = 266,
        //
        // Summary:
        //     Numeric keypad '/'.
        KeypadDivide = 267,
        //
        // Summary:
        //     Numeric keypad '*'.
        KeypadMultiply = 268,
        //
        // Summary:
        //     Numeric keypad '-'.
        KeypadMinus = 269,
        //
        // Summary:
        //     Numeric keypad '+'.
        KeypadPlus = 270,
        //
        // Summary:
        //     Numeric keypad Enter.
        KeypadEnter = 271,
        //
        // Summary:
        //     Numeric keypad '='.
        KeypadEquals = 272,
        //
        // Summary:
        //     Up arrow key.
        UpArrow = 273,
        //
        // Summary:
        //     Down arrow key.
        DownArrow = 274,
        //
        // Summary:
        //     Right arrow key.
        RightArrow = 275,
        //
        // Summary:
        //     Left arrow key.
        LeftArrow = 276,
        //
        // Summary:
        //     Insert key key.
        Insert = 277,
        //
        // Summary:
        //     Home key.
        Home = 278,
        //
        // Summary:
        //     End key.
        End = 279,
        //
        // Summary:
        //     Page up.
        PageUp = 280,
        //
        // Summary:
        //     Page down.
        PageDown = 281,
        //
        // Summary:
        //     F1 function key.
        F1 = 282,
        //
        // Summary:
        //     F2 function key.
        F2 = 283,
        //
        // Summary:
        //     F3 function key.
        F3 = 284,
        //
        // Summary:
        //     F4 function key.
        F4 = 285,
        //
        // Summary:
        //     F5 function key.
        F5 = 286,
        //
        // Summary:
        //     F6 function key.
        F6 = 287,
        //
        // Summary:
        //     F7 function key.
        F7 = 288,
        //
        // Summary:
        //     F8 function key.
        F8 = 289,
        //
        // Summary:
        //     F9 function key.
        F9 = 290,
        //
        // Summary:
        //     F10 function key.
        F10 = 291,
        //
        // Summary:
        //     F11 function key.
        F11 = 292,
        //
        // Summary:
        //     F12 function key.
        F12 = 293,
        //
        // Summary:
        //     F13 function key.
        F13 = 294,
        //
        // Summary:
        //     F14 function key.
        F14 = 295,
        //
        // Summary:
        //     F15 function key.
        F15 = 296,
        //
        // Summary:
        //     Numlock key.
        Numlock = 300,
        //
        // Summary:
        //     Capslock key.
        CapsLock = 301,
        //
        // Summary:
        //     Scroll lock key.
        ScrollLock = 302,
        //
        // Summary:
        //     Right shift key.
        RightShift = 303,
        //
        // Summary:
        //     Left shift key.
        LeftShift = 304,
        //
        // Summary:
        //     Right Control key.
        RightControl = 305,
        //
        // Summary:
        //     Left Control key.
        LeftControl = 306,
        //
        // Summary:
        //     Right Alt key.
        RightAlt = 307,
        //
        // Summary:
        //     Left Alt key.
        LeftAlt = 308,
        //
        // Summary:
        //     Right Command key.
        RightCommand = 309,
        //
        // Summary:
        //     Right Command key.
        RightApple = 309,
        //
        // Summary:
        //     Left Command key.
        LeftCommand = 310,
        //
        // Summary:
        //     Left Command key.
        LeftApple = 310,
        //
        // Summary:
        //     Left Windows key.
        LeftWindows = 311,
        //
        // Summary:
        //     Right Windows key.
        RightWindows = 312,
        //
        // Summary:
        //     Alt Gr key.
        AltGr = 313,
        //
        // Summary:
        //     Help key.
        Help = 315,
        //
        // Summary:
        //     Print key.
        Print = 316,
        //
        // Summary:
        //     Sys Req key.
        SysReq = 317,
        //
        // Summary:
        //     Break key.
        Break = 318,
        //
        // Summary:
        //     Menu key.
        Menu = 319,
        //
        // Summary:
        //     The Left (or primary) mouse button.
        Mouse0 = 323,
        //
        // Summary:
        //     Right mouse button (or secondary mouse button).
        Mouse1 = 324,
        //
        // Summary:
        //     Middle mouse button (or third button).
        Mouse2 = 325,
        //
        // Summary:
        //     Additional (fourth) mouse button.
        Mouse3 = 326,
        //
        // Summary:
        //     Additional (fifth) mouse button.
        Mouse4 = 327,
        //
        // Summary:
        //     Additional (or sixth) mouse button.
        Mouse5 = 328,
        //
        // Summary:
        //     Additional (or seventh) mouse button.
        Mouse6 = 329,
        //
        // Summary:
        //     Button 0 on any joystick.
        JoystickButton0 = 330,
        //
        // Summary:
        //     Button 1 on any joystick.
        JoystickButton1 = 331,
        //
        // Summary:
        //     Button 2 on any joystick.
        JoystickButton2 = 332,
        //
        // Summary:
        //     Button 3 on any joystick.
        JoystickButton3 = 333,
        //
        // Summary:
        //     Button 4 on any joystick.
        JoystickButton4 = 334,
        //
        // Summary:
        //     Button 5 on any joystick.
        JoystickButton5 = 335,
        //
        // Summary:
        //     Button 6 on any joystick.
        JoystickButton6 = 336,
        //
        // Summary:
        //     Button 7 on any joystick.
        JoystickButton7 = 337,
        //
        // Summary:
        //     Button 8 on any joystick.
        JoystickButton8 = 338,
        //
        // Summary:
        //     Button 9 on any joystick.
        JoystickButton9 = 339,
        //
        // Summary:
        //     Button 10 on any joystick.
        JoystickButton10 = 340,
        //
        // Summary:
        //     Button 11 on any joystick.
        JoystickButton11 = 341,
        //
        // Summary:
        //     Button 12 on any joystick.
        JoystickButton12 = 342,
        //
        // Summary:
        //     Button 13 on any joystick.
        JoystickButton13 = 343,
        //
        // Summary:
        //     Button 14 on any joystick.
        JoystickButton14 = 344,
        //
        // Summary:
        //     Button 15 on any joystick.
        JoystickButton15 = 345,
        //
        // Summary:
        //     Button 16 on any joystick.
        JoystickButton16 = 346,
        //
        // Summary:
        //     Button 17 on any joystick.
        JoystickButton17 = 347,
        //
        // Summary:
        //     Button 18 on any joystick.
        JoystickButton18 = 348,
        //
        // Summary:
        //     Button 19 on any joystick.
        JoystickButton19 = 349,
        //
        // Summary:
        //     Button 0 on first joystick.
        Joystick1Button0 = 350,
        //
        // Summary:
        //     Button 1 on first joystick.
        Joystick1Button1 = 351,
        //
        // Summary:
        //     Button 2 on first joystick.
        Joystick1Button2 = 352,
        //
        // Summary:
        //     Button 3 on first joystick.
        Joystick1Button3 = 353,
        //
        // Summary:
        //     Button 4 on first joystick.
        Joystick1Button4 = 354,
        //
        // Summary:
        //     Button 5 on first joystick.
        Joystick1Button5 = 355,
        //
        // Summary:
        //     Button 6 on first joystick.
        Joystick1Button6 = 356,
        //
        // Summary:
        //     Button 7 on first joystick.
        Joystick1Button7 = 357,
        //
        // Summary:
        //     Button 8 on first joystick.
        Joystick1Button8 = 358,
        //
        // Summary:
        //     Button 9 on first joystick.
        Joystick1Button9 = 359,
        //
        // Summary:
        //     Button 10 on first joystick.
        Joystick1Button10 = 360,
        //
        // Summary:
        //     Button 11 on first joystick.
        Joystick1Button11 = 361,
        //
        // Summary:
        //     Button 12 on first joystick.
        Joystick1Button12 = 362,
        //
        // Summary:
        //     Button 13 on first joystick.
        Joystick1Button13 = 363,
        //
        // Summary:
        //     Button 14 on first joystick.
        Joystick1Button14 = 364,
        //
        // Summary:
        //     Button 15 on first joystick.
        Joystick1Button15 = 365,
        //
        // Summary:
        //     Button 16 on first joystick.
        Joystick1Button16 = 366,
        //
        // Summary:
        //     Button 17 on first joystick.
        Joystick1Button17 = 367,
        //
        // Summary:
        //     Button 18 on first joystick.
        Joystick1Button18 = 368,
        //
        // Summary:
        //     Button 19 on first joystick.
        Joystick1Button19 = 369,
        //
        // Summary:
        //     Button 0 on second joystick.
        Joystick2Button0 = 370,
        //
        // Summary:
        //     Button 1 on second joystick.
        Joystick2Button1 = 371,
        //
        // Summary:
        //     Button 2 on second joystick.
        Joystick2Button2 = 372,
        //
        // Summary:
        //     Button 3 on second joystick.
        Joystick2Button3 = 373,
        //
        // Summary:
        //     Button 4 on second joystick.
        Joystick2Button4 = 374,
        //
        // Summary:
        //     Button 5 on second joystick.
        Joystick2Button5 = 375,
        //
        // Summary:
        //     Button 6 on second joystick.
        Joystick2Button6 = 376,
        //
        // Summary:
        //     Button 7 on second joystick.
        Joystick2Button7 = 377,
        //
        // Summary:
        //     Button 8 on second joystick.
        Joystick2Button8 = 378,
        //
        // Summary:
        //     Button 9 on second joystick.
        Joystick2Button9 = 379,
        //
        // Summary:
        //     Button 10 on second joystick.
        Joystick2Button10 = 380,
        //
        // Summary:
        //     Button 11 on second joystick.
        Joystick2Button11 = 381,
        //
        // Summary:
        //     Button 12 on second joystick.
        Joystick2Button12 = 382,
        //
        // Summary:
        //     Button 13 on second joystick.
        Joystick2Button13 = 383,
        //
        // Summary:
        //     Button 14 on second joystick.
        Joystick2Button14 = 384,
        //
        // Summary:
        //     Button 15 on second joystick.
        Joystick2Button15 = 385,
        //
        // Summary:
        //     Button 16 on second joystick.
        Joystick2Button16 = 386,
        //
        // Summary:
        //     Button 17 on second joystick.
        Joystick2Button17 = 387,
        //
        // Summary:
        //     Button 18 on second joystick.
        Joystick2Button18 = 388,
        //
        // Summary:
        //     Button 19 on second joystick.
        Joystick2Button19 = 389,
        //
        // Summary:
        //     Button 0 on third joystick.
        Joystick3Button0 = 390,
        //
        // Summary:
        //     Button 1 on third joystick.
        Joystick3Button1 = 391,
        //
        // Summary:
        //     Button 2 on third joystick.
        Joystick3Button2 = 392,
        //
        // Summary:
        //     Button 3 on third joystick.
        Joystick3Button3 = 393,
        //
        // Summary:
        //     Button 4 on third joystick.
        Joystick3Button4 = 394,
        //
        // Summary:
        //     Button 5 on third joystick.
        Joystick3Button5 = 395,
        //
        // Summary:
        //     Button 6 on third joystick.
        Joystick3Button6 = 396,
        //
        // Summary:
        //     Button 7 on third joystick.
        Joystick3Button7 = 397,
        //
        // Summary:
        //     Button 8 on third joystick.
        Joystick3Button8 = 398,
        //
        // Summary:
        //     Button 9 on third joystick.
        Joystick3Button9 = 399,
        //
        // Summary:
        //     Button 10 on third joystick.
        Joystick3Button10 = 400,
        //
        // Summary:
        //     Button 11 on third joystick.
        Joystick3Button11 = 401,
        //
        // Summary:
        //     Button 12 on third joystick.
        Joystick3Button12 = 402,
        //
        // Summary:
        //     Button 13 on third joystick.
        Joystick3Button13 = 403,
        //
        // Summary:
        //     Button 14 on third joystick.
        Joystick3Button14 = 404,
        //
        // Summary:
        //     Button 15 on third joystick.
        Joystick3Button15 = 405,
        //
        // Summary:
        //     Button 16 on third joystick.
        Joystick3Button16 = 406,
        //
        // Summary:
        //     Button 17 on third joystick.
        Joystick3Button17 = 407,
        //
        // Summary:
        //     Button 18 on third joystick.
        Joystick3Button18 = 408,
        //
        // Summary:
        //     Button 19 on third joystick.
        Joystick3Button19 = 409,
        //
        // Summary:
        //     Button 0 on forth joystick.
        Joystick4Button0 = 410,
        //
        // Summary:
        //     Button 1 on forth joystick.
        Joystick4Button1 = 411,
        //
        // Summary:
        //     Button 2 on forth joystick.
        Joystick4Button2 = 412,
        //
        // Summary:
        //     Button 3 on forth joystick.
        Joystick4Button3 = 413,
        //
        // Summary:
        //     Button 4 on forth joystick.
        Joystick4Button4 = 414,
        //
        // Summary:
        //     Button 5 on forth joystick.
        Joystick4Button5 = 415,
        //
        // Summary:
        //     Button 6 on forth joystick.
        Joystick4Button6 = 416,
        //
        // Summary:
        //     Button 7 on forth joystick.
        Joystick4Button7 = 417,
        //
        // Summary:
        //     Button 8 on forth joystick.
        Joystick4Button8 = 418,
        //
        // Summary:
        //     Button 9 on forth joystick.
        Joystick4Button9 = 419,
        //
        // Summary:
        //     Button 10 on forth joystick.
        Joystick4Button10 = 420,
        //
        // Summary:
        //     Button 11 on forth joystick.
        Joystick4Button11 = 421,
        //
        // Summary:
        //     Button 12 on forth joystick.
        Joystick4Button12 = 422,
        //
        // Summary:
        //     Button 13 on forth joystick.
        Joystick4Button13 = 423,
        //
        // Summary:
        //     Button 14 on forth joystick.
        Joystick4Button14 = 424,
        //
        // Summary:
        //     Button 15 on forth joystick.
        Joystick4Button15 = 425,
        //
        // Summary:
        //     Button 16 on forth joystick.
        Joystick4Button16 = 426,
        //
        // Summary:
        //     Button 17 on forth joystick.
        Joystick4Button17 = 427,
        //
        // Summary:
        //     Button 18 on forth joystick.
        Joystick4Button18 = 428,
        //
        // Summary:
        //     Button 19 on forth joystick.
        Joystick4Button19 = 429,
        //
        // Summary:
        //     Button 0 on fifth joystick.
        Joystick5Button0 = 430,
        //
        // Summary:
        //     Button 1 on fifth joystick.
        Joystick5Button1 = 431,
        //
        // Summary:
        //     Button 2 on fifth joystick.
        Joystick5Button2 = 432,
        //
        // Summary:
        //     Button 3 on fifth joystick.
        Joystick5Button3 = 433,
        //
        // Summary:
        //     Button 4 on fifth joystick.
        Joystick5Button4 = 434,
        //
        // Summary:
        //     Button 5 on fifth joystick.
        Joystick5Button5 = 435,
        //
        // Summary:
        //     Button 6 on fifth joystick.
        Joystick5Button6 = 436,
        //
        // Summary:
        //     Button 7 on fifth joystick.
        Joystick5Button7 = 437,
        //
        // Summary:
        //     Button 8 on fifth joystick.
        Joystick5Button8 = 438,
        //
        // Summary:
        //     Button 9 on fifth joystick.
        Joystick5Button9 = 439,
        //
        // Summary:
        //     Button 10 on fifth joystick.
        Joystick5Button10 = 440,
        //
        // Summary:
        //     Button 11 on fifth joystick.
        Joystick5Button11 = 441,
        //
        // Summary:
        //     Button 12 on fifth joystick.
        Joystick5Button12 = 442,
        //
        // Summary:
        //     Button 13 on fifth joystick.
        Joystick5Button13 = 443,
        //
        // Summary:
        //     Button 14 on fifth joystick.
        Joystick5Button14 = 444,
        //
        // Summary:
        //     Button 15 on fifth joystick.
        Joystick5Button15 = 445,
        //
        // Summary:
        //     Button 16 on fifth joystick.
        Joystick5Button16 = 446,
        //
        // Summary:
        //     Button 17 on fifth joystick.
        Joystick5Button17 = 447,
        //
        // Summary:
        //     Button 18 on fifth joystick.
        Joystick5Button18 = 448,
        //
        // Summary:
        //     Button 19 on fifth joystick.
        Joystick5Button19 = 449,
        //
        // Summary:
        //     Button 0 on sixth joystick.
        Joystick6Button0 = 450,
        //
        // Summary:
        //     Button 1 on sixth joystick.
        Joystick6Button1 = 451,
        //
        // Summary:
        //     Button 2 on sixth joystick.
        Joystick6Button2 = 452,
        //
        // Summary:
        //     Button 3 on sixth joystick.
        Joystick6Button3 = 453,
        //
        // Summary:
        //     Button 4 on sixth joystick.
        Joystick6Button4 = 454,
        //
        // Summary:
        //     Button 5 on sixth joystick.
        Joystick6Button5 = 455,
        //
        // Summary:
        //     Button 6 on sixth joystick.
        Joystick6Button6 = 456,
        //
        // Summary:
        //     Button 7 on sixth joystick.
        Joystick6Button7 = 457,
        //
        // Summary:
        //     Button 8 on sixth joystick.
        Joystick6Button8 = 458,
        //
        // Summary:
        //     Button 9 on sixth joystick.
        Joystick6Button9 = 459,
        //
        // Summary:
        //     Button 10 on sixth joystick.
        Joystick6Button10 = 460,
        //
        // Summary:
        //     Button 11 on sixth joystick.
        Joystick6Button11 = 461,
        //
        // Summary:
        //     Button 12 on sixth joystick.
        Joystick6Button12 = 462,
        //
        // Summary:
        //     Button 13 on sixth joystick.
        Joystick6Button13 = 463,
        //
        // Summary:
        //     Button 14 on sixth joystick.
        Joystick6Button14 = 464,
        //
        // Summary:
        //     Button 15 on sixth joystick.
        Joystick6Button15 = 465,
        //
        // Summary:
        //     Button 16 on sixth joystick.
        Joystick6Button16 = 466,
        //
        // Summary:
        //     Button 17 on sixth joystick.
        Joystick6Button17 = 467,
        //
        // Summary:
        //     Button 18 on sixth joystick.
        Joystick6Button18 = 468,
        //
        // Summary:
        //     Button 19 on sixth joystick.
        Joystick6Button19 = 469,
        //
        // Summary:
        //     Button 0 on seventh joystick.
        Joystick7Button0 = 470,
        //
        // Summary:
        //     Button 1 on seventh joystick.
        Joystick7Button1 = 471,
        //
        // Summary:
        //     Button 2 on seventh joystick.
        Joystick7Button2 = 472,
        //
        // Summary:
        //     Button 3 on seventh joystick.
        Joystick7Button3 = 473,
        //
        // Summary:
        //     Button 4 on seventh joystick.
        Joystick7Button4 = 474,
        //
        // Summary:
        //     Button 5 on seventh joystick.
        Joystick7Button5 = 475,
        //
        // Summary:
        //     Button 6 on seventh joystick.
        Joystick7Button6 = 476,
        //
        // Summary:
        //     Button 7 on seventh joystick.
        Joystick7Button7 = 477,
        //
        // Summary:
        //     Button 8 on seventh joystick.
        Joystick7Button8 = 478,
        //
        // Summary:
        //     Button 9 on seventh joystick.
        Joystick7Button9 = 479,
        //
        // Summary:
        //     Button 10 on seventh joystick.
        Joystick7Button10 = 480,
        //
        // Summary:
        //     Button 11 on seventh joystick.
        Joystick7Button11 = 481,
        //
        // Summary:
        //     Button 12 on seventh joystick.
        Joystick7Button12 = 482,
        //
        // Summary:
        //     Button 13 on seventh joystick.
        Joystick7Button13 = 483,
        //
        // Summary:
        //     Button 14 on seventh joystick.
        Joystick7Button14 = 484,
        //
        // Summary:
        //     Button 15 on seventh joystick.
        Joystick7Button15 = 485,
        //
        // Summary:
        //     Button 16 on seventh joystick.
        Joystick7Button16 = 486,
        //
        // Summary:
        //     Button 17 on seventh joystick.
        Joystick7Button17 = 487,
        //
        // Summary:
        //     Button 18 on seventh joystick.
        Joystick7Button18 = 488,
        //
        // Summary:
        //     Button 19 on seventh joystick.
        Joystick7Button19 = 489,
        //
        // Summary:
        //     Button 0 on eighth joystick.
        Joystick8Button0 = 490,
        //
        // Summary:
        //     Button 1 on eighth joystick.
        Joystick8Button1 = 491,
        //
        // Summary:
        //     Button 2 on eighth joystick.
        Joystick8Button2 = 492,
        //
        // Summary:
        //     Button 3 on eighth joystick.
        Joystick8Button3 = 493,
        //
        // Summary:
        //     Button 4 on eighth joystick.
        Joystick8Button4 = 494,
        //
        // Summary:
        //     Button 5 on eighth joystick.
        Joystick8Button5 = 495,
        //
        // Summary:
        //     Button 6 on eighth joystick.
        Joystick8Button6 = 496,
        //
        // Summary:
        //     Button 7 on eighth joystick.
        Joystick8Button7 = 497,
        //
        // Summary:
        //     Button 8 on eighth joystick.
        Joystick8Button8 = 498,
        //
        // Summary:
        //     Button 9 on eighth joystick.
        Joystick8Button9 = 499,
        //
        // Summary:
        //     Button 10 on eighth joystick.
        Joystick8Button10 = 500,
        //
        // Summary:
        //     Button 11 on eighth joystick.
        Joystick8Button11 = 501,
        //
        // Summary:
        //     Button 12 on eighth joystick.
        Joystick8Button12 = 502,
        //
        // Summary:
        //     Button 13 on eighth joystick.
        Joystick8Button13 = 503,
        //
        // Summary:
        //     Button 14 on eighth joystick.
        Joystick8Button14 = 504,
        //
        // Summary:
        //     Button 15 on eighth joystick.
        Joystick8Button15 = 505,
        //
        // Summary:
        //     Button 16 on eighth joystick.
        Joystick8Button16 = 506,
        //
        // Summary:
        //     Button 17 on eighth joystick.
        Joystick8Button17 = 507,
        //
        // Summary:
        //     Button 18 on eighth joystick.
        Joystick8Button18 = 508,
        //
        // Summary:
        //     Button 19 on eighth joystick.
        Joystick8Button19 = 509
    }

#endif

}
