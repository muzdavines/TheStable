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
    void Start()
    {
        control = GameObject.FindObjectOfType<MissionController>();
        col = GetComponent<Collider>();
        step.mod = 1;
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
   
    public MissionCharacter currentCharacterToAttempt;
    public virtual void StepActivated(MissionCharacter activeChar) {
        //POI activated, start task and broadcast to teammates to move here
        //Start task - call step type on Character, let the state machine handle it
        //call teammates, check whether they are engaged

        //Calculate the best character to use

        List<Character> heroes = control.heroes;
        MissionCharacter characterToAttempt = step.CharacterToAttempt(heroes).currentMissionCharacter;
        currentCharacterToAttempt = characterToAttempt;
        foreach (Character c in heroes) {
            if (c.currentMissionCharacter != characterToAttempt) { c.currentMissionCharacter.IdleDontAct(); }
        }
        print(characterToAttempt.name);
        control.currentActiveStepChar = characterToAttempt.character;
        characterToAttempt.ActivateStep(this);
        
        MissionHelper help = GetComponent<MissionHelper>();
        if (help != null) { help.Activate(); }

    }

    public virtual void Resolve(bool success) {
        print("Resolve " + step.type + "  " + success);
        if (success) {
            print ("Sucess!");
            GetComponent<OnStepSuccess>().OnSuccess();
            Helper.PlayOneShot(onSuccessSound);
            control.RemovePOI(this);
        } else {
            print("Fail. Step Required: " + step.required);
            GetComponent<OnStepFail>().OnFail();
            if (step.required) {
                control.MissionFailed();
            }
            control.RemovePOI(this);
        }
    }

    public virtual void Resolve(int quality) {
        print("Sucess!");
        GetComponent<OnStepSuccess>().OnSuccess(quality);
        Helper.PlayOneShot(onSuccessSound);
        control.RemovePOI(this);
    }
}
