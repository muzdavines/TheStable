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
        MissionController control = FindObjectOfType<MissionController>();
        control.heroes = control.SpawnChars(heroes, heroSpawn, new MissionCharacterStatePostureIdle());
        control.heroes[0].currentMissionCharacter.team = 0;
        control.heroes[0].currentMissionCharacter.SetCombatComponents(true);
        control.currentEnemies = control.SpawnChars(enemies, enemySpawn, new MissionCharacterStatePostureIdle());
        control.currentEnemies[0].currentMissionCharacter.team = 1;
        control.currentEnemies[0].currentMissionCharacter.SetCombatComponents(true);
    }

   
}
