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
        public TextMeshProUGUI archetype;

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
    public GameObject promoteButton;
    public TextMeshProUGUI promoteText;
    public GameObject promoteWindow;
    public void SetTraining() {

    }
    public void Init(Character _activeChar) {
        activeChar = _activeChar;
        currentShooting = 0;
        currentCost = 0;
        UpdateUI();
    }

    public void DelayUpdate() {
        StartCoroutine(IDelayUpdate());
    }
    IEnumerator IDelayUpdate() {
        yield return new WaitForEndOfFrame();
        GetComponentInParent<HeroMainController>().UpdateUI();
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
        scroller.Start();
        string known = "";
        if (activeChar != null) {
            heroName.text = activeChar.name;
            heroType.text = activeChar.archetype.ToString();
            archetype.text = activeChar.archetype.ToString();
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
        nextShootingCost.text = "Next XP: "+activeChar.UpgradeCost(CharacterAttribute.shooting, currentShooting);
        nextPassingCost.text = "Next XP: " + activeChar.UpgradeCost(CharacterAttribute.passing, currentPassing);
        nextTacklingCost.text = "Next XP: " + activeChar.UpgradeCost(CharacterAttribute.tackling, currentTackling);
        nextCarryingCost.text = "Next XP: " + activeChar.UpgradeCost(CharacterAttribute.carrying, currentCarrying);
        if (activeChar.ReadyToPromote()) {
            promoteButton.SetActive(true);
            promoteText.text = "Ready to Promote";
        }else {
            promoteButton.SetActive(false);
            promoteText.text = activeChar.GetPromoteText();
        }
        promoteWindow.SetActive(false);

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
        if (Input.GetKeyDown(KeyCode.P)) {
            activeChar.shooting = 100;
            activeChar.carrying = 100;
            activeChar.tackling = 100;
            activeChar.passing = 100;
            UpdateUI();
            GetComponentInParent<HeroMainController>().UpdateUI();
        }
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
                thisCost = activeChar.UpgradeCost(CharacterAttribute.shooting, currentShooting);
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentShooting += 1;
                currentShootingText.text = currentShooting.ToString();
                break;
            case "passing":
                thisCost = activeChar.UpgradeCost(CharacterAttribute.passing, currentPassing);
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentPassing += 1;
                currentPassingText.text = currentPassing.ToString();
                break;
            case "tackling":
                thisCost = activeChar.UpgradeCost(CharacterAttribute.tackling, currentTackling);
                if (thisCost > activeChar.xp - currentCost) {
                    return;
                }
                currentTackling += 1;
                currentTacklingText.text = currentTackling.ToString();
                break;
            case "carrying":
                thisCost = activeChar.UpgradeCost(CharacterAttribute.carrying, currentCarrying);
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
        int thisCost = 0;
        switch (attribute) {
            case "shooting":
                if (currentShooting <= 0) {
                    return;
                }
                currentShooting -= 1;
                thisCost = activeChar.UpgradeCost(CharacterAttribute.shooting);
                currentShootingText.text = currentShooting.ToString();
                break;
            case "tackling":
                if (currentTackling<= 0) {
                    return;
                }
                currentTackling -= 1;
                thisCost = activeChar.UpgradeCost(CharacterAttribute.tackling);
                currentTacklingText.text = currentTackling.ToString();
                break;
            case "passing":
                if (currentPassing <= 0) {
                    return;
                }
                currentPassing -= 1;
                thisCost = activeChar.UpgradeCost(CharacterAttribute.passing);
                currentPassingText.text = currentPassing.ToString();
                break;
            case "carrying":
                if (currentCarrying <= 0) {
                    return;
                }
                currentCarrying -= 1;
                thisCost = activeChar.UpgradeCost(CharacterAttribute.carrying);
                currentCarryingText.text = currentCarrying.ToString();
                break;
        }
        currentCost -= thisCost;
        UpdateUI();
    }

    public void TraitTraining(Trait traitToTrain) {
        int cost = traitToTrain.level == 0 ? traitToTrain.baseCost * 4 : traitToTrain.level * traitToTrain.baseCost;
        if (cost > activeChar.xp) {
            Debug.Log("#Training#Not enough XP");
            return;
        }

        if (traitToTrain.level >= 5) {
            Debug.Log("#Training#Max Level.");
            return;
        }
        activeChar.xp -= cost;
        if (traitToTrain.level == 0) {
            Trait newTrait = Instantiate(traitToTrain);
            newTrait.level = 1;
            activeChar.activeTraits.Add(newTrait);
        }
        else {
            traitToTrain.level++;
        }
        Init(activeChar);
    }
    public void Promote() {
        if (!activeChar.ReadyToPromote()) {
            return;
        }
        promoteWindow.SetActive(true);
        promoteWindow.GetComponent<PromotionController>().Init(activeChar);
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
