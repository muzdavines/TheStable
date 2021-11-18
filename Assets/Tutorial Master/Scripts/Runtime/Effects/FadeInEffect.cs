using System.Collections;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/fade-in-effect")]
    public class FadeInEffect : Effect<FadeInEffectSettings>, IEffectOneShot
    {
        public event EffectEvent EffectEnd;
        public event EffectEvent EffectStart;

        public bool HasFinished { get; set; }

        private bool _initialInteractable;
        private float _initialAlpha;

        protected override void OnEffectSettingsSet(FadeInEffectSettings settings)
        {
            // capture initial settings
            _initialAlpha = ModuleCanvasGroup.alpha;
            _initialInteractable = ModuleCanvasGroup.interactable;

            ModuleCanvasGroup.alpha = 0;
            ModuleCanvasGroup.interactable = EffectSettings.CanInteract;

            OnEffectStart();
        }

        /// <inheritdoc />
        protected override IEnumerator OnEffectBegin()
        {
            float lerpAmount = 0;

            while (lerpAmount < 1)
            {
                lerpAmount += EffectSettings.Speed * Time.fixedDeltaTime;
                ModuleCanvasGroup.alpha = Mathf.Lerp(0, 1, lerpAmount);
                yield return new WaitForFixedUpdate();
            }

            if (!EffectSettings.CanInteract)
                ModuleCanvasGroup.interactable = true;

            OnEffectEnd();
            yield return null;
        }

        protected virtual void OnEffectEnd()
        {
            HasFinished = true;
            var handler = EffectEnd;
            if (handler != null)
                handler();
        }

        protected virtual void OnEffectStart()
        {
            HasFinished = false;
            var handler = EffectStart;
            if (handler != null)
                handler();
        }

        protected override void ResetEffect()
        {
            ModuleCanvasGroup.alpha = _initialAlpha;
            ModuleCanvasGroup.interactable = _initialInteractable;
        }
    }
}