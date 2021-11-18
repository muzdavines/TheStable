using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Starts an AI action when pressed. Displays cooldown as a bar being filled.
    /// </summary>
    public class ActionButton : PressButton
    {
        /// <summary>
        /// Character switcher to get the character object from.
        /// </summary>
        [Tooltip("Character switcher to get the character object from.")]
        public CharacterSwitcher Switcher;

        /// <summary>
        /// Strategy input object that is given a command.
        /// </summary>
        [Tooltip("Strategy input object that is given a command.")]
        public StrategyInput Strategy;

        /// <summary>
        /// Character index inside the switcher.
        /// </summary>
        [Tooltip("Character index inside the switcher.")]
        public int CharacterIndex;

        /// <summary>
        /// Action to be performed.
        /// </summary>
        [Tooltip("Action to be performed.")]
        public int ActionIndex;

        /// <summary>
        /// UI object whose height is managed during the cooldown. If none a button target graphic is used.
        /// </summary>
        [Tooltip("UI object whose height is managed during the cooldown. If none a button target graphic is used.")]
        public RectTransform FillOverride;

        protected override void OnPress()
        {
            if (Strategy == null || Switcher == null || CharacterIndex < 0 || CharacterIndex >= Switcher.Characters.Length || Switcher.Characters[CharacterIndex] == null)
                return;

            var target = Switcher.Characters[CharacterIndex];

            if (target != null)
            {
                var actions = target.GetComponent<AIActions>();

                if (actions != null && ActionIndex >= 0 && ActionIndex < actions.Actions.Length)
                {
                    var action = actions.Actions[ActionIndex];

                    if (action != null && action.Wait <= float.Epsilon)
                    {
                        if (action.NeedsSingleTargetActor || action.NeedsTargetLocation)
                        {
                            Strategy.Target = target;
                            Strategy.GiveCommand(action);
                        }
                        else
                            actions.Execute(action);
                    }
                }
            }
        }

        private void Update()
        {
            var fill = FillOverride != null ? FillOverride : (Button.Target != null ? Button.Target.rectTransform : null);

            if (fill == null)
                return;

            if (Switcher == null || CharacterIndex < 0 || CharacterIndex >= Switcher.Characters.Length || Switcher.Characters[CharacterIndex] == null)
                return;

            var actions = Switcher.Characters[CharacterIndex].GetComponent<AIActions>();

            if (ActionIndex < 0 || ActionIndex >= actions.Actions.Length)
                return;
            var action = actions.Actions[ActionIndex];

            float value;

            if (action.Cooldown > float.Epsilon)
                value = 1 - action.Wait / action.Cooldown;
            else
                value = 1;

            fill.anchorMax = new Vector2(1, value);
        }
    }
}
