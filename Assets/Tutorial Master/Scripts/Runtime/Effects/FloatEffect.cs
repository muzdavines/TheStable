using System.Collections;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/floating-effect")]
    public class FloatEffect : Effect<FloatEffectSettings>
    {
        /// <summary>
        /// The default position of this Module before this effect occured.
        /// </summary>
        private Vector2 _defaultPosition;

        protected override void Awake()
        {
            base.Awake();
            DestinationPosition = RectTransform.anchoredPosition;
        }

        /// <inheritdoc />
        protected override IEnumerator OnEffectBegin()
        {
            while (true)
            {
                float diff = CalculateDiff();
                Vector2 newPos = new Vector2();

                switch (EffectSettings.Direction)
                {
                    case Orientation.Horizontal:

                        newPos = new Vector2(
                            DestinationPosition.x - diff,
                            DestinationPosition.y
                        );

                        break;

                    case Orientation.Vertical:

                        newPos = new Vector2(
                            DestinationPosition.x,
                            DestinationPosition.y - diff
                        );

                        break;

                    case Orientation.DiagonalLeft:

                        newPos = new Vector2(
                            DestinationPosition.x - diff,
                            DestinationPosition.y + diff
                        );

                        break;

                    case Orientation.DiagonalRight:

                        newPos = new Vector2(
                            DestinationPosition.x - diff,
                            DestinationPosition.y - diff
                        );

                        break;
                }

                RectTransform.anchoredPosition = newPos;

                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Calculates the current position of the module.
        /// </summary>
        /// <returns></returns>
        private float CalculateDiff()
        {
            if (EffectSettings == null)
                return 0;

            float diff = 0;

            switch (EffectSettings.FloatPattern)
            {
                case WaveType.Sine:

                    diff = (Mathf.Sin(Time.time * EffectSettings.Speed) * EffectSettings.FloatRange - EffectSettings.FloatRange) / 2;

                    break;

                case WaveType.Square:

                    diff = (Mathf.Sign(Mathf.Sin(Time.time * EffectSettings.Speed)) * EffectSettings.FloatRange - EffectSettings.FloatRange) / 2;

                    break;

                case WaveType.Triangle:

                    diff = (Mathf.Abs((Time.time * EffectSettings.Speed % 6) - 3) * EffectSettings.FloatRange - EffectSettings.FloatRange) / 2;

                    break;

                case WaveType.Custom:

                    diff = (EffectSettings.CustomPattern.Evaluate(Time.time * EffectSettings.Speed) * EffectSettings.FloatRange - EffectSettings.FloatRange) / 2;

                    break;
            }

            return diff;
        }
    }
}