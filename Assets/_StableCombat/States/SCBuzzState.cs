using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCBuzzState : SCSkillState {

    float nextNumCheck = Mathf.Infinity;

    public Step step;
    bool success;
    float fireActionTime;
    bool didFireAction;
    public Transform skillTarget;
    bool complete;
    public Transform walkTarget;
    public bool seated;

    public int finalScore;
    public string playerIntro, NPCIntro;
    public List<string> dialogue = new List<string>(); // always start with the NPC
    
    public int maxRounds = 5; //set max rounds based on difficulty, default should be 6

    public string[] npcPositive = { "I'm listening.", "Without respecting authority, we have nothing but chaos.", "Profits are good. I think we can work something out.", "I don't see why not! Let's drink to this new endeavor!" };
    public string[] npcNegative = { "No1.", "No2.", "No3.", "No4." };
    public string[] playerPositive = { "You seem like a man who knows how to conduct business.", "My stablemaster knows how important it is to ensure that...authority...is properly acknowledged.", "If your eminence would see fit to approve our proposal, I know we would have a...profitable...relationship.", "In light of our friendship, I'm sure my stablemaster would appreciate waiver of the market tariff." };
    public string[] playerNegative = { "Ahem...Ah...We are here to inquire about the possibility of potentially buying a stall in your marketplace.", "Bad2.", "Bad3.", "Bad4." };

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        nextNumCheck = Mathf.Infinity;
        maxRounds = poi.step.challengeNum;
        step = poi.step;
        nextNumCheck = Time.time + Mathf.Infinity;
        thisChar.anima.Idle();
        thisChar.agent.isStopped = false;
        walkTarget = poi.allPurposeTransforms[0];

        npcNegative = new string[] { "[QUERY OPENAI] NPCDialogue1", "[QUERY OPENAI] NPCDialogue2", "[QUERY OPENAI] NPCDialogue3", "[QUERY OPENAI] NPCDialogue4", "[QUERY OPENAI] NPCDialogue5" };
        npcPositive = new string[] { "[QUERY OPENAI] NPCDialogue1", "[QUERY OPENAI] NPCDialogue2", "[QUERY OPENAI] NPCDialogue3", "[QUERY OPENAI] NPCDialogue4", "[QUERY OPENAI] NPCDialogue5" };
        playerNegative = new string[] { "[QUERY OPENAI] Dialogue1", "[QUERY OPENAI] Dialogue2", "[QUERY OPENAI] Dialogue3", "[QUERY OPENAI] Dialogue4", "[QUERY OPENAI] Dialogue5" };
        playerPositive = new string[] { "[QUERY OPENAI] Dialogue1", "[QUERY OPENAI] Dialogue2", "[QUERY OPENAI] Dialogue3", "[QUERY OPENAI] Dialogue4", "[QUERY OPENAI] Dialogue5" };
        playerIntro = "Hello";
        NPCIntro = "Hi";
        if (maxRounds > npcNegative.Length || maxRounds <= 0) {
            maxRounds = npcNegative.Length;
        }

        ///GameObject.FindObjectOfType<MissionController>().SetAllHeroesDontAct(thisChar);
        skillTarget = poi.allPurposeTransforms[1];
        thisChar.agent.SetDestination(walkTarget.position + new Vector3(0, 0, 0));
        didFireAction = false;
        SkillResult();
        Helper.Cam().SetTarget(poi.allPurposeTransforms[2]);
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
    }
    void FireAction() {
        if (didFireAction) { return; }
        didFireAction = true;
        thisChar.agent.transform.rotation = poi.targetPos.rotation;
        thisChar.agent.isStopped = true;
        fireActionTime = Mathf.Infinity;
        FireAction();
        Debug.Log("#TODO#Add Custom Skill Action Here");
        thisChar.anima.Idle();
        //Arrow
        //thisChar.ControlCam();

        nextNumCheck = Time.time + 2.0f;

    }
    bool finalSkipped;
    public override void Update() {
        base.Update();
        //Debug.Log(nextNumCheck + " : " + Time.time);
        if (!didFireAction && Vector3.Distance(thisChar.transform.position, walkTarget.position) < 1) { FireAction(); }
        if (Input.GetKeyDown(KeyCode.Space)) {
            nextNumCheck = 0;
            if (!finalSkipped && speechIndex >= dialogue.Count) {
                finalSkipped = true;
                nextNumCheck = Time.time + 6;
            }
        }
        if (Time.time >= nextNumCheck) {
            nextNumCheck = Mathf.Infinity;
            NextDialogue();
        }
    }
    int speechIndex = 0;
    int attitudeIndex = 0;

    void NextDialogue() {
        if (speechIndex >= dialogue.Count) {
            InteractionEnd();
            nextNumCheck = Mathf.Infinity;
            return;
        }
        Helper.Speech(thisChar.transform, dialogue[speechIndex++], 5f);
        Helper.Speech(skillTarget, dialogue[speechIndex++], 8f);
        ProcessBuzz();
        //Helper.UIUpdate("Current Attitude: " + attitudes[attitudeIndex++]);
        nextNumCheck = Time.time + 10f;

    }

    public override void WillExit() {
        base.WillExit();
    }

    public void InteractionEnd() {
        thisChar.Idle();
        nextNumCheck = Mathf.Infinity;
        Debug.Log("Is Final? " + poi.isFinalForMission + "  " + finalScore);
        if (poi.isFinalForMission) {
            MissionFinalDetails details = GameObject.FindObjectOfType<MissionFinalDetails>();
            float tempMod = 1;
            Debug.Log("#Mission#Final Score: " + finalScore);
            if (finalScore >= 30) { tempMod = 1.1f; }
            if (finalScore >= 40) { tempMod = 1.25f; }
            if (finalScore >= 50) { tempMod = 1.50f; }
            if (tempMod > 1) {
                details.narrative.Add(thisChar.myCharacter.name + " obtained a " + (int)((tempMod - 1) * 100) + "% bonus ");
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
    public void SkillResult() {
        dialogue = new List<string>();
        bool npcTalking = true;
        int attitude = 0; //made modifications if there is an initital bonus;
        //maxRounds = playerPositive.Length;
        dialogue.Add(playerIntro);
        dialogue.Add(NPCIntro);
        if (poi.step.level == 0) {
            threshold = 0;
        }
        else {
            threshold = (poi.step.level * 4) + skillArray[poi.step.level - 1];
        }
        for (int x = 0; x < maxRounds; x++) {
            //need a list of NPC statements and another list of Player negotiating statements
            npcTalking = !npcTalking;
            float comp = GetPlayerScore(poi.attempter.trait.level);
            playerScore.Add(new Roll() { total = comp, max = 40, threshold = threshold });
            otherScore.Add(new Roll() { total = threshold, max = 40, threshold = threshold });
            //Helper.UIUpdate("Roll: " + comp + " Needed: " + threshold);

            if (comp >= threshold) {
                dialogue.Add(playerPositive[x]);
                dialogue.Add(npcPositive[x]);
                attitude += 10;
            }
            else {
                attitude -= 10;
                dialogue.Add(playerNegative[x]);
                dialogue.Add(npcNegative[x]);
            }
            if (comp >= 100) {
                attitude += 10;
            }
            attitudes.Add(attitude);
        }
        finalScore = attitude;
        success = (attitude > 0);
    }

}
