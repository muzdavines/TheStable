namespace CoverShooter
{
    /// <summary>
    /// Takes directional movement input from the touch screen and passes it to Mobile Controller.
    /// </summary>
    public class TouchMovement : TouchBase
    {
        protected override void Update()
        {
            base.Update();

            if (Controller == null)
                return;

            if (Delta.sqrMagnitude > float.Epsilon)
            {
                Controller.HasMovement = true;
                Controller.Movement = Delta;
            }
            else
                Controller.HasMovement = false;
        }
    }
}
