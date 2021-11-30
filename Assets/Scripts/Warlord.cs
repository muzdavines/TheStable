using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Warlord : Character
{
    public int warlordReputation;

    public void InitWarlord(CharClass c){

        switch (c)
        {
            case CharClass.Warrior:
                swordsmanship = 10;
                strength = 10;
                break;
            case CharClass.Wizard:
                attackmagic = defensemagic = supportmagic = 10;
                break;
            case CharClass.Rogue:
                lockpicking = pickpocketing = 10;
                break;

        }
    }
}
