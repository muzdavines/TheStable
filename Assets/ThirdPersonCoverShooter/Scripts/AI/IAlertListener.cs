using UnityEngine;

namespace CoverShooter
{
    public interface IAlertListener
    {
        void OnAlert(ref GeneratedAlert alert);
    }
}
