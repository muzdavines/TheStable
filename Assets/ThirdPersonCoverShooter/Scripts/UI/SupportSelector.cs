using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a single possible character value to be supported for the currently active character.
    /// </summary>
    public class SupportSelector : PressButton
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
        /// Objects that are activated when the selector is active.
        /// </summary>
        [Tooltip("Objects that are activated when the selector is active.")]
        public GameObject[] Active;

        /// <summary>
        /// Objects that are activated when the selector is active.
        /// </summary>
        [Tooltip("Objects that are activated when the selector is active.")]
        public GameObject[] Inactive;

        /// <summary>
        /// Default portrait to be displayed.
        /// </summary>
        [Tooltip("Default portrait to be displayed.")]
        public GameObject Default;

        /// <summary>
        /// Portrair that's displayed when the index is the active character.
        /// </summary>
        [Tooltip("Portrair that's displayed when the index is the active character.")]
        public GameObject Replacement;

        private void manage()
        {
            if (Switcher == null)
                return;

            var isDefault = index == Index;

            if (Default != null && Default.activeSelf != isDefault)
                Default.SetActive(isDefault);

            if (Replacement != null && Replacement.activeSelf == isDefault)
                Replacement.SetActive(!isDefault);

            var isActive = getTarget() == Switcher.Characters[index];

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
            if (Switcher == null)
                return;

            setTarget(Switcher.Characters[index]);
        }

        private int index
        {
            get
            {
                if (Switcher != null && Switcher.Active >= Index)
                    return Index - 1;

                return Index;
            }
        }

        private Actor getTarget()
        {
            var protector = getActorComponent<AIProtector>();
            if (protector != null) return protector.Target;

            var support = getSupport();
            if (support != null) return support.Priority;

            return null;
        }

        private AIActions getSupport()
        {
            var support = getActorComponent<AIActions>();

            if (support != null && !support.HasAllyActions)
                return null;

            return support;
        }

        void setTarget(Actor value)
        {
            var protector = getActorComponent<AIProtector>();
            if (protector != null)
            {
                protector.Target = value;
                return;
            }

            var support = getSupport();
            if (support != null)
            {
                support.Priority = value;
                return;
            }
        }

        private T getActorComponent<T>() where T : AIBase
        {
            if (Switcher == null)
                return null;

            var actor = Switcher.GetActive();

            if (actor == null)
                return null;

            return actor.GetComponent<T>();
        }
    }
}
