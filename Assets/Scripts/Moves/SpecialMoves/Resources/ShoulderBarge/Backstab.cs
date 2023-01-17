using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Backstab : ActiveSpecialMove {
    private float lastFired = -45;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.Backstab();
    }

    public override string GetDescription() {
        return "Flash behind an enemy and deliver a devastating attack, knocking them down in the process.";
    }

   
    public override bool Check(StableCombatChar _char) {
        Debug.Log("#Backstab#Checking " + _char.myCharacter.name);
        if (Time.time <= lastFired + 45) {
           Debug.Log("#Backstab#CoolingDown");
            return false;
        }
        Debug.Log("#Backstab#Not CoolingDown");
        if (_char.ball != null) {
            if (_char.ball.passTarget == _char) {
                return false;
            }
        }
        if (!_char.state.GetType().GetInterfaces().Contains(typeof(CanBackStab))) {
            Debug.Log("#Backstab#Can't Backstab State");
            return false;
        }
        var checkList = _char.FindAllEnemiesWithinRange(500);
        StableCombatChar target = null;
        foreach (var c in checkList)
        {
            if (c.IsHealer())
            {
                Debug.Log("#Backstab#Found Healer");
               target = c;
                break;
            }
        }
        if (target == null)
        {
            target = _char.FindEnemyWithinRange(7);
        }
        if (target != null) {
            if (!_char.AcquireTarget(target)) {
                Debug.Log("#Backstab#Can't Aquire Target " + target.name);
                return false;
            }
        }
        else {
            Debug.Log("#Backstab#Couldn't find valid target");
            return false;
        }
        Debug.Log("#Backstab#Activating "+_char.myCharacter.name);
        OnActivate(_char);
        return true;
    }
}

public interface CanBackStab {

}
