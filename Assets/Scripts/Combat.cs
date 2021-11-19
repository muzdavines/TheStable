using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat 
{
    public enum Intent { Neutral, Attack, Defend, Use, Cast, Interact, Move}
    public enum Modifier {Neutral, Defensive, Balanced, Full, Armor, Parry, Dodge, Walk, Run, Sneak }
    public enum State { Neutral, Engaged, KnockedDown, Blinded, Flanked, Dead}

    
}

public static class CombatHelper {
    public static int GetValue(this Combat.Modifier mod) {
        switch (mod) {
            case Combat.Modifier.Defensive:
                return -1;
                break;
            case Combat.Modifier.Balanced:
                return 0;
                break;
            case Combat.Modifier.Full:
                return 1;
                break;
            case Combat.Modifier.Armor:
                return -5;
        }
        return 0;
    }
}