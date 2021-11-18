using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterWalkListener
    {
        /// <summary>
        /// Character stops.
        /// </summary>
        void OnStop();
        
        /// <summary>
        /// Character is moving at walking speed.
        /// </summary>
        void OnWalk();

        /// <summary>
        /// Character is moving at running speed.
        /// </summary>
        void OnRun();

        /// <summary>
        /// Character is moving at sprinting speed.
        /// </summary>
        void OnSprint();
    }
}