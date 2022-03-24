using UnityEngine;
using UnityEngine.Events;

namespace MedievalKingdomUI.Scripts.Controls
{
    public class EventTriggerInvoker : MonoBehaviour
    {
        public UnityEvent eventToTrigger;

        public void Trigger()
        {
            eventToTrigger.Invoke();
        }
    }
}
