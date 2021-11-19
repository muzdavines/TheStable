using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessOpenDoor : OnStepSuccess
{
    public Transform leftDoor, rightDoor;
    public Vector3 leftDoorTarget, rightDoorTarget;
    public bool rotateDoors = false;
    bool leftDoorOpen, rightDoorOpen;
    public string notification = "Lock Successfully Picked!";
    public override void OnSuccess() {
        FindObjectOfType<MissionController>().update.text += "\n"+notification;
        rotateDoors = true;
        if (leftDoor == null) {
            leftDoorOpen = true;
        }
        if (rightDoor == null) {
            rightDoorOpen = true;
        }
    }
    public void Update() {
        if (rotateDoors) {
            if (!leftDoorOpen) {
                leftDoor.localEulerAngles = Vector3.MoveTowards(leftDoor.localEulerAngles, leftDoorTarget, .5f);
                float currentLeft = leftDoor.localRotation.eulerAngles.y;
                float compAngle = leftDoorTarget.y < 0 ? leftDoorTarget.y + 360 : leftDoorTarget.y;
                if (Mathf.Abs(currentLeft-compAngle) < .1f) { leftDoorOpen = true; }
            }
            if (!rightDoorOpen) {
                rightDoor.localEulerAngles = Vector3.MoveTowards(rightDoor.localEulerAngles, rightDoorTarget, .5f);
                float currentRight = rightDoor.localRotation.eulerAngles.y;
                float compAngle = rightDoorTarget.y < 0 ? rightDoorTarget.y + 360 : rightDoorTarget.y;
                if (Mathf.Abs(currentRight - compAngle) < .1f) { rightDoorOpen = true; }
            }
        }
    }
}
