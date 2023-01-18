using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTeammate : ActiveSpecialMove {
    float lastFired;

    public override void OnActivate(StableCombatChar _char) {
        base.OnActivate(_char);
        lastFired = Time.time;
        _char.TeleportTeammate();
    }

    public override string GetName() {
        return "Teleport Teammate";
    }

    public override string GetDescription() {
        return "Teleports a teammate ball carrier to a spot near the opposing goal";
    }

    public override bool Check(StableCombatChar _char) {
        if (Time.time < lastFired + 20) {
            return false;
        }

        if (_char.ball == null || _char.ball.holder == null || _char.ball.holder == _char || _char.ball.holder.team != _char.team) {
            return false;
        }

        float distToGoal = Vector3.Distance(_char.ball.transform.position, _char.enemyGoal.transform.position);
        if (distToGoal < 25 || distToGoal > 55) {
            return false;}
        Debug.Log("#TeleportTeammate#Activate");
        OnActivate(_char);
        return true;
    }

}