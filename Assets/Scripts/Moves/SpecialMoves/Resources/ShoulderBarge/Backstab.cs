using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Backstab : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Backstab();
    }

    public override bool Check(StableCombatChar _char) {
        Debug.Log("#Backstab#Checking " + _char.myCharacter.name);
        if (Time.time <= lastFired + 60) {
            Debug.Log("#Backstab#CoolingDown");
            return false;
        }
        if (!_char.state.GetType().GetInterfaces().Contains(typeof(CanBackStab))) {
            Debug.Log("#Backstab#Can't Backstab State");
            return false;
        }
        var target = _char.FindEnemyWithinRange(7);
        if (target != null) {
            _char.myAttackTarget = target;
        } else { Debug.Log("#Backstab#TargetNull"); return false; }
        Debug.Log("#Backstab#Activating "+_char.myCharacter.name);
        OnActivate(_char);
        return true;
    }
}

public interface CanBackStab {

}
