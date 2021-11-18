using System;
using System.Collections;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <inheritdoc />
    /// <summary>
    /// Module Effect Component. Used to execute an effect for a module.
    /// </summary>
    /// <seealso cref="T:UnityEngine.MonoBehaviour" />
    [Serializable]
    [RequireComponent(typeof(Module))]
    public abstract class Effect<TEffectSettings> : MonoBehaviour
        where TEffectSettings : EffectSettings
    {
        /// <summary>
        /// Current settings for this Effect.
        /// </summary>
        protected TEffectSettings EffectSettings;

        /// <summary>
        /// The Canvas Group component for target Module
        /// </summary>
        protected CanvasGroup ModuleCanvasGroup;

        /// <summary>
        /// The Rect Transform of this GameObject
        /// </summary>
        protected RectTransform RectTransform;

        /// <summary>
        /// The target module
        /// </summary>
        protected Module TargetModule;

        /// <summary>
        /// Checks whether this Effect is ready for playing.
        /// </summary>
        protected bool IsReady { get; private set; }

        public virtual Vector2 DestinationPosition { get; set; }

        /// <summary>
        /// Applies the effect settings for this effect
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void SetEffectSettings(TEffectSettings settings)
        {
            EffectSettings = settings;
            OnEffectSettingsSet(settings);
            IsReady = true;
        }

        /// <summary>
        /// Invokes coroutine to start an effect
        /// </summary>
        /// <returns></returns>
        protected IEnumerator BeginEffect()
        {
            if (EffectSettings == null)
                yield break;

            if (!IsReady)
                yield break;

            ResetEffect();

            yield return OnEffectBegin();
        }

        /// <summary>
        /// Called when effect is invoked
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator OnEffectBegin()
        {
            yield break;
        }

        /// <summary>
        /// Called only once.
        /// </summary>
        protected virtual void Awake()
        {
            TargetModule = GetComponent<Module>();
            RectTransform = GetComponent<RectTransform>();
            ModuleCanvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Executes this effect.
        /// </summary>
        public void Execute(Vector2 destinationPosition)
        {
            DestinationPosition = destinationPosition;
            Execute();
        }

        /// <summary>
        /// Executes this effect.
        /// </summary>
        public void Execute()
        {
            StartCoroutine(BeginEffect());
        }

        /// <summary>
        /// Called when this component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            StopCoroutine(BeginEffect());
            ResetEffect();
        }

        /// <summary>
        /// Called after effect settings are set
        /// </summary>
        /// <param name="settings"></param>
        protected virtual void OnEffectSettingsSet(TEffectSettings settings)
        {
        }

        /// <summary>
        /// Used to reset module state
        /// </summary>
        protected virtual void ResetEffect()
        {
        }
    }
}