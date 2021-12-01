using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MissionCharacterStateNegotiate : MissionCharacterState {
    float nextNumCheck = Mathf.Infinity;
    
    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public Transform negotiateTarget;
    bool complete;
    public Transform walkTarget;
    public bool seated;

    public int finalAttitude;
    public string playerIntro, NPCIntro;
    public List<string> dialogue = new List<string>(); // always start with the NPC
    public List<int> attitudes = new List<int>();
    public int maxRounds = 4; //set max rounds based on difficulty, default should be 6
    
    public override void EnterFrom(MissionCharacterState state) {
        base.EnterFrom(state);
        nextNumCheck = Mathf.Infinity;
        step = poi.step;
        nextNumCheck = Time.time + Mathf.Infinity;
        anim.SetBool("Walk", true);
        anim.SetBool("Run", false);
        walkTarget = poi.allPurposeTransforms[0];
        NegotiateDialogue loadDialogue = GameObject.Instantiate<NegotiateDialogue>(Resources.Load<NegotiateDialogue>(poi.allPurposeStrings[0]));
        npcNegative = loadDialogue.npcNegative;
        npcPositive = loadDialogue.npcPositive;
        playerNegative = loadDialogue.playerNegative;
        playerPositive = loadDialogue.playerPositive;
        playerIntro = loadDialogue.playerIntro;
        NPCIntro = loadDialogue.npcIntro;
        maxRounds = npcNegative.Length;
        GameObject.FindObjectOfType<MissionController>().SetAllHeroesDontAct(thisChar);
        agent.SetDestination(walkTarget.position + new Vector3(0, 0, 0));
        didFireAction = false;
        NegotiationResult();
        Helper.Cam().SetTarget(poi.allPurposeTransforms[2]);
    }

    void FireAction() {
        if (didFireAction) { return; }
        Debug.Log("Fire Negotiate Action");
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
            NegotiateEnd();
            nextNumCheck = Mathf.Infinity;
            return;
        }
        Helper.Speech(thisChar.transform, dialogue[speechIndex++], 5f);
        Helper.Speech(negotiateTarget, dialogue[speechIndex++], 8f);
        ProcessBuzz();
        //Helper.UIUpdate("Current Attitude: " + attitudes[attitudeIndex++]);
        nextNumCheck = Time. time + 12f;
        
    }

    public override void WillExit() {
        base.WillExit();
        thisChar.ControlCam(false, 5f);
    }



    public void NegotiateEnd() {
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
        Debug.Log("Is Final? " + poi.isFinalForMission+ "  "+finalAttitude);
        if (poi.isFinalForMission) {
            MissionFinalDetails details = GameObject.FindObjectOfType<MissionFinalDetails>();
            float tempMod = 1;
            if (finalAttitude >= 50) { tempMod = 1.1f; }
            if (finalAttitude >= 75) { tempMod = 1.25f; }
            if (finalAttitude >= 100) { tempMod = 1.50f; }
            if (tempMod > 1) {
                details.narrative.Add(thisChar.character.name + " obtained a "+ (int)((tempMod-1)*100) +"% bonus ");
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

    public void NegotiationResult() {
        dialogue = new List<string>();
        bool npcTalking = true;
        int attitude = 0; //made modifications if there is an initital bonus;
        maxRounds = playerPositive.Length;
        dialogue.Add(playerIntro);
        dialogue.Add(NPCIntro);
        attitudes.Add(attitude);
        for (int x = 0; x < maxRounds; x++) {
            //need a list of NPC statements and another list of Player negotiating statements
            npcTalking = !npcTalking;
            float comp = GetPlayerScore(thisChar.character.negotiating);
            //Helper.UIUpdate("Roll: " + comp + " Needed: " + threshold);
            
            if (comp >= threshold) {
                dialogue.Add(playerPositive[x]);
                dialogue.Add(npcPositive[x]);
                attitude += 25;
            }
            else {
                attitude -= 25;
                dialogue.Add(playerNegative[x]);
                dialogue.Add(npcNegative[x]);
            }
            attitudes.Add(attitude);
        }
        finalAttitude = attitude;
        success = (attitude > 0);
    }
    public string[] npcPositive = { "I'm listening.", "Without respecting authority, we have nothing but chaos.", "Profits are good. I think we can work something out.", "I don't see why not! Let's drink to this new endeavor!" };
    public string[] npcNegative = { "No1.", "No2.", "No3.", "No4." };
    public string[] playerPositive = { "You seem like a man who knows how to conduct business.", "My stablemaster knows how important it is to ensure that...authority...is properly acknowledged.", "If your eminence would see fit to approve our proposal, I know we would have a...profitable...relationship.", "In light of our friendship, I'm sure my stablemaster would appreciate waiver of the market tariff."};
    public string[] playerNegative = { "Ahem...Ah...We are here to inquire about the possibility of potentially buying a stall in your marketplace.","Bad2.","Bad3.", "Bad4." };
}