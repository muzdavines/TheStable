using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStatePostureIdle : MissionCharacterCombatState
{
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        thisChar.posture = Posture.Idle;
        target = null;
    }
    public override void Update() {
        base.Update();
        target = FindEnemy();
        if (target != null) {
            thisChar.ChangePostureAttack();
            return;
        }
        target = FindBall();
        if (target != null) {
            //changeState to PursueBall
        }
        target = FindLeader();
        if (target != null) {
            //changeSTate to FollowLeader
        }
        target = FindPOI();
        if (target != null) {
            //changeSTate to DefendArea
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
