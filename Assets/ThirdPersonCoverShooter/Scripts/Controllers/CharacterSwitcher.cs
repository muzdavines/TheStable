using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages a list of actors. Marks one as active.
    /// </summary>
    public class CharacterSwitcher : MonoBehaviour
    {
        /// <summary>
        /// Characters available for switching.
        /// </summary>
        [Tooltip("Characters available for switching.")]
        public Actor[] Characters;

        /// <summary>
        /// HUD that is assigned the current player.
        /// </summary>
        [Tooltip("HUD that is assigned the current player.")]
        public ActorHUD HUD;

        /// <summary>
        /// How long the character number should be held for the strategy input to activate.
        /// </summary>
        [Tooltip("How long the character number should be held for the strategy input to activate.")]
        public float StrategyHold = 0.5f;

        /// <summary>
        /// Index of the currently active character.
        /// </summary>
        [Tooltip("Index of the currently active character.")]
        public int Active;

        private int _active;
        private StrategyInput _strategy;
        private bool _wasPressingStrategy;
        private bool _hadStrategy;

        private AIFormation _previousFormation;

        private float[] _hold = new float[10];
        
        /// <summary>
        /// Returns currently active actor. Can be null.
        /// </summary>
        public Actor GetActive()
        {
            if (Active >= 0 && Active < Characters.Length)
                return Characters[Active];

            return null;
        }

        private void Awake()
        {
            _strategy = GetComponent<StrategyInput>();
            switchTo(Active);
        }

        private void Update()
        {
            var strategyTarget = -1;

            for (int i = 0; i < _hold.Length; i++)
            {
                var key = KeyCode.Alpha1 + i;
                if (i == 9) key = KeyCode.Alpha0;

                if (Input.GetKey(key))
                {
                    _hold[i] += Time.deltaTime;

                    if (_hold[i] >= StrategyHold && _active != i)
                    {
                        strategyTarget = i;

                        if (_strategy != null && Characters != null && Characters.Length > i)
                            _strategy.Target = Characters[i];
                    }
                }

                if (Input.GetKeyDown(key)) _hold[i] = 0;

                if (Input.GetKeyUp(key))
                    if (_hold[i] < StrategyHold)
                        Active = i;
            }

            if (_strategy != null && _wasPressingStrategy && _strategy.Target != null && strategyTarget < 0)
                _strategy.Target = null;

            var hasStrategy = _strategy != null && _strategy.Target != null;

            if (_hadStrategy && !hasStrategy)
                switchTo(Active);
            else if (hasStrategy && !_hadStrategy)
                setCharacter(Active, false, false);

            if (_strategy != null && _strategy.Target == null)
                switchTo(Active);

            _hadStrategy = hasStrategy;
            _wasPressingStrategy = strategyTarget >= 0;
        }

        private void setCharacter(int index, bool controls, bool ai)
        {
            if (Characters == null || Characters.Length <= index)
                return;

            var actor = Characters[index];

            if (actor == null)
                return;

            var thirdPersonInput = actor.GetComponent<ThirdPersonInput>();
            var topDownInput = actor.GetComponent<TopDownInput>();

            var mobileController = actor.GetComponent<MobileController>();
            var thirdPersonController = actor.GetComponent<ThirdPersonController>();

            var aiComponents = actor.GetComponents<AIBase>();

            if (Active >= 0 && Active < Characters.Length)
            {
                var leader = Characters[Active].GetComponent<AIFormation>();

                if (leader != null)
                {
                    var formation = actor.GetComponent<AIFormation>();

                    if (formation != null)
                        formation.Leader = leader;
                }
            }

            foreach (var c in aiComponents)
            {
                if (c is AIFormation)
                    c.enabled = ai || (index == Active);
                else
                    c.enabled = ai;
            }

            if (thirdPersonInput != null) thirdPersonInput.enabled = controls;
            if (topDownInput != null) topDownInput.enabled = controls;

            if (mobileController != null) mobileController.enabled = controls;
            if (thirdPersonController != null) thirdPersonController.enabled = controls;
        }

        private void switchTo(int index)
        {
            if (Characters == null || Characters.Length <= index)
                return;

            for (int i = 0; i < Characters.Length; i++)
            {
                var isAI = i != index;

                if (!isAI && Characters[i] != null)
                {
                    var actions = Characters[i].GetComponent<AIActions>();

                    if (actions != null && actions.IsPerforming)
                        isAI = true;
                }

                setCharacter(i, !isAI, isAI);
            }

            _active = index;

            var active = GetActive();
            if (active != null)
            {
                if (HUD != null)
                    HUD.Player = active;

                var comp = active.GetComponent<AIFormation>();

                if (comp != null && _previousFormation != null)
                    comp.Formation = _previousFormation.Formation;

                _previousFormation = comp;
            }

            var topDownCamera = GetComponent<TopDownCamera>();
            var strategyCamera = GetComponent<StrategyCamera>();
            var thirdPersonCamera = GetComponent<ThirdPersonCamera>();
            var mobileCamera = GetComponent<MobileCamera>();

            var motor = Characters[index].GetComponent<CharacterMotor>();

            if (topDownCamera != null) topDownCamera.Target = motor;
            if (strategyCamera != null) strategyCamera.Target = motor;
            if (thirdPersonCamera != null) thirdPersonCamera.Target = motor;
            if (mobileCamera != null) mobileCamera.Target = motor;
        }
    }
}
