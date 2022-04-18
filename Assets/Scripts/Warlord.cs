using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Warlord : Character
{
    public int warlordReputation;

    public void InitWarlord(StableMasterType c){

        switch (c)
        {
            case StableMasterType.Warrior:
                swordsmanship = 10;
                strength = 10;
                break;
            case StableMasterType.Wizard:
                attackmagic = defensemagic = supportmagic = 10;
                break;
            case StableMasterType.Rogue:
                lockpicking = pickpocketing = 10;
                break;

        }
    }
}
