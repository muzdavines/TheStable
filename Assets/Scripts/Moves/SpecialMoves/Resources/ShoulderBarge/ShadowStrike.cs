using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowStrike : ActiveSpecialMove {
    private float lastFired = -45;
    private StableCombatChar target;
    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.ShadowStrike(target);
    }

    public override bool Check(StableCombatChar _char) {
        Debug.Log("#ShadowStrike#Checking " + _char.myCharacter.name);
        if (Time.time <= lastFired + 10) {
            Debug.Log("#ShadowStrike#CoolingDown");
            return false;
        }
        Debug.Log("#ShadowStrike#Not CoolingDown");
        if (_char.ball != null) {
            if (_char.ball.passTarget == _char) {
                return false;
            }
        }
        if (!_char.state.GetType().GetInterfaces().Contains(typeof(CanBackStab))) {
            Debug.Log("#ShadowStrike#Can't Backstab State");
            return false;
        }
        var checkList = _char.FindAllEnemiesWithinRange(15);
        target = null;
        foreach (var c in checkList) {
            if (c.balance < 50) {
                target = c;
            }
        }
        
        if (target != null) {
            if (!_char.AcquireTarget(target)) {
                Debug.Log("#ShadowStrike#Can't Aquire Target " + target.name);
                return false;
            }
        }
        else {
            Debug.Log("#ShadowStrike#Couldn't find valid target");
            return false;
        }
        Debug.Log("#ShadowStrike#Activating " + _char.myCharacter.name);
        OnActivate(_char);
        return true;
    }
}