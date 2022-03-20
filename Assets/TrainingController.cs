using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingController : MonoBehaviour, UIElement
{
    public Training activeTraining;
    public Character activeChar;
    
    public TextMeshProUGUI heroName, heroType, shooting, passing, tackling, carrying, melee, ranged, magic, speed, dex, agi, str;
    public TextMeshProUGUI trainingText;
    public TextMeshProUGUI knownMoves;
    public StableTrainingScrollerController scroller;
    public void SetTraining() {

    }
    public void Init(Character _activeChar) {
        activeChar = _activeChar;
        UpdateUI();
    }
    public void UpdateUI() {
        
        string known = "";
        if (activeChar != null) {
            heroName.text = activeChar.name;
            heroType.text = activeChar.archetype.ToString();

            shooting.text = activeChar.shooting.ToString();
            passing.text = activeChar.passing.ToString();
            tackling.text = activeChar.tackling.ToString();
            carrying.text = activeChar.carrying.ToString();
            melee.text = activeChar.melee.ToString();
            ranged.text = activeChar.ranged.ToString();
            magic.text = activeChar.magic.ToString();
            speed.text = activeChar.runspeed.ToString();
            dex.text = activeChar.dexterity.ToString();
            agi.text = activeChar.agility.ToString();
            str.text = activeChar.strength.ToString();
            foreach (Move m in activeChar.knownMoves) {
                known += m.name + "\n";
            }
        }
        knownMoves.text = known;
        string activeTrainingText = "";
        trainingText.text = "";
        
        if (activeTraining != null) {
            activeTrainingText = activeTraining.training.Title();
        }
        
        if (activeChar != null && activeTraining != null) {

            if (activeTraining.moveToTrain != null) { trainingText.text += "\nSkill to Train: " + activeTraining.moveToTrain.name.Title();
                trainingText.text += "\nTotal Cost: " + activeTraining.cost;
                if (activeChar.HasMove(activeTraining.moveToTrain.name)) {
                    trainingText.text = "Hero already has this skill.";
                }
            }
            else {
                
                trainingText.text += "\nCurrent " + activeTrainingText + " Ability: " + activeChar.GetCharacterAttributeValue(activeTraining.training);
                trainingText.text += "\nTotal Cost: " + (activeChar.GetCharacterAttributeValue(activeTraining.training) + activeTraining.amount) * activeTraining.cost;
            }
        }
        if (activeChar != null && activeChar.currentTraining.training != "None") {
            trainingText.text = "Selected Hero is Already Training";
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
            cost = activeChar.GetCharacterAttributeValue(activeTraining.training) + activeTraining.amount;
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
        f.AddExpense(cost, LedgerAccount.Personnel, "Training: " + activeTraining.training + ", " + activeChar.name, "Training");
        activeChar.StartTraining(Instantiate<Training>(activeTraining));
        activeTraining = null;
        FindObjectOfType<StableTrainingScrollerController>().ClearActives();
        //FindObjectOfType<HeroScrollerController>().ClearActives();
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
