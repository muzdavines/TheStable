using System;
using System.Collections;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    public class FlyInEffect : Effect<FlyInEffectSettings>, IEffectOneShot
    {
        /// <summary>
        /// The starting position of this module.
        /// </summary>
        private Vector2 _startPosition;

        /// <summary>
        /// Occurs when effect starts.
        /// </summary>
        public event EffectEvent EffectStart;

        /// <summary>
        /// Occurs when effect ends.
        /// </summary>
        public event EffectEvent EffectEnd;

        /// <summary>
        /// Returns true if the effect has finished.
        /// </summary>
        public bool HasFinished { get; set; }

        /// <summary>
        /// Called when effect starts.
        /// </summary>
        public void OnEffectStart()
        {
            HasFinished = false;
            var ev = EffectStart;
            if (ev == null)
                return;

            ev.Invoke();
        }

        /// <summary>
        /// Called when effect ends.
        /// </summary>
        public void OnEffectEnd()
        {
            HasFinished = true;
            var ev = EffectEnd;
            if (ev == null)
                return;

            ev.Invoke();
        }

        /// <inheritdoc />
        protected override IEnumerator OnEffectBegin()
        {
            SetStartPosition(EffectSettings.FlyDirection, EffectSettings.FlyDistance);
            
            yield return new WaitForFixedUpdate();

            float lerpAmount = 0;

            while (lerpAmount < 1)
            {
                lerpAmount += EffectSettings.Speed * Time.fixedDeltaTime;
                RectTransform.anchoredPosition = Vector2.Lerp(_startPosition, DestinationPosition, lerpAmount);
                yield return new WaitForFixedUpdate();
            }

            if (!EffectSettings.CanInteract)
                ModuleCanvasGroup.interactable = true;

            OnEffectEnd();
            yield return null;
        }

        /// <summary>
        /// Applies the effect settings for this effect.
        /// </summary>
        /// <param name="settings">The new effect settings.</param>
        protected override void OnEffectSettingsSet(FlyInEffectSettings settings)
        {
            OnEffectStart();
        }

        /// <summary>
        /// Called only once.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            DestinationPosition = RectTransform.anchoredPosition;
        }

        /// <summary>
        /// Sets a starting distance of a Module to prepare for a FlyIn Effect.
        /// </summary>
        /// <param name="direction">The fly direction.</param>
        /// <param name="distance">The distance from the designated position.</param>
        /// <exception cref="ArgumentOutOfRangeException">direction - null</exception>
        private void SetStartPosition(FlyDirection direction, float distance)
        {
            switch (direction)
            {
                case FlyDirection.Left:

                    _startPosition = new Vector2(
                            DestinationPosition.x - distance,
                            DestinationPosition.y
                        );

                    break;

                case FlyDirection.Right:

                    _startPosition = new Vector2(
                        DestinationPosition.x + distance,
                        DestinationPosition.y
                    );

                    break;

                case FlyDirection.Top:

                    _startPosition = new Vector2(
                        DestinationPosition.x,
                        DestinationPosition.y + distance
                    );

                    break;

                case FlyDirection.Bottom:

                    _startPosition = new Vector2(
                        DestinationPosition.x,
                        DestinationPosition.y - distance
                    );

                    break;

                case FlyDirection.TopLeft:

                    _startPosition = new Vector2(
                        DestinationPosition.x - distance,
                        DestinationPosition.y + distance
                    );

                    break;

                case FlyDirection.TopRight:

                    _startPosition = new Vector2(
                        DestinationPosition.x + distance,
                        DestinationPosition.y + distance
                    );

                    break;

                case FlyDirection.BottomLeft:

                    _startPosition = new Vector2(
                        DestinationPosition.x - distance,
                        DestinationPosition.y - distance
                    );

                    break;

                case FlyDirection.BottomRight:

                    _startPosition = new Vector2(
                        DestinationPosition.x + distance,
                        DestinationPosition.y - distance
                    );

                    break;

                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }
    }
}