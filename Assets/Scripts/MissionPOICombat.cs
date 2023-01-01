using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPOICombat : MissionPOI
{
    public List<Character> enemies;
    public List<Transform> spawnLoc;
    public List<GameObject> objectsToDestroy;
    
    public override void StepActivated(StableCombatChar activeChar) {
        List<StableCombatChar> heroes = control.allChars;
        foreach (StableCombatChar c in heroes) {
            c.MissionIdleDontAct();
        }
        control.BeginCombat(enemies, spawnLoc, this);
        foreach (GameObject g in objectsToDestroy) {
            Destroy(g);
        }
    }

    public void PopulateEnemies(int dungeonLevel) {
        enemies = Resources.Load<CharacterListSO>("Enemies").GetRandomFromChallengeRating(dungeonLevel * 15);
    }
}
