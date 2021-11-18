using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Sets formation type of the active character when enabled. Disables itself and enables other UI object when pressed.
    /// </summary>
    public class FormationSelector : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Formation to be formed when this button is active.
        /// </summary>
        [Tooltip("Formation to be formed when this button is active.")]
        public FormationType Formation;

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
                        comp.Formation = Formation;
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
