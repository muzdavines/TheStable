using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Aiming step.
    /// </summary>
    public enum AimStep
    {
        None,
        Enter,
        Aiming
    }

    /// <summary>
    /// Maintains state of aiming in cover.
    /// </summary>
    public struct CoverAimState
    {
        /// <summary>
        /// Time in seconds to enter the aiming state.
        /// </summary>
        public const float TimeEnterToAim = 0.2f;

        /// <summary>
        /// Time in seconds to leave the aiming state.
        /// </summary>
        public const float TimeAimToLeave = 0.15f;

        /// <summary>
        /// Is in aiming state or currently tansitioning to it.
        /// </summary>
        public bool IsAiming
        {
            get { return Step == AimStep.Enter || Step == AimStep.Aiming; }
        }

        /// <summary>
        /// Current state of aiming.
        /// </summary>
        public AimStep Step;

        /// <summary>
        /// Degrees in world space to aim at.
        /// </summary>
        public float Angle;

        /// <summary>
        /// Is currently using zoom.
        /// </summary>
        public bool IsZoomed;

        /// <summary>
        /// Time in seconds to transition to the next step.
        /// </summary>
        public float TimeLeftForNextStep;

        /// <summary>
        /// Will leave state of aiming immediately after entering.
        /// </summary>
        public bool LeaveAfterAiming;

        /// <summary>
        /// Updates current state.
        /// </summary>
        public void Update()
        {
            if (TimeLeftForNextStep >= -float.Epsilon)
                TimeLeftForNextStep -= Time.deltaTime;
            else
                switch (Step)
                {
                    case AimStep.Enter:
                        Step = AimStep.Aiming;
                        TimeLeftForNextStep = TimeAimToLeave;
                        break;

                    case AimStep.Aiming:
                        if (LeaveAfterAiming)
                        {
                            Step = AimStep.None;
                            LeaveAfterAiming = false;
                        }
                        break;
                }
        }

        /// <summary>
        /// Immediately enters aiming without any transitions.
        /// </summary>
        public void ImmediateEnter()
        {
            LeaveAfterAiming = false;
            TimeLeftForNextStep = 0;
            Step = AimStep.Aiming;
        }

        /// <summary>
        /// Immediately cancels aiming without any transitions.
        /// </summary>
        public void ImmediateLeave()
        {
            LeaveAfterAiming = false;
            TimeLeftForNextStep = 0;
            Step = AimStep.None;
        }

        /// <summary>
        /// Starts off a transition to leave aiming.
        /// </summary>
        public void Leave()
        {
            switch (Step)
            {
                case AimStep.Enter:
                    LeaveAfterAiming = true;
                    break;

                case AimStep.Aiming:
                    if (!LeaveAfterAiming)
                    {
                        LeaveAfterAiming = true;
                        TimeLeftForNextStep = TimeAimToLeave;
                    }
                    break;
            }
        }

        /// <summary>
        /// Turns on aiming immediately, used when not in cover.
        /// </summary>
        /// <param name="angle">Degrees in world space to point the gun at.</param>
        public void FreeAim(float angle)
        {
            Angle = angle;
            Step = AimStep.Aiming;
        }

        /// <summary>
        /// Starts off a transition to aim, used when in cover.
        /// </summary>
        /// <param name="angle">Degrees in world space to point the gun at.</param>
        public void CoverAim(float angle)
        {
            Angle = angle;

            if (Step == AimStep.Aiming)
                LeaveAfterAiming = false;
            else if (Step != AimStep.Enter)
            {
                Step = AimStep.Enter;
                TimeLeftForNextStep = TimeEnterToAim;
            }
        }

        /// <summary>
        /// Starts off a transition to aim, wait's for ImmediateAim() to finish it.
        /// </summary>
        /// <param name="angle">Degrees in world space to point the gun at.</param>
        public void WaitAim(float angle)
        {
            Angle = angle;

            if (Step == AimStep.Aiming)
                LeaveAfterAiming = false;
            else if (Step != AimStep.Enter)
            {
                Step = AimStep.Enter;
                TimeLeftForNextStep = 10000;
            }
        }
    }
}