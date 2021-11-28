using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingController : MonoBehaviour, UIElement
{
    public Training activeTraining;
    public Character activeChar;
    public Text active;
    public void SetTraining() {

    }
    
    public void UpdateUI() {
        string activeTrainingText = "";
        active.text = "";
        if (activeTraining != null) {
            activeTrainingText = activeTraining.training.Title();
        }
        if (activeChar != null) {
            active.text = activeChar.myName;
        }
        if (activeChar != null && activeTraining != null) {

            if (activeTraining.moveToTrain != null) { active.text += "\nSkill to Train: " + activeTraining.moveToTrain.name.Title();
                active.text += "\nTotal Cost: " + activeTraining.cost;
                if (activeChar.HasMove(activeTraining.moveToTrain.name)) {
                    active.text = "Hero already has this skill.";
                }
            }
            else {
                active.text += "\nSkill to Train: " + activeTrainingText;
                active.text += "\nCurrent " + activeTrainingText + " Ability: " + activeChar.GetCharacterttributeValue(activeTraining.training);
                active.text += "\nTotal Cost: " + (activeChar.GetCharacterttributeValue(activeTraining.training) + 1) * activeTraining.cost;
            }
        }
        if (activeChar != null && activeChar.currentTraining.training != "None") {
            active.text = "Selected Hero is Already Training";
        }
        
        
    }
    public void SetActiveChar(Character c) {
        activeChar = c;
        UpdateUI();
    }    

    public void SetActiveTraining (Training t) {
        activeTraining = t;
        UpdateUI();
    }

    public void BeginTraining() {
        Finance f = Game.instance.playerStable.finance;
        int cost = 0;
        if (activeTraining.moveToTrain != null) {
            cost = activeTraining.cost;
            if (activeChar.HasMove(activeTraining.name)) {
                print("Character already has Move");
                return;
            }
        } else {
            cost = activeChar.GetCharacterttributeValue(activeTraining.training) + 1;
            cost *= activeTraining.cost;
        }
        
        print("Cost of training is: " + cost);
        if (f.gold < cost) {
            print("Not enough gold.");
            return;
        }
        if (activeChar.currentTraining.training != "None" || activeChar.currentTraining.moveToTrain != null) {
            print("Character is already training");
            return;
        }
        f.AddExpense(cost, LedgerAccount.Personnel, "Training: " + activeTraining.training + ", " + activeChar.myName, "Training");
        activeChar.StartTraining(Instantiate<Training>(activeTraining));
        activeChar = null;
        activeTraining = null;
        FindObjectOfType<StableTrainingScrollerController>().ClearActives();
        FindObjectOfType<HeroScrollerController>().ClearActives();
        FindObjectOfType<StableManagementController>().UpdateHeader();
        UpdateUI();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnable() {
        UpdateUI();
    }

    public void UpdateOnAdvance() {
        UpdateUI();
    }
}
