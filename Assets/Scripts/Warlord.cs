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
                attackMagic = defenseMagic = supportMagic = 10;
                break;
            case CharClass.Rogue:
                lockpicking = pickpocketing = 10;
                break;

        }
    }
}
