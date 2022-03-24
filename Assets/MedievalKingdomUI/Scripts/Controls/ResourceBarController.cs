using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class ResourceBarController : MonoBehaviour
    {
        public Image image;
        private static readonly int FillLevel = Shader.PropertyToID("_FillLevel");

        public void ApplyValue(float value)
        {
            if (image == null)
            {
                return;
            }
            image.material.SetFloat(FillLevel, value);
        }
    }
}
