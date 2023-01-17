using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Flechettes : ActiveSpecialMove {
    float lastFired;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Flechettes();
    }

    public override string GetDescription() {
        return
            "Becomes a whirlwind, spinning and sending sharp flechettes at all enemies in range. Used as a defensive move in Flonkball against opponents in the hero's own end of the pitch.";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time <= lastFired + 30) {
            return false;
        }

        if (_char.fieldSport) {
            if (!_char.state.GetType().GetInterfaces().Contains(typeof(CanFlechettes))) {
                return false;
            }

            if (_char.myGoal.Distance(_char) > 30) {
                return false;
            }

            var target = _char.FindEnemyWithinRange(10);
            if (target != null) {
                if (!_char.AcquireTarget(target)) {
                    return false;
                }
            }
            else {
                return false;
            }

            OnActivate(_char);
            return true;
        }
        else {
            var target = _char.FindEnemyWithinRange(10);
            if (target != null) {
                if (!_char.AcquireTarget(target)) {
                    return false;
                }
            }
            else {
                return false;
            }

            OnActivate(_char);
            return true;
        }
    }
}

public interface CanFlechettes {

}