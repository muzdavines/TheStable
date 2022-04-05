using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class BinarySliderController : MonoBehaviour
    {
        public Text valueTitle;
        public Slider slider;

        public string[] values;
        public Color[] titleColors;

        private void Start()
        {
            if (values.Length != 2 
                || titleColors.Length != 2
                || valueTitle == null
                || slider == null)
            {
                gameObject.SetActive(false);
            }
        }

        public void Turn(bool value)
        {
            var decimalValue = value ? 1 : 0;
            slider.value = slider.maxValue * decimalValue;
            valueTitle.text = values[decimalValue];
            valueTitle.color = titleColors[decimalValue];
        }
    }
}