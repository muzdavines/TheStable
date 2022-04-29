#if OOTII_MC && OOTII_CC

using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Cameras;
using com.ootii.Messages;

public class demo_MotionController_code : MonoBehaviour
{
    private MotionController lMC;
    private BasicWalkRunPivot lPivot;
    private BasicWalkRunStrafe lStrafe;

    private void Start()
    {
        MotionController lMC = Component.FindObjectOfType<MotionController>();
        if (lMC == null) { return; }

        lPivot = lMC.GetMotion<BasicWalkRunPivot>();
        lStrafe = lMC.GetMotion<BasicWalkRunStrafe>();
    }

    public void OnCameraTransition(IMessage rMessage)
    {
        if (rMessage.ID != EnumMessageID.MSG_CAMERA_MOTOR_ACTIVATE) { return; }
        if (lPivot == null || lStrafe == null) { return; }

        CameraMessage lMessage = rMessage as CameraMessage;
        if (lMessage == null) { return; }
        
        if (lMessage.Motor.Name == "To 1st Person")
        {
            lPivot.IsEnabled = false;
            lStrafe.IsEnabled = true;
        }
        else if (lMessage.Motor.Name == "To 3rd Person")
        {
            lPivot.IsEnabled = true;
            lStrafe.IsEnabled = false;
        }
    }
}

#endif
