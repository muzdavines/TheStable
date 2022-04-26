using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using UnityEngine.SceneManagement;
using UnityEngine;


public class CombatController : MonoBehaviour
{
    public bool combatActive;
    public List<StableCombatChar> heroes;
    public List<StableCombatChar> enemies;
    public HeroUIController enemyUI;
    public void Init(List<StableCombatChar> _heroes, List<StableCombatChar> _enemies) {
        combatActive = true;
        heroes = _heroes;
        enemies = _enemies;
        print("Set Teams in CombatController");
        print("Set Health Bars in CombatController");
        
        foreach (StableCombatChar c in heroes) {
            
           // c.SetCombatComponents(true);
            
        }
        foreach (StableCombatChar d in enemies) {
            //d.SetCombatComponents(true);
        }

        
    }
    public void EndCombat(bool success) {
        combatActive = false;
        GetComponent<MissionController>().EndCombat(success);
    }
    void Update()
    {
        if (!combatActive) { return; }
        if (Time.frameCount % 60 == 0) {
            CheckEndCombat();
        }
    }
    bool combatEnded = false;
    int lastEnemyCount;
    void CheckEndCombat() {
        if (combatEnded) { return; }
        foreach (StableCombatChar c in heroes) {
            if (c.health <= 0) {
                heroes.Remove(c);
            }
        }
        heroes.RemoveAll((x) => x.health <= 0);
        enemies.RemoveAll((x) => x.health <= 0);
        if (enemies.Count != lastEnemyCount) {
            lastEnemyCount = enemies.Count;
            enemyUI.Init(enemies);
        }
        if (heroes.Count == 0) {
            combatEnded = true;
            EndCombat(false);
        } else if (enemies.Count == 0) {
            combatEnded = true;
            EndCombat(true);
        }

    }
}
