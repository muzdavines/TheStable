using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SCInjuredBuff : SCSpeedBuff {
    /// <summary>
    /// Initializes the Buff
    /// </summary>
    /// <param name="_duration"></param>
    /// <param name="_startTime"></param>
    /// <param name="speedMod">speedMod should be in units of speed for the Character attribute. The method will convert the agent speed</param>
    AnimancerController anim;
    public override SCSpeedBuff Init(float _duration, float _speedMod) {
        anim = GetComponent<AnimancerController>();
        anim.injured = true;
        base.Init(_duration, _speedMod);
        return this as SCSpeedBuff;
    }
    public override void EndEffect() {
        anim.injured = false;
        base.EndEffect();
        
    }
    public override void OnDestroy() {
        anim.injured = false;
    }


}
