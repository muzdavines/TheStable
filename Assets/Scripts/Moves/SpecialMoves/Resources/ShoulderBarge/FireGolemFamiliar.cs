using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGolemFamiliar : MonoBehaviour
{
    public StableCombatChar owner;
    float startTime;
    public void Start() {
        startTime = Time.time;
        Destroy(gameObject, 14f);
    }
    public void Update() {
        if (Time.time >= startTime + 10) {
            GetComponent<StableCombatChar>().TakeDamage(new StableDamage() { stamina = 1000, health = 1000, isKnockdown = true }, null);
        }
    }
}
