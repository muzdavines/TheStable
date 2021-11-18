using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace VikingCrew.Tools.UI
{
    public class SpeechBubbleTmp : SpeechBubbleBase
    {
        [SerializeField] private TextMeshProUGUI _text;
        protected override void SetTextAlpha(float alpha)
        {
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);
        }

        protected override void SetText(string text)
        {
            _text.text = text;
        }
    }
}