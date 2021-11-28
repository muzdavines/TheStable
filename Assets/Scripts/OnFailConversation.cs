using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFailConversation :  OnStepFail {
    public GameObject goToDestroy;
    public override void OnFail() {
        // Destroy(goToDestroy);
    }
}