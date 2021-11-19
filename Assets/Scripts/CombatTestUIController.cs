using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CombatTestUIController : MonoBehaviour
{

    public CombatTestController combatData;
    public Text[] characterName;
    public int team = 0;
    public int character = 0;

    private void Update() {
        team = 0;
        for (int i = 0; i < 4; i++) {
            
            characterName[i].text = combatData.teams[team][character].character.name;
            
            
            
            
            
            
            
            
            if (character == 0) { character = 1; } else { character = 0; }
            if (i == 1) { team = 1; }
            
        }
    }

}
