using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace VikingCrew.Tools.UI
{
    public delegate void SpeechBubbleFinishedDelegate(SpeechBubbleBase sender, Transform followedObject);

    public abstract class SpeechBubbleBase : MonoBehaviour
    {
        private float _timeToLive = 1f;
        private bool _isSizeDirty;


        [SerializeField] private Image _image;

        /// <summary>
        /// Use this to see if a speech bubble can be updated (i.e, is still the same speech bubble, following the same character)
        /// using UpdateText
        /// </summary>
        public int Iteration { get; private set; }

        public float Height
        {
            get
            {
                if (_isSizeDirty)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_image.rectTransform);
                    _isSizeDirty = false;
                }

                var localHeight = _image.rectTransform.sizeDelta.y;
                var worldHeight = localHeight * _image.rectTransform.lossyScale.y;

                return worldHeight;
            }
        }

        private Vector3 _originalOffset;

        public Camera Cam { get; set; }
        /// <summary>
        /// The offset caused by multiple speech bubbles following the same actor
        /// </summary>
        public Vector3 QueueOffset { set; private get; }
        private Vector3 TotalOffset
        {
            get
            {
                return _originalOffset + QueueOffset;
            }
        }
        public Transform FollowedObject { get; private set; }

        public event SpeechBubbleFinishedDelegate SpeechBubbleFinishedEvent;

        protected virtual void Update()
        {
            _timeToLive -= Time.deltaTime;
            HandleFadeout();

            if (_timeToLive <= 0)
            {
                Clear();
            }
        }

        private void HandleFadeout()
        {
            // When text is about to die start fading out
            if (0 < _timeToLive && _timeToLive < 1)
            {
                var alpha = _timeToLive;
                SetBackgroundAlpha(alpha);
                SetTextAlpha(alpha);
            }
        }

        private void SetBackgroundAlpha(float alpha)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
        }

        protected virtual void LateUpdate()
        {
            UpdatePosition();

            transform.rotation = Cam.transform.rotation;
            _isSizeDirty = false;
        }

        protected abstract void SetTextAlpha(float alpha);

        private void UpdatePosition()
        {
            if (FollowedObject != null)
                transform.position = FollowedObject.position + Cam.transform.TransformVector(TotalOffset);
        }

        /// <summary>
        /// Instantly removes this speech bubble, sending it to be recycled
        /// </summary>
        public void Clear()
        {
            gameObject.SetActive(false);
            Iteration++;
            SpeechBubbleFinishedEvent?.Invoke(this, FollowedObject);
        }

        /// <summary>
        /// You can use this method to update the text inside an existing speech bubble.
        /// 
        /// Note that the speech bubble will be recycled at the end of its timeToLive so you will need to check that it is still on 
        /// the same Iteration as when you first created it. If it is on a later iteration then create a new one instead
        /// </summary>
        /// <param name="text"></param>
        /// <param name="newTimeToLive"></param>
        public void UpdateText(string text, float newTimeToLive)
        {
            SetText(text);
            _timeToLive = newTimeToLive;
        }

        /// <summary>
        /// Called by Speech bubble manager.
        /// Hands off!
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="timeToLive"></param>
        /// <param name="color"></param>
        public void Setup(Vector3 position, string text, float timeToLive, Color color, Camera cam)
        {
            Setup(text, timeToLive, color, cam);

            transform.position = position;
            transform.rotation = Cam.transform.rotation;

            FollowedObject = null;
            _originalOffset = Vector3.zero;

            if (timeToLive > 0)
                gameObject.SetActive(true);
        }

        /// <summary>
        /// Called by Speech bubble manager.
        /// Hands off!
        /// </summary>
        /// <param name="objectToFollow"></param>
        /// <param name="offset"></param>
        /// <param name="text"></param>
        /// <param name="timeToLive"></param>
        /// <param name="color"></param>
        public void Setup(Transform objectToFollow, Vector3 offset, string text, float timeToLive, Color color, Camera cam)
        {
            Setup(text, timeToLive, color, cam);

            FollowedObject = objectToFollow;

            UpdatePosition();
            transform.rotation = Cam.transform.rotation;

            _originalOffset = offset;

            if (timeToLive > 0)
                gameObject.SetActive(true);
        }

        private void Setup(string text, float timeToLive, Color color, Camera cam)
        {
            if (cam)
                Cam = cam;
            else
                Cam = Camera.main;

            _timeToLive = timeToLive;
            SetText(text);
            _image.color = color;
            SetTextAlpha(1);
            _isSizeDirty = true;
        }

        protected abstract void SetText(string text);
    }
}