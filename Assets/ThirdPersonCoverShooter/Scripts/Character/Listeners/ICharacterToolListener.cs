using UnityEngine;

namespace CoverShooter
{
    public interface ICharacterToolListener
    {
        /// <summary>
        /// Character has used a tool (it's no longer in use).
        /// </summary>
        void OnToolUsed(bool isAlternateMode);
    }
}