using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using UnityEngine.SceneManagement;
using UnityEngine;
using CoverShooter;

public class CombatController : MonoBehaviour
{
    public bool combatActive;
    public List<Character> heroes;
    public List<Character> enemies;
    public void Init(List<Character> _heroes, List<Character> _enemies) {
        combatActive = true;
        heroes = _heroes;
        enemies = _enemies;
        print("Set Teams in CombatController");
        print("Set Health Bars in CombatController");
        
        foreach (Character c in heroes) {
            
            c.currentMissionCharacter.SetCombatComponents(true);
            
        }
        foreach (Character d in enemies) {
            d.currentMissionCharacter.SetCombatComponents(true);
        }

        
    }
    public void EndCombat() {
        combatActive = false;
        GetComponent<MissionController>().EndCombat();
    }
    void Update()
    {
        if (!combatActive) { return; }
        if (Time.frameCount % 60 == 0) {
            CheckEndCombat();
        }
    }

    void CheckEndCombat() {
        
        foreach (Character c in heroes) {
            c.health = (int)c.currentObject.GetComponent<CharacterHealth>().Health;
            if (c.health <= 0) {
                heroes.Remove(c);
            }
        }
        foreach (Character cc in enemies) {
            
            cc.health = (int)cc.currentObject.GetComponent<CharacterHealth>().Health;
            if (cc.health <= 0) {
                enemies.Remove(cc);
            }
        }
        if (heroes.Count == 0 || enemies.Count == 0) {
            EndCombat();
        }

    }
}
