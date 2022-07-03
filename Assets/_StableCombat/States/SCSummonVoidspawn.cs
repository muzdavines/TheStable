using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SCSummonVoidspawn : StableCombatCharState {

    public override void EnterFrom(StableCombatCharState state) {
        base.EnterFrom(state);
        canGrabBall = false;
        thisChar.agent.isStopped = true;
        thisChar.anima.Summon();
    }
    public override void Update() {
        base.Update();
    }

    public override void AnimEventReceiver(string message) {
        base.AnimEventReceiver(message);
        if (message == "SpawnEffect") {
            GameObject voidSpawnPrefab = Resources.Load<GameObject>("Voidspawn");

            for (int i = 0; i < 5; i++) {
                var voidspawn = GameObject.Instantiate(voidSpawnPrefab);
                voidspawn.transform.position = thisChar.position + new Vector3(1 + i, 0, 0);
                var thisSpawn = voidspawn.GetComponent<StableCombatChar>();
                var agent = thisSpawn.GetComponent<NavMeshAgent>();
                agent.enabled = false;
                Character spawnChar = Resources.Load<Character>("Voidspawn");
                thisSpawn.myCharacter = spawnChar;
                thisSpawn.team = thisChar.team;
                thisSpawn.fieldSport = true;
                thisSpawn.playStyle = PlayStyle.Fight;
                thisSpawn.fieldPosition = Position.STC;
                thisSpawn.gameObject.AddComponent<FireGolemFamiliar>();
                thisSpawn.Init();
                agent.enabled = true;
                thisSpawn.GetComponent<NavMeshAgent>().ResetPath();
            }
        }
    }

    public override void WillExit() {
        base.WillExit();
    }
}