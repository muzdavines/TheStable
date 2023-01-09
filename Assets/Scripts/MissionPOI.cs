using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPOI : MonoBehaviour
{

    public Collider col;
    public MissionController control;
    public Transform targetPos;
    [SerializeField]
    public Step step;
    public AudioClip onSuccessSound;
    // Start is called before the first frame update
    public Transform[] allPurposeTransforms;
    public string[] allPurposeStrings;
    public bool isFinalForMission;
    public Vector3 cameraAngle = Vector3.zero;
    public AudioClip backgroundMusic;
    public string attributePrereq;
    public int levelReq;
    public string successText, failText;
  
    void Start()
    {
        control = GameObject.FindObjectOfType<MissionController>();
        col = GetComponent<Collider>();
       
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void OnTriggerEnter(Collider other) {

        //proximity triggered, move Character to main target pos

        if (other.CompareTag("Character")) {
            col.enabled = false;
            control.POITriggered(this, other);
        }
        
    }

    /// <summary>
    /// //TODO - create a base Step class for MissionChar state machine that waits for available
    /// teammates before starting the task
    /// </summary>
   
    public StableCombatChar currentCharacterToAttempt;
    public Attempter attempter;
    public class Attempter {
        public StableCombatChar thisChar;
        public Trait trait;
    }
    public virtual void StepActivated(StableCombatChar activeChar) {
        //POI activated, start task and broadcast to teammates to move here
        //Start task - call step type on Character, let the state machine handle it
        //call teammates, check whether they are engaged

        //Calculate the best character to use

        List<StableCombatChar> heroes = control.allChars;
        attempter = step.CharacterToAttempt(heroes);
        currentCharacterToAttempt = attempter.thisChar;
        levelReq = step.level;
        foreach (StableCombatChar c in heroes) {
            if (c != currentCharacterToAttempt) { c.MissionIdleDontAct(); }
        }
        control.currentActiveStepChar = currentCharacterToAttempt;
        if (!step.required && attempter.trait.level < levelReq) {
            print("Does not meet minimum Req");
            Avoid(true);
        }
        else {
            Debug.Log("#Mission#Step Activated " + attempter.thisChar.myCharacter.name);
            currentCharacterToAttempt.ActivateStep(this, attempter.trait);
        }
        
        MissionHelper help = GetComponent<MissionHelper>();
        if (help != null) { help.Activate(); }

    }

    public virtual void Resolve(bool success) {
        control.buzz.Reset();
        print("Resolve " + step.type + "  " + success);
        if (success) {
            print ("Sucess!");
            GetComponent<OnStepSuccess>()?.OnSuccess();
            GetComponent<SkillStepResolution>()?.OnSuccess(successText);
            Helper.PlayOneShot(onSuccessSound);
        } else {
            print("Fail. Step Required: " + step.required);
            GetComponent<OnStepFail>()?.OnFail();
            GetComponent<SkillStepResolution>()?.OnFailed(successText);
            if (step.required) {
                control.MissionFailed();
            }
        }
        
        control.RemovePOI(this);
    }

    public virtual void Resolve(int quality) {
        control.buzz.Reset();
        print("Sucess!");
        GetComponent<OnStepSuccess>().OnSuccess(quality);
        Helper.PlayOneShot(onSuccessSound);
        control.RemovePOI(this);
    }
    public virtual void Avoid(bool minReqNotMet = true) {
        control.buzz.Reset();
        print("Avoiding");
        GetComponent<SkillStepResolution>().OnAvoid(minReqNotMet);
        Helper.PlayOneShot(onSuccessSound);
        control.RemovePOI(this);
    }
}
