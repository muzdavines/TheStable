using UnityEngine;
using com.ootii.Input;

namespace com.ootii.Actors
{
    public class BasicController : MonoBehaviour
    {
        public GameObject InputSourceOwner = null;

        public Transform Camera = null;

        public bool UseGamepad = false;

        public bool MovementRelative = true;

        public float MovementSpeed = 3f;

        public bool RotationEnabled = true;

        public bool RotateToInput = false;

        public float RotationSpeed = 180f;

        protected Transform mTransform = null;

        protected IInputSource mInputSource = null;

        public void Awake()
        {
            if (InputSourceOwner != null)
            {
                mInputSource = InputSourceOwner.GetComponent<IInputSource>();
            }

            mTransform = gameObject.transform;
        }

        public void Update()
        {
            if (RotationEnabled)
            {
                float lYaw = (GetKey(KeyCode.E) ? 1f : 0f);
                lYaw = lYaw - (GetKey(KeyCode.Q) ? 1f : 0f);

                if (lYaw != 0f)
                {
                    mTransform.rotation = mTransform.rotation * Quaternion.AngleAxis(lYaw * RotationSpeed * Time.deltaTime, Vector3.up);
                }
            }

            Vector3 lMovement = Vector3.zero;

            lMovement.z = (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow) ? 1f : 0f);
            lMovement.z = lMovement.z - (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow) ? 1f : 0f);

            lMovement.x = (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow) ? 1f : 0f);
            lMovement.x = lMovement.x - (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow) ? 1f : 0f);

            if (UseGamepad && lMovement.x == 0f && lMovement.z == 0f)
            {
                lMovement.z = GetAxis("Vertical");
                lMovement.x = GetAxis("Horizontal");
            }

            if (RotateToInput && Camera != null && lMovement.sqrMagnitude > 0f)
            {
                Quaternion lCameraRotation = Quaternion.Euler(0f, Camera.rotation.eulerAngles.y, 0f);
                mTransform.rotation = Quaternion.LookRotation(lCameraRotation * lMovement, Vector3.up);
                lMovement.z = lMovement.magnitude;
                lMovement.x = 0f;
            }

            if (lMovement.magnitude >= 1f) { lMovement.Normalize(); }
            if (MovementRelative) { lMovement = mTransform.rotation * lMovement; }

            mTransform.position = mTransform.position + (lMovement * (MovementSpeed * Time.deltaTime));
        }

        private bool GetKey(KeyCode rKey)
        {
#if ENABLE_INPUT_SYSTEM
            if (mInputSource != null) { return mInputSource.IsPressed(rKey); }
            return UnityEngine.Input.GetKey(rKey);
#else
            return UnityEngine.Input.GetKey(rKey);
#endif
        }

        private float GetAxis(string rAction)
        {
#if ENABLE_INPUT_SYSTEM
            if (mInputSource != null) { return mInputSource.GetValue(rAction); }
            return UnityEngine.Input.GetAxis(rAction);
#else
            return UnityEngine.Input.GetAxis(rAction);
#endif
        }
    }
}
