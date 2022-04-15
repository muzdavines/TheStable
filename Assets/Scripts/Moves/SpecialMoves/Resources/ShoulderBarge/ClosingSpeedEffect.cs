using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosingSpeedEffect : MonoBehaviour
{
    public float startTime;
    bool init;
    void Start()
    {
        startTime = Time.time;
        init = true;
    }

    // Update is called once per frame
    void Update()
    {
         if (!init) { return; }
        if (Time.time >= startTime + 2f) {
            init = false;
            GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
