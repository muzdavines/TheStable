using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages the UI for adjusting support parameters of the active character.
    /// </summary>
    public class SupportDialog : MonoBehaviour
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Objects that are activated when the current character has the AIProtector component.
        /// </summary>
        [Tooltip("Objects that are activated when the current character has the AIProtector component.")]
        public GameObject[] Protector;

        /// <summary>
        /// Objects that are activated when the current character has the AIHealer component.
        /// </summary>
        [Tooltip("Objects that are activated when the current character has the AIHealer component.")]
        public GameObject[] Healer;

        /// <summary>
        /// Panel that is activated when the current character has any support component.
        /// </summary>
        [Tooltip("Panel that is activated when the current character has any support component.")]
        public GameObject Panel;

        private void manage()
        {
            if (Switcher == null)
                return;

            var motor = Switcher.GetActive();

            if (motor == null)
                return;

            var protector = motor.GetComponent<AIProtector>();
            var support = motor.GetComponent<AIActions>();
            if (support != null && !support.HasAllyActions) support = null;

            if (protector != null)
            {
                if (Panel != null && !Panel.activeSelf)
                    Panel.SetActive(true);

                activate(Protector, true);
                activate(Healer, false);
            }
            else if (support != null)
            {
                if (Panel != null && !Panel.activeSelf)
                    Panel.SetActive(true);

                activate(Protector, false);
                activate(Healer, true);
            }
            else if (Panel != null && Panel.activeSelf)
                Panel.SetActive(false);
        }

        private void OnEnable()
        {
            manage();
        }

        private void Update()
        {
            manage();
        }

        private void activate(GameObject[] objects, bool value)
        {
            for (int i = 0; i < objects.Length; i++)
                if (objects[i] != null && objects[i].activeSelf != value)
                    objects[i].SetActive(value);
        }
    }
}
