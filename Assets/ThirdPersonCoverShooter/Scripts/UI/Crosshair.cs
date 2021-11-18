using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(ThirdPersonCamera))]
    public class Crosshair : MonoBehaviour
    {
        /// <summary>
        /// Settings to use when aiming a weapon of a type not covered by other properties.
        /// </summary>
        [Tooltip("Settings to use when aiming a weapon of a type not covered by other properties.")]
        public CrosshairSettings Default = CrosshairSettings.Default();

        /// <summary>
        /// Settings to use when aiming a pistol.
        /// </summary>
        [Tooltip("Settings to use when aiming a pistol.")]
        public CrosshairSettings Pistol = CrosshairSettings.Default();

        /// <summary>
        /// Settings to use when aiming a rifle.
        /// </summary>
        [Tooltip("Settings to use when aiming a rifle.")]
        public CrosshairSettings Rifle = CrosshairSettings.Default();

        /// <summary>
        /// Settings to use when aiming a shotgun.
        /// </summary>
        [Tooltip("Settings to use when aiming a shotgun.")]
        public CrosshairSettings Shotgun = CrosshairSettings.Default();

        /// <summary>
        /// Settings to use when aiming a sniper.
        /// </summary>
        [Tooltip("Settings to use when aiming a sniper.")]
        public CrosshairSettings Sniper = CrosshairSettings.Default();

        private ThirdPersonCamera _thirdPersonCamera;
        private Camera _camera;

        private float _recoil;
        private float _fov;
        private CrosshairSettings _settings = CrosshairSettings.Default();
        private float _previousAlpha;

        private void Awake()
        {
            _thirdPersonCamera = GetComponent<ThirdPersonCamera>();
            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Draws the crosshair.
        /// </summary>
        private void OnGUI()
        {
            var settings = _settings;

            if (_thirdPersonCamera.Target != null)
            {
                var weapon = _thirdPersonCamera.Target.ActiveWeapon;

                if (!_thirdPersonCamera.Target.IsChangingWeapon || (_thirdPersonCamera.CrosshairAlpha > _previousAlpha))
                {
                    if (weapon.Gun != null && weapon.Gun.UseCustomCrosshair)
                        settings = weapon.Gun.CustomCrosshair;
                    else if (weapon.Gun != null && weapon.Gun.Type == WeaponType.Pistol)
                        settings = Pistol;
                    else if (weapon.Gun != null && weapon.Gun.Type == WeaponType.Rifle)
                        settings = Rifle;
                    else
                        settings = Default;
                }
            }
            else
                settings = Default;

            _previousAlpha = _thirdPersonCamera.CrosshairAlpha;

            if (settings.Sprites == null || settings.Sprites.Length == 0)
                return;

            var targetFOV = _thirdPersonCamera.ShakeOffset * settings.ShakeMultiplier;

            {
                var vabs = Mathf.Abs(_thirdPersonCamera.VerticalRecoil);
                var habs = Mathf.Abs(_thirdPersonCamera.HorizontalRecoil);

                var targetRecoil = vabs > habs ? vabs : habs;
                var newRecoil = Util.Lerp(_recoil, targetRecoil, 0.5f);
                targetFOV += Mathf.Max(0, targetRecoil - _recoil) * settings.RecoilMultiplier;
                _recoil = newRecoil;
            }

            if (_thirdPersonCamera.Target != null)
            {
                var gun = _thirdPersonCamera.Target.ActiveWeapon.Gun;

                if (gun != null)
                {
                    if (_thirdPersonCamera.Target.IsZooming)
                        targetFOV += gun.Error * _thirdPersonCamera.Target.ZoomErrorMultiplier;
                    else
                        targetFOV += gun.Error;
                }
            }

            if (_thirdPersonCamera.Target != null)
                targetFOV += _thirdPersonCamera.Target.MovementError;

            Util.Lerp(ref _fov, targetFOV, settings.Adaptation);

            if (_thirdPersonCamera.Target != null && _thirdPersonCamera.Target.IsScoping)
                return;
            
            var lerp = settings.LowAngle < settings.HighAngle ? Mathf.Clamp01((_fov - settings.LowAngle) / (settings.HighAngle - settings.LowAngle)) : 0;
            var sprite = settings.Sprites[(int)(lerp * (settings.Sprites.Length - 1))];

            if (sprite == null)
                return;

            var aimFOV = Mathf.Lerp(settings.LowAngle, settings.HighAngle, lerp);

            var size = settings.Scale * Screen.height * aimFOV / _camera.fieldOfView;
            var point = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

            var texture = sprite.texture;
            var textureRect = sprite.textureRect;
            var textureRectOffset = sprite.textureRectOffset;
            var pivot = sprite.pivot;

            var source = new Rect(textureRect.x / texture.width, textureRect.y / texture.height, textureRect.width / texture.width, textureRect.height / texture.height);
            var aspectRatio = textureRect.width / textureRect.height;

            Vector2 offset;
            offset.x = (pivot.x - textureRectOffset.x) / textureRect.width;
            offset.y = (textureRect.height - pivot.y + textureRectOffset.y) / textureRect.height;

            var dest = new Rect(point.x - size * offset.x * aspectRatio, point.y - size * offset.y, size * aspectRatio, size);

            var previous = GUI.color;
            GUI.color = new Color(1, 1, 1, _thirdPersonCamera.CrosshairAlpha);
            GUI.DrawTextureWithTexCoords(dest, texture, source, true);
            GUI.color = previous;
        }
    }
}
