using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MissionCharacterStateGamble : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public Transform gambleTarget;
    bool complete;
    public Transform walkTarget;
    public bool seated;

    public int finalAttitude;
    public string playerIntro, NPCIntro;
    public List<string> dialogue = new List<string>(); // always start with the NPC
    public List<int> attitudes = new List<int>();
    public int maxRounds = 7; //set max rounds based on difficulty, default should be 6

    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        nextNumCheck = Mathf.Infinity;
        step = poi.step;
        nextNumCheck = Time.time + Mathf.Infinity;
        anim.SetBool("Walk", true);
        anim.SetBool("Run", false);
        walkTarget = poi.allPurposeTransforms[0];
        
        playerIntro = "I've been known to win a game of Brimley or two.";
        NPCIntro = "Oi, guv'nor, how about we play?";
        
        GameObject.FindObjectOfType<MissionController>().SetAllHeroesDontAct(thisChar);
        agent.SetDestination(walkTarget.position + new Vector3(0, 0, 0));
        didFireAction = false;
        GambleResult();
        Debug.Log(dialogue.Count + " entries for Gambling Dialogue");
        
    }

    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Gamble Action");
        didFireAction = true;
        agent.transform.rotation = poi.targetPos.rotation;
        agent.isStopped = true;
        fireActionTime = Mathf.Infinity;
        FireAction();
        anim.SetBool("Walk", false);
        anim.SetFloat("ActionType", (int)step.type);
        anim.SetTrigger("Action");
        //Arrow
        //thisChar.ControlCam();

        nextNumCheck = Time.time + 4.0f;

    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        switch (message) {
            case "":
                break;
        }
    }

    public override void Update() {
        base.Update();
        //Debug.Log(nextNumCheck + " : " + Time.time);
        if (!didFireAction && Vector3.Distance(thisChar.transform.position, walkTarget.position) < 1) { FireAction(); }

        if (Time.time >= nextNumCheck) {
            nextNumCheck = Mathf.Infinity;
            NextDialogue();
        }
    }
    int speechIndex = 0;
    int attitudeIndex = 0;
    void NextDialogue() {
        if (speechIndex >= dialogue.Count) {
            GambleEnd();
            nextNumCheck = Mathf.Infinity;
            return;
        }
        ProcessBuzz();
        Helper.Speech(thisChar.transform, dialogue[speechIndex++], 4f);
        Helper.Speech(gambleTarget, dialogue[speechIndex++], 8f);
        //Helper.UIUpdate("Current Attitude: " + attitudes[attitudeIndex++]);
        nextNumCheck = Time.time + 12f;
    }

    public override void WillExit() {
        base.WillExit();
        Debug.Log("GambleExiting, cam should revert");
        thisChar.ControlCam(false, 5f);
    }



    public void GambleEnd() {
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
        Debug.Log("Is Final? " + poi.isFinalForMission + "  " + finalAttitude);
        if (poi.isFinalForMission) {
            MissionFinalDetails details = GameObject.FindObjectOfType<MissionFinalDetails>();
            float tempMod = 1;
            if (finalAttitude >= 50) { tempMod = 1.1f; }
            if (finalAttitude >= 75) { tempMod = 1.25f; }
            if (finalAttitude >= 100) { tempMod = 1.50f; }
            if (tempMod > 1) {
                details.narrative.Add(thisChar.character.name + " obtained a " + (int)((tempMod - 1) * 100) + "% bonus ");
            }
            MissionFinalDetails finalDetails = GameObject.FindObjectOfType<MissionFinalDetails>();
            finalDetails.finalMod = tempMod;
            finalDetails.successful = success;
            if (success) {
                finalDetails.businessReward = poi.control.contract.businessReward;
            }

        }
        poi.Resolve(success);
    }
   
    public void GambleResult() {
        //Brimley
        int playerRounds = 0;
        int NPCRounds = 0;
        int playerAbil = thisChar.character.gambling;
        int NPCAbil = poi.step.level;

        dialogue = new List<string>();
        dialogue.Add(playerIntro);
        dialogue.Add(NPCIntro);

        for (int round = 0; round < 7; round++) {
            float playerDiceRoll = Random.Range(1, playerAbil + 1);
            float playerRoll = playerDiceRoll * (1.1f + step.mod);
            float NPCRoll = Random.Range(1, NPCAbil+1);
            
            playerScore.Add(new Roll() { total = playerRoll, mod = 1.1f + step.mod, diceRoll = playerDiceRoll, max = playerAbil > NPCAbil ? playerAbil * 2 : NPCAbil * 2, threshold = NPCRoll});
            otherScore.Add(new Roll() { total = NPCRoll, max = playerAbil > NPCAbil ? playerAbil * 2 : NPCAbil * 2, threshold = NPCRoll });
            //Helper.UIUpdate("Players: " + playerRoll.ToString("F2") + " NPC: "+NPCRoll.ToString("F2"));
            if (playerRoll >= NPCRoll) {
                //player wins round
                playerRounds++;
                dialogue.Add("I win this round!");
                dialogue.Add("Lucky roll.");
            } else {
                NPCRounds++;
                dialogue.Add("Dang it!");
                dialogue.Add("Round goes to me.");
            }
            Debug.Log("Player: " + playerRounds + " NPC: " + NPCRounds);
            if (playerRounds >= 4) {
                success = true;
                return;
            }
            if (NPCRounds >= 4) {
                success = false;
                return;
            }
        }
    }
    
    public string[] npcPositive = { "I'm listening.", "Without respecting authority, we have nothing but chaos.", "Profits are good. I think we can work something out.", "I don't see why not! Let's drink to this new endeavor!" };
    public string[] npcNegative = { "No1.", "No2.", "No3.", "No4." };
    public string[] playerPositive = { "You seem like a man who knows how to conduct business.", "My stablemaster knows how important it is to ensure that...authority...is properly acknowledged.", "If your eminence would see fit to approve our proposal, I know we would have a...profitable...relationship.", "In light of our friendship, I'm sure my stablemaster would appreciate waiver of the market tariff." };
    public string[] playerNegative = { "Ahem...Ah...We are here to inquire about the possibility of potentially buying a stall in your marketplace.", "Bad2.", "Bad3.", "Bad4." };
}