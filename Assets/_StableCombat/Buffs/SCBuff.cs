using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBuff : MonoBehaviour
{
    public float duration;
    public float startTime;
    public bool init;
    public bool endCalled;
    public virtual void Init(float _duration) {
        duration = _duration;
        startTime = Time.time;
        init = true;
    }
    public virtual void Start() {

    }
    public virtual void Update() {
        if (!init) { return; }
        if (Time.time >= startTime + duration) {
            EndEffect();
        }
    }
    public virtual void AddEffect() {

    }
    public virtual void RemoveEffect() {

    }
    public virtual void IntervalEffect() {

    }
    public virtual void EndEffect() {
        Destroy(this);
    }

}
