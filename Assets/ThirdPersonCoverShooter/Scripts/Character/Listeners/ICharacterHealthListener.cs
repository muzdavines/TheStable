using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterHealthListener
    {
        /// <summary>
        /// Character is dead.
        /// </summary>
        void OnDead();
        
        /// <summary>
        /// Character is no longer dead.
        /// </summary>
        void OnResurrect();
    }
}