using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StepType { Combat, Lockpick, Assassinate, Sneak, Demolish, Distract, DetectTrap, DisarmTrap, Intimidate, Persuade, Pickpocket, Entertain, Exit, Hunt, Forage, Fish, Camp, NavigateLand, Connection, NegotiateBusiness, Inspire, Portal, Gamble, Medicine, Collection}
[System.Serializable]
public class Step {

    //step isn't over until it is successful or no saves (fail)

    public StepBlueprint blueprint;
    public StepType type;
    public bool required;
    public Reward reward;
    [SerializeField]
    //public List<Step> saves;
    [Tooltip("Level 1-5")]
    public int level;
    public int challengeNum = 5;
    public float mod = 0;
    public void Blueprint() {
        type = blueprint.type;
        required = blueprint.required;
        reward = blueprint.reward;
        level = blueprint.level;
        /*foreach (StepBlueprint b in blueprint.saves) {
            Step s = new Step();
            s.blueprint = b;
            s.Blueprint();
            saves.Add(s);
        }*/
    }

    public MissionPOI.Attempter CharacterToAttempt(List<StableCombatChar> heroes) {
        var attempter = new MissionPOI.Attempter() { trait = new Trait() { level = 0 } };
        int bestScore = 0;
        int lastNotIncapacitated=-1;
        for (int i = 0; i < heroes.Count; i++) {
            if (heroes[i].myCharacter.incapacitated) { continue; }
            lastNotIncapacitated = i;
            var thisHeroTrait = heroes[i].myCharacter.GetBestTrait(type);
            if (thisHeroTrait.level > bestScore) {
                attempter.thisChar = heroes[i];
                attempter.trait = thisHeroTrait;
                bestScore = thisHeroTrait.level;
            }
        }
        if (attempter.trait.level == 0) {
            Debug.Log("#Step#No trait found.");
            attempter.thisChar = heroes[lastNotIncapacitated];
            attempter.trait = new Trait() { level = 0 };
        }
        return attempter;
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
   
    //public List<StepBlueprint> saves;
    public int level;
}
