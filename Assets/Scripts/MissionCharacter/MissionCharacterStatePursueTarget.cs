using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStatePursueTarget : MissionCharacterCombatState
{

    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
    }
    public override void Update() {
        base.Update();
        if (thisChar.DistanceToTarget() > thisChar.maxAttackRange*.9f) {
            thisChar.RunAtTarget();
        } else {
            thisChar.StopAndFaceTarget();
            thisChar.state.TransitionTo(new MissionCharacterStateAttack());
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }
}
