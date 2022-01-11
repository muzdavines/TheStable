using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionCombatTester : MonoBehaviour
{
    public List<Character> heroes;
    public List<Character> enemies;
    public List<Transform> heroSpawn;
    public List<Transform> enemySpawn;
    void Start()
    {
        foreach (Character h in heroes) {
            Character thisBaseChar = h;
            
            GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), heroSpawn[0].position, Quaternion.identity);
            StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
            thisChar.team = 0;
            thisChar.myCharacter = thisBaseChar;
            thisChar.GetComponent<SCModelSelector>().Init(thisBaseChar.modelNum, 0);
            thisChar.Init();
        }
        foreach (Character e in enemies) {
            Character thisBaseChar = e;
            
            GameObject co = Instantiate<GameObject>(Resources.Load<GameObject>(thisBaseChar.modelName), enemySpawn[0].position, Quaternion.identity);
            StableCombatChar thisChar = co.GetComponent<StableCombatChar>();
            thisChar.team = 1;
            thisChar.myCharacter = thisBaseChar;
            thisChar.GetComponent<SCModelSelector>().Init(thisBaseChar.modelNum, 1);
            thisChar.Init();
        }
    }

   
}
