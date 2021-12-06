using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StepType { Combat, Lockpick, Assassinate, Sneak, Demolish, Distract, DetectTrap, DisarmTrap, Intimidate, Persuade, Pickpocket, Entertain, Exit, Hunt, Forage, Fish, Camp, NavigateLand, Connection, NegotiateBusiness, Inspire, Portal, Gamble}
[System.Serializable]
public class Step {

    //step isn't over until it is successful or no saves (fail)

    public StepBlueprint blueprint;
    public StepType type;
    public bool required;
    public Reward reward;
    [SerializeField]
    public List<Step> saves;
    [Tooltip("Enter the score that the player should match or exceed. Think of it like Armor Class.")]
    public int level;
    public float mod = 0;
    public void Blueprint() {
        type = blueprint.type;
        required = blueprint.required;
        reward = blueprint.reward;
        level = blueprint.level;
        foreach (StepBlueprint b in blueprint.saves) {
            Step s = new Step();
            s.blueprint = b;
            s.Blueprint();
            saves.Add(s);
        }
    }

    public Character CharacterToAttempt(List<Character> heroes) {

        string property = "";
        switch (type) {
            case StepType.Hunt:
                property = "hunting";
                break;
            case StepType.Camp:
                property = "survivalist";
                break;
            case StepType.NavigateLand:
                property = "landNavigation";
                break;
            case StepType.NegotiateBusiness:
            case StepType.Connection:
            case StepType.Inspire:
                property = "negotiating";
                break;
            case StepType.Portal:
                property = "strength";
                break;
            case StepType.Gamble:
                property = "gambling";
                break;
            case StepType.Lockpick:
                property = "lockpicking";
                break;
            
        }
        Debug.Log(property);
        Character returnChar = new Character() { name = "ReturnChar" };
        typeof(Character).GetField(property).SetValue(returnChar, -1);
        for (int i = 0; i < heroes.Count; i++) {
            if (heroes[i].incapacitated) { continue; }
            Debug.Log(heroes[i].name+ " Prop: " + typeof(Character).GetField(property).GetValue(returnChar) + "  " + typeof(Character).GetField(property).GetValue(heroes[i]));
            if ((int)typeof(Character).GetField(property).GetValue(returnChar) < (int)typeof(Character).GetField(property).GetValue(heroes[i])) { 
                returnChar = heroes[i];
            }
        }

        return returnChar;
        /*

        for (int i = 1; i<heroes.Count; i++) {
            switch (type) {
                case StepType.Hunt:
                    Debug.Log(returnChar.hunting + "  " + heroes[i].hunting);
                    if (heroes[i].hunting > returnChar.hunting) {
                        returnChar = heroes[i];
                    }
                    break;
                case StepType.Camp:
                    Debug.Log("Camp - " + returnChar.survivalist + " " + heroes[i].survivalist);
                    if (heroes[i].survivalist > returnChar.survivalist) {
                        returnChar = heroes[i];
                    }
                    break;
            }
        }
        return returnChar;
    }
    */

    }
}



[CreateAssetMenu]
[SerializeField]
public class StepBlueprint : ScriptableObject {
    public StepType type;
    public bool required;
    public Reward reward;
   
    public List<StepBlueprint> saves;
    public int level;
}
