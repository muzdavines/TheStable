using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Default implementation of the BaseGun script.
    /// </summary>
    public class Gun : BaseGun
    {
        /// <summary>
        /// Can the gun be loaded with more bullets.
        /// </summary>
        public override bool CanLoad { get { return BulletInventory > 0 && !IsFullyLoaded; } }

        /// <summary>
        /// Number of bullets left in the gun.
        /// </summary>
        public override int LoadedBulletsLeft { get { return LoadedBullets; } }

        /// <summary>
        /// Is the gun fully loaded with bullets.
        /// </summary>
        public override bool IsFullyLoaded { get { return LoadedBullets >= MagazineSize; } }

        /// <summary>
        /// Load percentage for the ammo ui.
        /// </summary>
        public override float LoadPercentage { get { return (float)LoadedBullets / (float)MagazineSize; } }

        /// <summary>
        /// Number of bullets stored at max in the weapon.
        /// </summary>
        [Tooltip("Number of bullets stored at max in the weapon.")]
        public int MagazineSize = 10;

        /// <summary>
        /// Current number of loaded bullets.
        /// </summary>
        [Tooltip("Current number of loaded bullets.")]
        public int LoadedBullets = 10;

        /// <summary>
        /// Number of bullets that are available to be loaded, not counting the already loaded ones.
        /// </summary>
        [Tooltip("Number of bullets that are available to be loaded, not counting the already loaded ones.")]
        public int BulletInventory = 10000;

        /// <summary>
        /// Decrease LoadedBullets.
        /// </summary>
        protected override void Consume()
        {
            LoadedBullets--;
        }

        /// <summary>
        /// Increase LoadedBullets, decrease BulletInventory. Execute related events (may include FullyLoaded).
        /// </summary
        public override bool LoadBullet()
        {
            if (BulletInventory > 0 && LoadedBullets < MagazineSize)
            {
                LoadedBullets++;

                for (int i = 0; i < Listeners.Length; i++)
                    Listeners[i].OnBulletLoad();

                if (BulletLoaded != null) BulletLoaded.Invoke();

                if (LoadedBullets >= MagazineSize)
                {
                    for (int i = 0; i < Listeners.Length; i++)
                        Listeners[i].OnFullyLoaded();

                    if (FullyLoaded != null) FullyLoaded.Invoke();
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Increase the number of loaded bullets, decrease BulletInventory. Execute FullyLoaded event.
        /// </summary>
        public override bool LoadMagazine()
        {
            if (BulletInventory > 0 && LoadedBullets < MagazineSize)
            {
                LoadedBullets = 0;

                var addition = BulletInventory;
                if (addition > MagazineSize)
                    addition = MagazineSize;

                BulletInventory -= addition;
                LoadedBullets += addition;

                for (int i = 0; i < Listeners.Length; i++)
                    Listeners[i].OnFullyLoaded();

                if (FullyLoaded != null) FullyLoaded.Invoke();

                return true;
            }
            else
                return false;
        }
    }
}