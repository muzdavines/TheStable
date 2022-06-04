using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCFaceSmash :SCBallCarrierState, CannotInterrupt, CannotTarget, CannotSpecial {
    public float timeOut;
    public Transform attackTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (ball.holder== null || ball.holder != thisChar) {
            thisChar.Idle();
            return;
        }
        timeOut = Time.time + 8f;
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        thisChar._t.LookAt(attackTarget);

        thisChar.anima.FaceSmash();
        
        thisChar.DisplaySpecialAbilityFeedback("Face Smash");
       
    }
    bool thrown;
    Vector3 oldScale;
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
        thisChar.agent.velocity = Vector3.zero;
        if (attackTarget != null) {
            thisChar.transform.LookAt(attackTarget.transform);
        }
        if (fakeBall == null) {
            return;
        }
        if (fakeBall.GetComponent<FaceSmashEffect>().finished) {
            thisChar.ball.transform.localScale = new Vector3(35,35,35);
            thisChar.Idle();
            thrown = false;
            GameObject.Destroy(fakeBall);
        }

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "FaceSmash") {
            ThrowBall();
        }
        
    }
    GameObject fakeBall;
    void ThrowBall() {
        oldScale = thisChar.ball.transform.localScale;
        thisChar.ball.transform.localScale = Vector3.zero;
        fakeBall = GameObject.Instantiate(Resources.Load<GameObject>("FaceSmashEffect"));
        fakeBall.transform.position = thisChar.ball.transform.position;
        fakeBall.GetComponent<FaceSmashEffect>().Init(attackTarget, thisChar);
        thrown = true;
    }
    
    public override void WillExit() {
        base.WillExit();
        
    }
}