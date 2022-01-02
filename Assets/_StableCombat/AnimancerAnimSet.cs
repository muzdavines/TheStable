using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AnimancerAnimSet : ScriptableObject
{
    public AnimationClip shootBall;
    public AnimationClip tackle;
    public AnimationClip knockdown;
    public AnimationClip downOnGround;
    public AnimationClip stayOnGround;
    public AnimationClip passBall;
    public AnimationClip catchBall;
    public AnimationClip missTackle;
    public AnimationClip dodgeTackle;

    public void Init(AnimancerController anim) {
        Debug.Log("Initialize Animset");
        anim.shootBall.Clip = shootBall;
        anim.tackle.Clip = tackle;
        anim.knockdown.Clip = knockdown;
        anim.downOnGround.Clip = downOnGround;
        anim.stayOnGround.Clip = stayOnGround;
        anim.passBall.Clip = passBall;
        anim.catchBall.Clip = catchBall;
        anim.missTackle.Clip = missTackle;
        anim.dodgeTackle.Clip = dodgeTackle;
    }
}
