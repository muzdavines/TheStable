using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class SliderStepController : MonoBehaviour
    {
        public Text textComponent;
        public Slider sliderComponent;
        public string[] values;

        private void Start()
        {
            if (textComponent == null
                || sliderComponent == null
                || values.Length == 0)
            {
                gameObject.SetActive(false);
            }

            sliderComponent.maxValue = values.Length - 1;
            sliderComponent.wholeNumbers = true;
        }

        public void UpdatePercentValue(float value)
        {
            var intValue = (int)value;
            textComponent.text = values[intValue];
        }
    }
}