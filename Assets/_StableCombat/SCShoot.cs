using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCShoot : StableCombatCharState
{

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        thisChar.anim.SetTrigger("ShootBall");
        thisChar.agent.isStopped = true;
    }
    public override void Update() {
        base.Update();
        thisChar.agent.isStopped = true;
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "Throw") {
            thisChar.transform.LookAt(thisChar.enemyGoal.transform.position);
            thisChar.ball.Shoot(thisChar.enemyGoal);
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}
