using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Displays a button inside the inspector that triggers a manual regroup.
    /// </summary>
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(AIMovement))]
    public class AIRegrouperButton : AIBaseRegrouper
    {
    }
}
