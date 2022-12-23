using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialMove : Move
{
    public string stateToCall;
    public bool passiveMove = false;
    public bool sportMove = false;
    public abstract bool Check(StableCombatChar _char);
    public abstract void OnActivate(StableCombatChar _char);
    public string MyName() {
        return this.GetType().ToString();
    }

}

public interface HealingMove {

}
