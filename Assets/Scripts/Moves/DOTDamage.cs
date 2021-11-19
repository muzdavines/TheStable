using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTDamage : MonoBehaviour
{

    public StableDamage damage;
    public float nextDamage;
    public float endTime;
    public CharacterHealth characterHealth;
    public GameObject effect;
    public string dotName;
    bool initDone;
    void Start()
    {
        
    }

    public void Init(StableDamage _damage, CharacterHealth _health, float modifier, string _dotName) {
        damage = _damage;
        damage.stamina = (int)modifier * damage.stamina;
        damage.balance = (int)modifier * damage.balance;
        damage.mind = (int)modifier * damage.mind;
        damage.health = (int)modifier * damage.health;
        endTime = Time.time + 30;
        initDone = true;
        dotName = _dotName;
        characterHealth = _health;
    }

    void Update()
    {
        if (!initDone) {
            return;
        }
        if (Time.time > endTime) {
            Destroy(effect);
            Destroy(this);
            return;
        }
        if (Time.time > nextDamage) {
            ApplyDamage();
        }
    }

    void ApplyDamage() {
        nextDamage = Time.time + 1;
        characterHealth.Deal(damage);
    }
}
