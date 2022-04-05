using UnityEngine;

namespace MedievalKingdomUI.Scripts.Window
{
    public class WindowController : MonoBehaviour
    {
        public GameObject initialActiveWindow;

        private GameObject _activeWindow;

        private void Start()
        {
            _activeWindow = initialActiveWindow;
        }
        
        public virtual void OpenWindow(GameObject window)
        {
            _activeWindow.SetActive(false);
            window.SetActive(true);
            _activeWindow = window;
        }
    }
}
