using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimancerTest : MonoBehaviour
{
    public AnimationClip attack;
    public AnimationClip idle;
    public AnimancerComponent anim;
    public AnimancerState attackState;
    void Start()
    {
        anim.Play(idle);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
