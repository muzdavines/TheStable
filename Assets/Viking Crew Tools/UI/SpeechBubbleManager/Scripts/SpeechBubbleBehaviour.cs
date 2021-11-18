using UnityEngine;
using UnityEngine.UI;


namespace VikingCrew.Tools.UI {

    
    /// <summary>
    /// A specific implementation of speech bubble that uses old Unity UI text.
    /// For best results it is recommended to use the newer Text Mesh Pro version
    /// </summary>
	public class SpeechBubbleBehaviour : SpeechBubbleBase
    {   
        [SerializeField] private Text _text;

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