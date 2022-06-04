using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SCSpeedBuff : SCBuff
{
    /// <summary>
    /// Initializes the Buff
    /// </summary>
    /// <param name="_duration"></param>
    /// <param name="_startTime"></param>
    /// <param name="speedMod">speedMod should be in units of speed for the Character attribute. The method will convert the agent speed</param>
    float speedMod;
    public virtual SCSpeedBuff Init(float _duration, float _speedMod) {
        base.Init(_duration);
        speedMod = _speedMod;
        Debug.Log("#SpeedMod#" + _duration + " " + _speedMod);
        GetComponent<NavMeshAgent>().speed += (speedMod * .4f);
        return this;
    }
    public override void Start() {
        base.Start();
    }
    public override void Update() {
        base.Update();
    }
    public override void AddEffect() {
        base.AddEffect();
        print("#TODO#Add Kripto glow on guy");
        
    }
    public override void RemoveEffect() {
        base.RemoveEffect();
    }
    public override void IntervalEffect() {
        base.IntervalEffect();
    }
    public override void EndEffect() {
        base.EndEffect();
    }
    public virtual void OnDestroy() {
        GetComponent<NavMeshAgent>().speed -= (speedMod * .4f);
    }
}
