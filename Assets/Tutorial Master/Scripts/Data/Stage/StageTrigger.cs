using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HardCodeLab.TutorialMaster
{
    [Serializable]
    [DataValidator(typeof(StageTriggerValidator))]
    public class StageTrigger
    {
        /// <summary>
        /// The amount of time to wait before activating a trigger.
        /// </summary>
        public float TriggerActivationDelay;

        /// <summary>
        /// Stores methods that will be invoked when a trigger delay has ended.
        /// </summary>
        public UnityEvent OnDelayEnd;

        /// <summary>
        /// Stores methods that will be invoked when a trigger has been activated.
        /// </summary>
        public UnityEvent OnTriggerActivate;

        /// <summary>
        /// An additional event which will be activated after OnTriggerActivate is invoked.
        /// </summary>
        public ActionType TriggerActivationEvent = ActionType.PlayNextStage;

        /// <summary>
        /// Name of an Input which will take user to the next stage.
        /// </summary>
        public string TriggerInput;

        /// <summary>
        /// The KeyCode which will trigger next stage
        /// </summary>
        public KeyCode TriggerKey;

        /// <summary>
        /// UnityEvent to which this trigger will be subscribed to.
        /// </summary>
        public UnityEventTarget EventTarget;

        /// <summary>
        /// Amount of time (in milliseconds) till trigger is activated.
        /// </summary>
        public float TriggerTimerAmount;

        /// <summary>
        /// Specifies the kind of trigger to use
        /// </summary>
        public TriggerType Type;

        /// <summary>
        /// The button component which will be used for taking user to the next stage.
        /// </summary>
        public Button UIButtonTarget;

        /// <summary>
        /// Checks if triggers are ready to be checked.
        /// </summary>
        private bool _isReady;

        /// <summary>
        /// The on click action
        /// </summary>
        private UnityAction _triggerAction;

        /// <summary>
        /// Checks if the trigger has already been activated.
        /// </summary>
        private bool _triggerActivated;

        /// <summary>
        /// Displays the current amount of time elapsed for this input delay timer.
        /// </summary>
        public float CurrentInputDelayTimer { get; private set; }

        /// <summary>
        /// Displays the current amount of time elapsed for this trigger timer.
        /// </summary>
        public float CurrentTriggerTimer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageTrigger"/> class.
        /// </summary>
        public StageTrigger()
        {
            Type = TriggerType.None;

            TriggerTimerAmount = 3.5f;
            TriggerActivationDelay = 1.5f;
        }

        /// <summary>
        /// Checks if any trigger has been activated.
        /// </summary>
        public void Check(TutorialMasterManager manager)
        {
            if (_triggerActivated)
                return;

            if (!EvaluateDelayTimer())
                return;

            if (_triggerAction == null)
            {
                _triggerAction = () => ActivateTrigger(manager);
            }

            switch (Type)
            {
                case TriggerType.AnyKeyPress:

                    // check for any key press
                    if (Input.anyKeyDown)
                    {
                        ActivateTrigger(manager);
                    }

                    break;

                case TriggerType.KeyPress:

                    // check for key press
                    if (Input.GetKeyDown(TriggerKey))
                    {
                        ActivateTrigger(manager);
                    }

                    break;

                case TriggerType.Input:

                    // check for input press
                    if (Input.GetButtonDown(TriggerInput))
                    {
                        ActivateTrigger(manager);
                    }

                    break;

                case TriggerType.Timer:

                    // check for timer expiration
                    CurrentTriggerTimer += Time.deltaTime;

                    if (CurrentTriggerTimer >= TriggerTimerAmount)
                    {
                        ActivateTrigger(manager);
                    }

                    break;

                case TriggerType.UGUIButtonClick:

                    AddClickListener();

                    break;

                case TriggerType.UnityEventInvoke:

                    EventTarget.AddListener(_triggerAction);

                    break;
            }
        }

        /// <summary>
        /// Initializes this trigger and resets any timers.
        /// </summary>
        public void Init()
        {
            CurrentInputDelayTimer = 0;
            CurrentTriggerTimer = 0;

            _isReady = false;
            _triggerActivated = false;
        }

        /// <summary>
        /// Disables this trigger.
        /// </summary>
        public void Disable()
        {
            RemoveClickListener();
        }

        /// <summary>
        /// Activates the trigger.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ActivateTrigger(TutorialMasterManager manager)
        {
            if (_triggerActivated)
                return;

            OnTriggerActivate.Invoke();

            switch (TriggerActivationEvent)
            {
                case ActionType.PlayNextStage:

                    manager.NextStage();

                    break;

                case ActionType.StopTutorial:

                    manager.StopTutorial();

                    break;
            }

            _triggerActivated = true;
        }

        /// <summary>
        /// Adds the click listener to a UI element
        /// </summary>
        private void AddClickListener()
        {
            if (Type == TriggerType.UGUIButtonClick)
            {
                UIButtonTarget.onClick.AddListener(_triggerAction);
            }
        }

        /// <summary>
        /// Determines whether or not the delay timer has been activated.
        /// </summary>
        /// <returns>Returns true if the </returns>
        private bool EvaluateDelayTimer()
        {
            if (_isReady)
                return true;

            if (Type == TriggerType.Timer)
            {
                _isReady = true;
                return true;
            }

            CurrentInputDelayTimer += Time.deltaTime;

            if (CurrentInputDelayTimer >= TriggerActivationDelay)
            {
                OnDelayEnd.Invoke();
                _isReady = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the OnClick event from the UI element.
        /// </summary>
        private void RemoveClickListener()
        {
            if (Type == TriggerType.UGUIButtonClick)
            {
                UIButtonTarget.onClick.RemoveListener(_triggerAction);

                _triggerAction = null;
            }

            if (Type == TriggerType.UnityEventInvoke)
            {
                EventTarget.RemoveAllAddedListeners();
            }
        }
    }
}