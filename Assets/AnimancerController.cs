using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using UnityEngine.AI;

public class AnimancerController : MonoBehaviour {
    public LinearMixerTransitionAsset.UnShared movement;
    public AnimancerComponent anim;
    public AnimancerAnimSet animSet;
    public ClipTransition shootBall;
    public ClipTransition tackle;
    public ClipTransition knockdown;
    public ClipTransition downOnGround;
    public ClipTransition stayOnGround;
    public ClipTransition passBall;
    public ClipTransition catchBall;
    public ClipTransition missTackle;
    public ClipTransition[] dodgeTackle;
    public ClipTransition avoidStrip;
    public ClipTransition failStrip;
    public ClipTransition getStripped;
    public ClipTransition successStrip;
    public ClipTransition oneTimer;
    public ClipTransition goalScored;
    public ClipTransition takeDamage;
    public ClipTransition takeOutBow;
    public ClipTransition takeOutSword;
    public ClipTransition[] skills;
    public List<Move> baseMeleeAttackMoves;
    public List<Move> baseRangedAttackMoves;
    NavMeshAgent agent;
    StableCombatChar thisChar;
    public bool shouldBackpedal;
    public enum SkillAnims { Hunt = 0, NavigateLand = 0 };

    private void Awake() {
        Debug.Log("Init AnimSet");
        animSet.Init(this);
    }
    void Start() {
        thisChar = GetComponent<StableCombatChar>();
        anim = GetComponent<AnimancerComponent>();
        agent = GetComponent<NavMeshAgent>();
        baseMeleeAttackMoves = thisChar.meleeAttackMoves;
        baseRangedAttackMoves = thisChar.rangedAttackMoves;
        anim.Play(failStrip);
    }

    public void Idle() {
        anim.Play(movement);
        movement.State.Root.Component.Animator.applyRootMotion = false;
    }
    // _Animancer.Play(_Action, 0.25f, FadeMode.FromStart)
    //.Events.OnEnd = () => _Animancer.Play(_Idle, 0.25f);
    public void Knockdown() {
        anim.Play(knockdown, .25f, FadeMode.FromStart).Events.OnEnd = () => anim.Play(downOnGround, .25f).Events.OnEnd = () => anim.Play(stayOnGround, .25f).Events.OnEnd = () => thisChar.Idle();
    }

    public void MissTackle() {
        var thisMissTackle = anim.Play(missTackle, .25f, FadeMode.FromStart);
        thisMissTackle.NormalizedEndTime = .25f;
        thisMissTackle.Events.OnEnd = () => Knockdown();
    }

    public void Tackle() {
        anim.Play(tackle, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void PassBall() {
        anim.Play(passBall, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void ShootBall() {
        anim.Play(shootBall, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void OneTimer() {
        anim.Play(oneTimer, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void GoalScored() {
        anim.Play(goalScored, .25f, FadeMode.FromStart);
    }

    public void DodgeTackle(string dodgeType = "Front") {
        int thisClip = dodgeType == "Front" ? 0 : 1;
        anim.Play(dodgeTackle[thisClip], .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        dodgeTackle[thisClip].State.Root.Component.Animator.applyRootMotion = true;
    }
    public void AvoidStrip() {
        anim.Play(avoidStrip, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        avoidStrip.State.Root.Component.Animator.applyRootMotion = true;
    }
    public void GetStripped() {
        anim.Play(getStripped, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        getStripped.State.Root.Component.Animator.applyRootMotion = true;
    }

    public void SuccessStrip() {
        anim.Play(successStrip, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        successStrip.State.Root.Component.Animator.applyRootMotion = true;
    }

    public void FailStrip() {
        anim.Play(successStrip, .25f, FadeMode.FromStart).Events.OnEnd = () => anim.Play(failStrip, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        successStrip.State.Root.Component.Animator.applyRootMotion = true;
        failStrip.State.Root.Component.Animator.applyRootMotion = true;
    }
    public void CatchBall() {
        Debug.Log("#TODO# Add Mask for Catching");
        anim.Play(catchBall, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void TakeOutSword() {
        anim.Play(takeOutSword, .25f, FadeMode.FromStart).Events.OnEnd = () => Idle();
    }

    public void GetDowned() {
        anim.Play(knockdown, .25f, FadeMode.FromStart).Events.OnEnd = () => anim.Play(downOnGround, .25f);
    }

    public void SkillHunt() {
        anim.Play(skills[(int)SkillAnims.Hunt], .25f, FadeMode.FromStart).Events.OnEnd = () => Idle();
    }

    public void SkillNavigateLand() {
        anim.Play(skills[(int)SkillAnims.NavigateLand], .25f, FadeMode.FromStart).Events.OnEnd = () => Idle();
    }
    public void SkillGeneric() {
        Debug.Log("#TODO#Generic Skill Fired in Animancer Controller");
        anim.Play(skills[(int)SkillAnims.Hunt], .25f, FadeMode.FromStart).Events.OnEnd = () => Idle();
    }
    public void GKBored() {

    }
    public int currentMeleeAttackIndex;
    public Move currentMeleeMove;
    public int currentRangedAttackIndex;
    public Move currentRangedMove;
    
    public void FireBaseMeleeAttackMoves() {
        if (currentMeleeAttackIndex > 0) { return; }
        currentMeleeAttackIndex = 0;
        currentRangedAttackIndex = -1;
        thisChar.accumulatedCooldown = 0;
        FireNextBaseMeleeAttackMove();
    }

    public void FireNextBaseMeleeAttackMove() {
        if (currentMeleeAttackIndex < 0) { return; }
        if (currentMeleeAttackIndex >= baseMeleeAttackMoves.Count) { anim.Stop();  currentMeleeAttackIndex = 0; thisChar.CombatIdle(); }
        currentMeleeMove = baseMeleeAttackMoves[currentMeleeAttackIndex++];
        anim.Play(currentMeleeMove.animation, .6f, FadeMode.FromStart).Events.OnEnd = () => ProcessBaseMeleeAttackCombo();
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown += currentMeleeMove.cooldown;
        currentMeleeMove.animation.State.Root.Component.Animator.applyRootMotion = true;
    }
    public void ProcessBaseMeleeAttackCombo() {
        if (currentMeleeAttackIndex < 0) { return; }
        if (currentMeleeAttackIndex >= baseMeleeAttackMoves.Count) {
            currentMeleeAttackIndex = 0;
            thisChar.CombatIdle();
        } else {
            FireNextBaseMeleeAttackMove();
        }
    }

    public void FireBaseRangedAttackMoves() {
        if (currentRangedAttackIndex > 0) { return; }
        currentRangedAttackIndex = 0;
        currentMeleeAttackIndex = -1;
        thisChar.accumulatedCooldown = 0;
        FireNextBaseRangedAttackMove();
    }

    public void FireNextBaseRangedAttackMove() {
        if (currentRangedAttackIndex < 0) { return; }
        if (currentRangedAttackIndex >= baseRangedAttackMoves.Count) { anim.Stop(); currentRangedAttackIndex = 0; thisChar.CombatIdle(); }
        currentRangedMove = baseRangedAttackMoves[currentRangedAttackIndex++];
        anim.Play(currentRangedMove.animation, .6f, FadeMode.FromStart).Events.OnEnd = () => ProcessBaseRangedAttackCombo();
        thisChar.lastAttack = Time.time;
        thisChar.accumulatedCooldown += currentRangedMove.cooldown;
        currentRangedMove.animation.State.Root.Component.Animator.applyRootMotion = true;
    }
    public void ProcessBaseRangedAttackCombo() {
        if (currentRangedAttackIndex < 0) { return; }
        if (currentRangedAttackIndex >= baseRangedAttackMoves.Count) {
            currentRangedAttackIndex = 0;
            thisChar.CombatIdle();
        }
        else {
            FireNextBaseRangedAttackMove();
        }
    }





    public void TakeDamage() {
        currentMeleeAttackIndex = -1;
        currentRangedAttackIndex = -1;
        anim.Play(takeDamage).Events.OnEnd = () => thisChar.CombatIdle();
    }

    void Update()
    {
        movement.State.Parameter = GetComponent<NavMeshAgent>().velocity.magnitude * (shouldBackpedal ? -1 : 1);
    }
}
