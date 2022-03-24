using UnityEngine;

namespace MedievalKingdomUI.Scripts
{
    public class ExitButtonController : MonoBehaviour
    {
        public void Exit()
        {
            Application.Quit(0);
        }
    }
}
