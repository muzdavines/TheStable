using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnSuccessConversation : OnStepSuccess {
    public GameObject goToDestroy;
    public override void OnSuccess() {
       // Destroy(goToDestroy);
    }
}
