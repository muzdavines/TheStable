using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Contains a character name used by the UI. If no Character Name is present the name of the game object is taken as the name.
    /// </summary>
    public class CharacterName : MonoBehaviour
    {
        /// <summary>
        /// Name of the character to be display in the UI.
        /// </summary>
        [Tooltip("Name of the character to be display in the UI.")]
        public string Name;
    }
}
