using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSpecialMove : SpecialMove {
   
    public override bool Check(StableCombatChar _char) {
        return false;   
    }
    public override void OnActivate(StableCombatChar _char) {
        
    }

    public virtual bool SpotCheck(StableCombatChar _char) {
        return false;
    }

   

}
