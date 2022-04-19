using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GKBlocker : MonoBehaviour
{
    StableCombatChar myChar;
    float lastSwat;
    private void Start() {
        myChar = GetComponentInParent<StableCombatChar>();
    }
    public void OnTriggerEnter(Collider other) {
        Debug.Log("#GKBlocker#Entered: " + other.transform.name + "  "+other.gameObject.layer);
        if (other.gameObject.layer != 14) {
            return;   
        }
        if (Time.time < lastSwat + 5) {
            return;
        }
        var ball = other.GetComponent<Ball>();
        if (ball == null) {
            return;
        }
        if (ball.isHeld) {
        //    return;
        }
        if (ball.lastHolder?.team == myChar.team) {
            return;
        }
        lastSwat = Time.time + 5;
        myChar.GKSwat();
    }
}
