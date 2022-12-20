using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using UnityEngine.AI;

public class AnimancerController : MonoBehaviour {
    public LinearMixerTransitionAsset.UnShared movement;
    public LinearMixerTransitionAsset.UnShared injuredMovement;
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
    public ClipTransition shoulderBarge;
    public ClipTransition backstab;
    public ClipTransition backstabVictim;
    public ClipTransition[] skills;
    public ClipTransition flameCircle;
    public ClipTransition powerSlam;
    public ClipTransition flechettes;
    public ClipTransition summon;
    public ClipTransition gkSwat;
    public ClipTransition runForward;
    public ClipTransition jumpCatch;
    public ClipTransition assassinate;
    public ClipTransition kneecap;
    public ClipTransition getKneecapped;
    public ClipTransition uncannyDodge;
    public ClipTransition bolaThrow;
    public ClipTransition pistolShot;
    public ClipTransition viciousMockery;
    public List<ClipTransition> swordFlurry;
    public ClipTransition bullRush;
    public ClipTransition soulSteal;
    public ClipTransition execute;
    public ClipTransition executeVictim;
    public ClipTransition rallyingCry;
    public ClipTransition divineIntervention;
    public ClipTransition seated;
    public ClipTransition singleHeal;
    
    public List<Move> baseMeleeAttackMoves;
    public List<Move> baseRangedAttackMoves;
    
    NavMeshAgent agent;
    StableCombatChar thisChar;
    public bool shouldBackpedal;
    public enum SkillAnims { Hunt = 0, NavigateLand = 0 };

    public bool injured;

    private void Awake() {
        Debug.Log("Init AnimSet");
        animSet.Init(this);
    }
    void Start() {
        thisChar = GetComponent<StableCombatChar>();
        anim = GetComponent<AnimancerComponent>();
        agent = GetComponent<NavMeshAgent>();
        injured = false;
        if (thisChar != null) {
            baseMeleeAttackMoves = thisChar.meleeAttackMoves;
            baseRangedAttackMoves = thisChar.rangedAttackMoves;
            anim.Play(failStrip);
        }
        Idle();
    }

    public void Idle() {
        if (injured) {
            anim.Play(injuredMovement);
        }
        else {
            anim.Play(movement);
        }
        currentMeleeAttackIndex = -1;
        currentRangedAttackIndex = -1;
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
    public void FaceSmash() {
        anim.Play(passBall, .25f, FadeMode.FromStart).Events.OnEnd = () => Idle();
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
    public void SoulSteal() {
        anim.Play(soulSteal, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
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
    public void PistolShot() {
        anim.Play(pistolShot, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
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
        //anim.Play(runForward, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
        thisChar.Idle();
        
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
    public void ShoulderBarge() {
        anim.Play(shoulderBarge, .25f, FadeMode.FromStart).Events.OnEnd = () => anim.Play(shoulderBarge, .25f, FadeMode.FromStart).Events.OnEnd = () => anim.Play(shoulderBarge, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.CombatIdle();
        shoulderBarge.State.Root.Component.Animator.applyRootMotion = true;
    }
    public void ViciousMockery() {
        anim.Play(viciousMockery, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void UncannyDodge() {
        anim.Play(uncannyDodge, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.PistolShot();
        uncannyDodge.State.Root.Component.Animator.applyRootMotion = true;
        uncannyDodge.Speed = .7f;
    }
    public void BolaThrow() {
        anim.Play(bolaThrow, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void Backstab() {
        anim.Play(backstab, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void BackstabVictim() {
        anim.Play(backstabVictim, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
   
    public void Assassinate() {
        anim.Play(assassinate, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void FlameCircle() {
        anim.Play(flameCircle, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void PowerSlam() {
        anim.Play(powerSlam, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void Flechettes() {
        anim.Play(flechettes, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void Summon() {
        anim.Play(summon, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void Kneecap() {
        anim.Play(kneecap, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void GetKneecapped() {
        injured = true;
        anim.Play(getKneecapped, .25f, FadeMode.FromStart).Events.OnEnd = () => Knockdown();
    }
    public void SingleHeal() {
        anim.Play(singleHeal, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void Seated() {
        anim.Play(seated, .25f, FadeMode.FromStart);
    }

    public void JumpCatch() {
        Debug.Log("#BallHawk#JumpCatchAnim");
        anim.Play(jumpCatch, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }

    public void SwordFlurry(int index=0) {
        currentMeleeMove = new Move() { healthDamage = 1, staminaDamage = 20, mindDamage = 20, balanceDamage = 20 };
        if (index >= swordFlurry.Count) {
            Idle();
            StartCoroutine(DelayIdle());
            return;
        }
        anim.Play(swordFlurry[index++], .25f, FadeMode.FromStart).Events.OnEnd = () => SwordFlurry(index);
    }
    public void Execute() {
        anim.Play(execute, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void ExecuteVictim() {
        anim.Play(executeVictim, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void DivineIntervention() {
        anim.Play(divineIntervention, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
    }
    public void GKBored() {

    }
    public void GKSwat() {
        gkSwat.Speed = 1.7f;
        anim.Play(gkSwat, .25f, FadeMode.FromStart).Events.OnEnd = () => thisChar.Idle();
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
        anim.Play(currentMeleeMove.animation, .2f, FadeMode.FromStart).Events.OnEnd = () => ProcessBaseMeleeAttackCombo();
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





    public void TakeDamage(bool idleOnEnd = true) {
        currentMeleeAttackIndex = -1;
        currentRangedAttackIndex = -1;
        anim.Play(takeDamage).Events.OnEnd = () => { if (idleOnEnd) { thisChar.CombatIdle(); } else { Idle(); } };
    }
    IEnumerator DelayIdle() {
        yield return new WaitForSeconds(1.0f);
        thisChar.Idle();
    }

    void Update()
    {
        if (injured) {
            injuredMovement.State.Parameter = GetComponent<NavMeshAgent>().velocity.magnitude * (shouldBackpedal ? -1 : 1);
        } else {
            movement.State.Parameter = GetComponent<NavMeshAgent>().velocity.magnitude * (shouldBackpedal ? -1 : 1);
        }
        
    }
}
