using System.Collections;
using System.Collections.Generic;
using PsychoticLab;
using UnityEngine;
using UnityEngine.AI;

public class StableSceneController : MonoBehaviour {
    public List<Transform> places;
    void Start() {
        var heroes = Game.instance.playerStable.heroes;
        BaseSpawnChars(heroes, places, 0);
    }

    void Update()
    {
        
    }
    List<StableCombatChar> BaseSpawnChars(List<Character> chars, List<Transform> spawns, int team = 0) {
        print("Spawn Base Chars");
        List<StableCombatChar> theseChars = new List<StableCombatChar>();
        for (int i = 0; i < chars.Count; i++) {
            Character thisBaseChar = chars[i];
            GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), spawns[i].transform.position, Quaternion.identity);
            StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
            thisChar.fieldSport = false;
            thisChar.myCharacter = thisBaseChar;
            thisChar.team = team;
            thisChar.combatFocus = thisBaseChar.combatFocus;

            thisChar.GetComponent<SCModelSelector>()?.Init(thisBaseChar.modelNum, thisBaseChar.skinNum);
            thisChar.GetComponent<CharacterRandomizer>()?.Init(thisBaseChar, team == 0 ? Game.instance.playerStable.primaryColor : Color.gray, team == 0 ? Game.instance.playerStable.secondaryColor : Color.black);
            thisChar.Init();
            co.GetComponent<NavMeshAgent>().enabled = false;
            co.transform.position = spawns[i].transform.position;
            co.transform.rotation = spawns[i].transform.rotation;
            co.GetComponent<NavMeshAgent>().enabled = true;
            theseChars.Add(thisChar);
            StartCoroutine(DelaySeated(thisChar));
        }
        
        return theseChars;

    }

    IEnumerator DelaySeated(StableCombatChar _char) {
        yield return new WaitForSeconds(.25f);
        _char.Cinematic();
    }
    
}
