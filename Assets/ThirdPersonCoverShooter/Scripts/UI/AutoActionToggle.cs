using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Toggles whether an AI action is performed automatically or manually. Manages other UI objects based on the value.
    /// </summary>
    public class AutoActionToggle : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Action index for the main character.
        /// </summary>
        [Tooltip("Action index for the main character.")]
        public int Action;

        /// <summary>
        /// Objects that are activated when the action is automatic.
        /// </summary>
        [Tooltip("Objects that are activated when the action is automatic.")]
        public GameObject[] Automatic;

        /// <summary>
        /// Objects that are activated when the action is not automatic.
        /// </summary>
        [Tooltip("Objects that are activated when the action is not automatic.")]
        public GameObject[] Manual;

        private void Update()
        {
            var action = get();
            if (action == null)
                return;

            for (int i = 0; i < Automatic.Length; i++)
                if (Automatic[i] != null && Automatic[i].activeSelf != action.Auto)
                    Automatic[i].SetActive(action.Auto);

            for (int i = 0; i < Manual.Length; i++)
                if (Manual[i] != null && Manual[i].activeSelf == action.Auto)
                    Manual[i].SetActive(!action.Auto);
        }

        private AIAction get()
        {
            if (Switcher == null || Action < 0)
                return null;

            var active = Switcher.GetActive();
            if (active == null)
                return null;

            var actions = active.GetComponent<AIActions>();
            if (actions == null || Action >= actions.Actions.Length)
                return null;

            return actions.Actions[Action];
        }

        protected override void OnPress()
        {
            var action = get();

            if (action != null)
                action.Auto = !action.Auto;
        }
    }
}
