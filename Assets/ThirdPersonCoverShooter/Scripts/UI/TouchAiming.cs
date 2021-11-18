namespace CoverShooter
{
    /// <summary>
    /// Takes directional input from the touch screen and passes it to Mobile Controller as aim direction.
    /// </summary>
    public class TouchAiming : TouchBase
    {
        protected override void Update()
        {
            base.Update();

            if (Controller == null)
                return;

            if (Delta.sqrMagnitude > float.Epsilon)
            {
                Controller.IsAllowedToFire = !IsCancelled;
                Controller.HasAiming = true;
                Controller.Aiming = Delta;
                Controller.Magnitude = Magnitude;
            }
            else
                Controller.HasAiming = false;                
        }
    }
}
