using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets aggressivness of the active character when enabled. Disables itself and enables other UI object when pressed.
    /// </summary>
    public class AggressiveSelector : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Aggressivness value set in the AIFormation component.
        /// </summary>
        [Tooltip("Aggressivness value set in the AIFormation component.")]
        public bool Aggressive;

        /// <summary>
        /// UI object that is activated when pressing this one.
        /// </summary>
        [Tooltip("Selector object that is activated if the character is selected.")]
        public GameObject Next;

        private void OnEnable()
        {
            if (Switcher != null)
            {
                var active = Switcher.GetActive();

                if (active != null)
                {
                    var comp = active.GetComponent<AIFormation>();

                    if (comp != null)
                        comp.Aggressive = Aggressive;
                }
            }
        }

        protected override void OnPress()
        {
            if (Next != null)
            {
                Next.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
