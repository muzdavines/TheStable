using Animancer;
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
    public AnimationClip avoidStrip;
    public AnimationClip getStripped;
    public AnimationClip successStrip;
    public AnimationClip failStrip;
    public AnimationClip[] dodgeTackle;
    public AnimationClip oneTimer;
    public AnimationClip goalScored;
    public AnimationClip takeDamage;
    public AnimationClip takeOutSword;
    public AnimationClip[] skills;
    public AnimationClip shoulderBarge;
    public AnimationClip backstab;
    public AnimationClip backstabVictim;
    public AnimationClip assassinate;
    public AnimationClip flameCircle;
    public AnimationClip powerSlam;
    public AnimationClip flechettes;
    public AnimationClip summon;
    public AnimationClip gkSwat;
    public AnimationClip runForward;
    public AnimationClip jumpCatch;
    public AnimationClip kneecap;
    public AnimationClip getKneecapped;
    public AnimationClip uncannyDodge;
    public AnimationClip bolaThrow;
    public AnimationClip pistolShot;
    public AnimationClip viciousMockery;
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
        anim.avoidStrip.Clip = avoidStrip;
        anim.getStripped.Clip = getStripped;
        anim.failStrip.Clip = failStrip;
        anim.successStrip.Clip = successStrip;
        for (int i = 0; i < dodgeTackle.Length; i++) {
            anim.dodgeTackle[i].Clip = dodgeTackle[i];
        }
        for (int x = 0; x < skills.Length; x++) {
            anim.skills[x].Clip = skills[x];
            anim.skills[x].Speed = 1;
        }
        anim.oneTimer.Clip = oneTimer;
        anim.goalScored.Clip = goalScored;
        anim.takeDamage.Clip = takeDamage;
        anim.takeOutSword.Clip = takeOutSword;
        anim.shoulderBarge.Clip = shoulderBarge;
        anim.backstab.Clip = backstab;
        anim.backstabVictim.Clip = backstabVictim;
        anim.flameCircle.Clip = flameCircle;
        anim.powerSlam.Clip = powerSlam;
        anim.flechettes.Clip = flechettes;
        anim.summon.Clip = summon;
        anim.gkSwat.Clip = gkSwat;
        anim.runForward.Clip = runForward;
        anim.jumpCatch.Clip = jumpCatch;
        anim.assassinate.Clip = assassinate;
        anim.kneecap.Clip = kneecap;
        anim.getKneecapped.Clip = getKneecapped;
        anim.uncannyDodge.Clip = uncannyDodge;
        anim.bolaThrow.Clip = bolaThrow;
        anim.pistolShot.Clip = pistolShot;
        anim.viciousMockery.Clip = viciousMockery;
    }
}
