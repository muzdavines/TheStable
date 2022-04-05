using System;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class SliderPercentController : MonoBehaviour
    {
        public Text textComponent;

        public void UpdatePercentValue(float value)
        {
            textComponent.text = $"{value:F0} %";
        }
    }
}