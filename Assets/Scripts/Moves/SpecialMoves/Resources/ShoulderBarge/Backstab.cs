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
        if (Time.time <= lastFired + 60) {
            return false;
        }
        if (!_char.state.GetType().GetInterfaces().Contains(typeof(CanBackStab))) {
            return false;
        }
        var target = _char.FindEnemyWithinRange(7);
        if (target != null) {
            _char.myAttackTarget = target;
        } else { return false; }
        OnActivate(_char);
        return true;
    }
}

public interface CanBackStab {

}
