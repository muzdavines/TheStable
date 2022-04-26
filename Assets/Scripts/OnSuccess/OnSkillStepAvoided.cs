using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSkillStepAvoided : MonoBehaviour
{
    public void OnAvoided(bool minReqNotMet) {
        if (minReqNotMet) {
            print("Min Req Not Met");
        }
        else {
            print("Avoided something bad");
        }
    }
}
