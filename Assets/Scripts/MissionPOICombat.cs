using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPOICombat : MissionPOI
{
    public List<CharacterSO> enemies;
    public List<Transform> spawnLoc;
    public List<GameObject> objectsToDestroy;
    public override void StepActivated(MissionCharacter activeChar) {
        List<Character> heroes = control.heroes;
        foreach (Character c in heroes) {
            c.currentMissionCharacter.IdleDontAct();
        }
        control.BeginCombat(enemies, spawnLoc, this);
        foreach (GameObject g in objectsToDestroy) {
            Destroy(g);
        }
    }
}
