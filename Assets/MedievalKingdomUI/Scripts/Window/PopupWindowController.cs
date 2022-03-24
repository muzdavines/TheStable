using UnityEngine;

namespace MedievalKingdomUI.Scripts.Window
{
    public class PopupWindowController : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
