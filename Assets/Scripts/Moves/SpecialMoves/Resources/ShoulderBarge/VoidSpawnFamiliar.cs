using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidSpawnFamiliar : MonoBehaviour {
    public StableCombatChar owner;
    float startTime;
    public void Start() {
        startTime = Time.time;
        Destroy(gameObject, 34f);
    }
    public void Update() {
        if (Time.time >= startTime + 30) {
            GetComponent<StableCombatChar>().TakeDamage(new StableDamage() { stamina = 1000, health = 1000, isKnockdown = true }, null);
        }
    }
}
