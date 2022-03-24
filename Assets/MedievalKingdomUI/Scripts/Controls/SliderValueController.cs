using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class SliderValueController : MonoBehaviour
    {
        public Text textComponent;

        public void UpdatePercentValue(float value)
        {
            textComponent.text = $"{value:F0}/{GetComponent<Slider>().maxValue:F0}";
        }
    }
}
