using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSwordFlurry: StableCombatCharState, CannotSpecial, CannotTarget {

    public StableCombatChar victim;
    private bool shouldFaceTarget;
    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        if (victim == null || victim.isKnockedDown) {
            thisChar.Idle();
                return;
        }

        thisChar.agent.TotalStop();
        victim.SwordFlurryVictim(thisChar);
        thisChar._t.LookAt(victim.position);
        thisChar.RHMWeapon.gameObject.SetActive(true);
        thisChar.matchController.AddAnnouncerLine(thisChar.myCharacter.name + " wants to duel " +
                                                  victim.myCharacter.name + "!");
        didShowHighlight = HighlightCamera.instance.ShowHighlight(thisChar, 10f);
    }

    private bool attackFired = false;
    IEnumerator DelayIdle() {
        yield return new WaitForSeconds(1.5f);
        thisChar.Idle();
        thisChar.RHMWeapon.gameObject.SetActive(false);
    }
    bool notActive;
    public override void Update() {
        if (notActive) {
            return;
        }
        base.Update();
        if (victim == null || victim.isKnockedDown) {
            notActive = true;
            thisChar.StartCoroutine(DelayIdle());
            return;
        }
        if (thisChar.Distance(victim) > 1.5f) {
            thisChar.agent.SetDestination(victim.position);
            thisChar.agent.isStopped = false;
        }
        else {
            thisChar.agent.TotalStop();
            if (!attackFired) {
                attackFired = true;
                thisChar.anima.SwordFlurry();
            }
        }
        if (shouldFaceTarget && victim != null) {
            thisChar._t.LookAt(victim.position);
        }
        
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        thisChar.MeleeScanDamage(message);
        Debug.Log("#SwordFlurry#Message: "+message);
        
        if (message == "BeginFaceTarget") {
            string s = attackTalk[Random.Range(0,attackTalk.Length)];
           s= s.Replace("XYZ",thisChar.myCharacter.name);
           s= s.Replace("ABC",victim.myCharacter.name);
            thisChar.matchController.AddAnnouncerLine(s);
            shouldFaceTarget = true;
        }
        if (message == "EndFaceTarget") {
            shouldFaceTarget = false;
        }
    }

    public override void WillExit() {
        base.WillExit();
        if (didShowHighlight) {
            HighlightCamera.instance.Idle();
        }

        thisChar.RHMWeapon.gameObject.SetActive(false);
    }

    private readonly string[] attackTalk = {
        "XYZ is on the attack, and ABC is feeling the heat!",
        "XYZ is relentless in their assault, and ABC is struggling to keep up!",
        "XYZ is looking dominant so far, can ABC turn things around?",
        "XYZ is in control of this fight, but ABC is not giving up!",
        "ABC is hanging in there, but XYZ is starting to wear them down!",
        "This could be the end for ABC, as XYZ continues to pour on the pressure!",
        "XYZ is not letting up, and ABC is looking increasingly fatigued!",
        "Can ABC muster up one last push, or will XYZ finish them off?",
        "This could be it for ABC, as XYZ looks poised to deliver the final blow!",
        "XYZ is about to deliver the finishing blow to ABC...!",
        "ABC is down but not out, can they make a comeback?",
        "XYZ is in striking distance of victory, but ABC is not giving up!",
        "This is anyone's fight now, as both warriors are giving it their all!",
        "ABC is really feeling the heat now, as XYZ continues to press the advantage!",
        "XYZ is looking to finish this fight once and for all, can ABC hold on?",
        "ABC is on the ropes and struggling to keep up with XYZ's onslaught!",
        "This could be the end for ABC, as they seem to be running out of steam!",
        "XYZ is not letting up, and ABC is starting to look overwhelmed!",
        "Can ABC make a comeback, or will XYZ finish them off?",
        "This could be it for ABC, as XYZ looks poised to deliver the final blow!",
        "XYZ is about to deliver the finishing blow to ABC...!",
        
    };
}
