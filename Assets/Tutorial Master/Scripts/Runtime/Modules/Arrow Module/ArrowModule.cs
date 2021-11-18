using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [AddComponentMenu("Tutorial Master/UGUI Arrow Module")]
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/arrow-module")]
    public class ArrowModule : Module
    {
        /// <inheritdoc />
        /// <summary>
        /// Deactivates the components if any.
        /// </summary>
        protected override void OnModuleDeactivated() { }

        /// <inheritdoc />
        /// <summary>
        /// Initialize additional components if any.
        /// </summary>
        protected override void OnModuleActivated()
        {
            var settings = GetSettings<ArrowModuleSettings>();
            if (settings == null)
                return;

            RotateArrow(settings.PointDirection, settings.TargetType, settings.PointTarget);
        }

        /// <inheritdoc />
        /// <summary>
        /// Called every OnUpdate call if enabled
        /// </summary>
        protected override void OnUpdate()
        {
            base.OnUpdate();
            var settings = GetSettings<ArrowModuleSettings>();
            RotateArrow(settings.PointDirection, settings.TargetType, settings.PointTarget);
        }

        /// <summary>
        /// Sets the rotation of this object based on transform targetPos
        /// </summary>
        /// <param name="targetPos">The position of a targetPos.</param>
        /// <param name="targetType">Specifies whether the target the arrow should look at is Canvas Element or a Game Element</param>
        private void LookAtTarget(Vector3 targetPos, TargetType targetType)
        {
            Vector3 difference = targetType == TargetType.CanvasSpace
                ? targetPos - RectTransform.position
                : (Vector3)CalculatePositionFromWorld(targetPos) - RectTransform.position;

            float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            RectTransform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
        }

        /// <summary>
        /// Rotates the Arrow in a specified direction
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="lookTarget">The transform of a UI element at which this Arrow will look at.</param>
        private void RotateArrow(PointDirection direction, TargetType targetType, Transform lookTarget)
        {
            switch (direction)
            {
                case PointDirection.Left:

                    RectTransform.eulerAngles = new Vector3(0, 180, 0);

                    break;

                case PointDirection.Right:

                    RectTransform.eulerAngles = new Vector3(0, 0, 0);

                    break;

                case PointDirection.Up:

                    RectTransform.eulerAngles = new Vector3(0, 0, 90);

                    break;

                case PointDirection.Down:

                    RectTransform.eulerAngles = new Vector3(0, 0, -90);

                    break;

                case PointDirection.TopLeft:

                    RectTransform.eulerAngles = new Vector3(0, 0, 135);

                    break;

                case PointDirection.TopRight:

                    RectTransform.eulerAngles = new Vector3(0, 0, 45);

                    break;

                case PointDirection.BottomLeft:

                    RectTransform.eulerAngles = new Vector3(0, 0, -135);

                    break;

                case PointDirection.BottomRight:

                    RectTransform.eulerAngles = new Vector3(0, 0, -45);

                    break;

                case PointDirection.LookAtTransform:

                    LookAtTarget(lookTarget.position, targetType);

                    break;

                default:

                    throw TMLogger.LogException(new ArgumentOutOfRangeException("direction", direction, null));
            }
        }
    }
}