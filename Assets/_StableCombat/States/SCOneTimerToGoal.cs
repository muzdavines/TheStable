using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCOneTimerToGoal : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anima.OneTimer();
        thisChar.transform.LookAt(thisChar.enemyGoal.transform);
        thisChar.agent.isStopped = true;
        canGrabBall = false;
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    public override void BallCollision(Collision collision) {
        // Failed One Timer
        if (Random.value > thisChar.myCharacter.shooting * .01f)
        {
            thisChar.GetTackled(null);
        }
        // Succeed One Timer
        else
        {
            ball.lastHolder = thisChar;
            ball.Shoot(thisChar.enemyGoal.transform.position + (thisChar.enemyGoal.transform.right * Random.Range(2f, 2.5f) * (Random.Range(0f, 1f) > .5f ? 1 : -1)), 0, 1);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
