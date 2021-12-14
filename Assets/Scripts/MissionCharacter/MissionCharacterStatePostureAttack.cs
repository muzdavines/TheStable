using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCharacterStatePostureAttack : MissionCharacterCombatState
{

    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        thisChar.posture = Posture.Attack;
        thisChar.detectRange = 100;
    }
    public override void Update() {
        base.Update();
        Debug.Log(thisChar.name + " " + InAttackRange());
        if (InAttackRange()) {
            thisChar.state.TransitionTo(new MissionCharacterStateAttack());
        } else {
            thisChar.state.TransitionTo(new MissionCharacterStatePursueTarget());
        }
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }

    public override void WillExit() {
        base.WillExit();
    }      
}

