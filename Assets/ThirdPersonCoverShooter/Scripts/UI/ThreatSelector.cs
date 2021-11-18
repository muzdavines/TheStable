using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Changes threat priority when pressed for the currently active character.
    /// </summary>
    public class ThreatSelector : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Value to set when the button is pressed.
        /// </summary>
        [Tooltip("Value to set when the button is pressed.")]
        public ThreatPriority Value;

        /// <summary>
        /// Objects that are activated when the setting is active.
        /// </summary>
        [Tooltip("Objects that are activated when the setting is active.")]
        public GameObject[] Active;

        /// <summary>
        /// Objects that are activated when the setting is active.
        /// </summary>
        [Tooltip("Objects that are activated when the setting is active.")]
        public GameObject[] Inactive;

        private AIThreatControl get()
        {
            if (Switcher == null)
                return null;

            var actor = Switcher.GetActive();

            if (actor == null)
                return null;

            return actor.GetComponent<AIThreatControl>();
        }

        private void manage()
        {
            var ai = get();
            if (ai == null) return;

            var isActive = ai.Priority == Value;

            for (int i = 0; i < Active.Length; i++)
                if (Active[i] != null && Active[i].activeSelf != isActive)
                    Active[i].SetActive(isActive);

            for (int i = 0; i < Inactive.Length; i++)
                if (Inactive[i] != null && Inactive[i].activeSelf == isActive)
                    Inactive[i].SetActive(!isActive);
        }

        private void OnEnable()
        {
            manage();
        }

        private void Update()
        {
            manage();
        }

        protected override void OnPress()
        {
            var ai = get();

            if (ai != null)
                ai.Priority = Value;
        }
    }
}
