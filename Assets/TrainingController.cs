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
    public TextMeshProUGUI currentXP;
    public TextMeshProUGUI currentCostText;

    public TextMeshProUGUI currentShootingText,
        currentPassingText,
        currentTacklingText,
        currentCarryingText,
        currentStaminaText,
        currentMindText,
        currentBalanceText;

    public TextMeshProUGUI nextShootingCost, nextPassingCost, nextTacklingCost, nextCarryingCost;
    public int currentCost;
    public int currentShooting, currentPassing, currentTackling, currentCarrying;
    public void SetTraining() {

    }
    public void Init(Character _activeChar) {
        activeChar = _activeChar;
        currentShooting = 0;
        currentCost = 0;
        UpdateUI();
    }
    public void UpdateUI() {
        if (activeChar == null) {
            HeroMainController main = GetComponentInParent<HeroMainController>();
            if (main == null) {
                Debug.LogError("No active controller");
                return;
            }
            activeChar = main.activeChar;
            if (activeChar == null) {
                Debug.LogError("No Active Character");
            }
        }
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
        UpdateXP();
        currentShootingText.text = currentShooting > 0 ? currentShooting.ToString() : "";
        currentPassingText.text = currentPassing > 0 ? currentPassing.ToString() : "";
        currentTacklingText.text = currentTackling > 0 ? currentTackling.ToString() : "";
        currentCarryingText.text = currentCarrying > 0 ? currentCarrying.ToString() : "";
        nextShootingCost.text = "Next XP: "+(activeChar.shooting + currentShooting) * 10+"";
        nextPassingCost.text = "Next XP: " + (activeChar.passing + currentPassing) * 10 + "";
        nextTacklingCost.text = "Next XP: " + (activeChar.tackling + currentTackling) * 10 + "";
        nextCarryingCost.text = "Next XP: " + (activeChar.carrying + currentCarrying) * 10 + "";


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

    public void ConfirmTraining() {
        activeChar.shooting += currentShooting;
        currentShooting = 0;
        activeChar.carrying += currentCarrying;
        currentCarrying = 0;
        activeChar.passing += currentPassing;
        currentPassing = 0;
        activeChar.tackling += currentTackling;
        currentTackling = 0;

        activeChar.xp -= currentCost;
        currentCost = 0;
        UpdateUI();
        GetComponentInParent<HeroMainController>().UpdateUI();

    }

    public void IncreaseAmount(string attribute) {
        int thisCost = 0;
        switch (attribute) {
            case "shooting":
                thisCost = (activeChar.shooting + currentShooting) * 10;
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentShooting += 1;
                currentShootingText.text = currentShooting.ToString();
                break;
            case "passing":
                thisCost = (activeChar.passing + currentPassing) * 10;
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentPassing += 1;
                currentPassingText.text = currentPassing.ToString();
                break;
            case "tackling":
                thisCost = (activeChar.tackling + currentTackling) * 10;
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentTackling += 1;
                currentTacklingText.text = currentTackling.ToString();
                break;
            case "carrying":
                thisCost = (activeChar.carrying + currentCarrying) * 10;
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentCarrying += 1;
                currentCarryingText.text = currentCarrying.ToString();
                break;
        }
        currentCost += thisCost;
        UpdateUI();
    }

    public void DecreaseAmount(string attribute) {
        switch (attribute) {
            case "shooting":
                if (currentShooting <= 0) {
                    return;
                }
                currentShooting -= 1;
                currentCost -= (activeChar.shooting + currentShooting) * 10;
                currentShootingText.text = currentShooting.ToString();
                break;
            case "tackling":
                if (currentTackling<= 0) {
                    return;
                }
                currentTackling -= 1;
                currentCost -= (activeChar.tackling + currentTackling) * 10;
                currentTacklingText.text = currentTackling.ToString();
                break;
            case "passing":
                if (currentPassing <= 0) {
                    return;
                }
                currentPassing -= 1;
                currentCost -= (activeChar.passing + currentPassing) * 10;
                currentPassingText.text = currentPassing.ToString();
                break;
            case "carrying":
                if (currentCarrying <= 0) {
                    return;
                }
                currentCarrying -= 1;
                currentCost -= (activeChar.carrying + currentCarrying) * 10;
                currentCarryingText.text = currentCarrying.ToString();
                break;
        }
        UpdateUI();
    }

    public void UpdateXP() {
        currentCostText.text = "Current XP Cost: " + currentCost;
        currentXP.text = "Current XP: " + activeChar.xp;
    }

    public void OnEnable() {
        UpdateUI();
    }

    public void UpdateOnAdvance() {
        UpdateUI();
    }
}
