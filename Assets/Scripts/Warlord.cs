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
                
                break;
            case StableMasterType.Wizard:
              
                break;
            case StableMasterType.Rogue:
              
                break;

        }
    }
}
