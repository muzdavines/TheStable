using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessOpenChest : OnStepSuccess {
    public Transform leftDoor;
    public Vector3 leftDoorTarget;
    public bool rotateDoors = false;
    bool leftDoorOpen;
    public string notification = "Chest Successfully Opened!";
    public override void OnSuccess() {
        FindObjectOfType<MissionController>().update.text += "\n" + notification;
        rotateDoors = true;
        if (leftDoor == null) {
            leftDoorOpen = true;
        }
        
    }
    public void Update() {
        if (rotateDoors) {
            if (!leftDoorOpen) {
                leftDoor.localEulerAngles = Vector3.MoveTowards(leftDoor.localEulerAngles, leftDoorTarget, .1f);
                float currentLeft = leftDoor.localRotation.eulerAngles.y;
                float compAngle = leftDoorTarget.x < 0 ? leftDoorTarget.x + 360 : leftDoorTarget.x;
                if (Mathf.Abs(currentLeft - compAngle) < .1f) { leftDoorOpen = true; }
            }
            
        }
    }
}
