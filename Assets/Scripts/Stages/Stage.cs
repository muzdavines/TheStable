using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Attribute {None, Lockpick, Demolish, Pay, Camp, Negotiate, Charm, Intimidate, Stealth, Snipe, Beatdown, Reputation, Socialize, }
public enum StageType { Combat, Lockpick, Travel, Connection, Negotiate, Intimidate, Assassinate, PitchedBattle, Inspire, Collection, DungeonCrawl}
[System.Serializable]
public class Stage
{
    public StageType type;
    public string subType;
    public string loadName { get { return type.ToString() + subType; } }
    public int difficulty;
    public string description;
    public int preferredMethod;
    public int bonusMethod; //hidden - if the player chooses this, he gets a bonus

    public List<Attribute> options { get; set; }

    public void Init() {
        options = new List<Attribute>();
        switch (type) {
            case StageType.Lockpick:
                options.Add(Attribute.Lockpick);
                options.Add(Attribute.Demolish);
                break;
            case StageType.Travel:
                options.Add(Attribute.Pay);
                options.Add(Attribute.Camp);
                break;
            case StageType.Negotiate:
                options.Add(Attribute.Negotiate);
                options.Add(Attribute.Charm);
                options.Add(Attribute.Intimidate);
                break;
            case StageType.Intimidate:
                options.Add(Attribute.Intimidate);
                options.Add(Attribute.Negotiate);
                break;
            case StageType.Assassinate:
                options.Add(Attribute.Stealth);
                options.Add(Attribute.Snipe);
                options.Add(Attribute.Beatdown);
                break;
            case StageType.Connection:
                options.Add(Attribute.Socialize);
                options.Add(Attribute.Reputation);
                break;
            case StageType.Inspire:
                options.Add(Attribute.Intimidate);
                options.Add(Attribute.Reputation);
                options.Add(Attribute.Pay);
                break;
            case StageType.Collection:
                options.Add(Attribute.Intimidate);
                options.Add(Attribute.Negotiate);
                break;
            default:
                options.Add(Attribute.None);
                break;

        }
    }
}
