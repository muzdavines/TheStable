using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Changes active character when pressed.
    /// </summary>
    public class CharacterSelector : PressButton
    {
        /// <summary>
        /// Character switcher whose active character is managed.
        /// </summary>
        [Tooltip("Character switcher whose active character is managed.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Character index inside the switcher.
        /// </summary>
        [Tooltip("Character index inside the switcher.")]
        public int Index;

        /// <summary>
        /// Objects that are activated when the character is active.
        /// </summary>
        [Tooltip("Objects that are activated when the character is active.")]
        public GameObject[] Active;

        /// <summary>
        /// Objects that are activated when the character is not active.
        /// </summary>
        [Tooltip("Objects that are activated when the character is not active.")]
        public GameObject[] Inactive;

        /// <summary>
        /// A dialog object to activate if the character was already selected when pressed.
        /// </summary>
        [Tooltip("A dialog object to activate if the character was already selected when pressed.")]
        public GameObject Dialog;

        private void Update()
        {
            if (Switcher == null)
                return;

            var isActive = Switcher.Active == Index;

            for (int i = 0; i < Active.Length; i++)
                if (Active[i] != null && Active[i].activeSelf != isActive)
                    Active[i].SetActive(isActive);

            for (int i = 0; i < Inactive.Length; i++)
                if (Inactive[i] != null && Inactive[i].activeSelf == isActive)
                    Inactive[i].SetActive(!isActive);
        }

        protected override void OnPress()
        {
            if (Switcher != null)
            {
                if (Index == Switcher.Active && Dialog != null)
                    Dialog.SetActive(true);

                Switcher.Active = Index;
            }
        }
    }
}
