using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCircleEffect : MonoBehaviour
{
    [SerializeField]
    public StableDamage damage;
    public float duration = 8f;
    bool init = false;
    public float startTime;
    StableCombatChar caster;
    void Start()
    {
        startTime = Time.time;
        Destroy(gameObject, duration+4);
    }

    public void Init(StableCombatChar _caster, StableDamage _damage) {
        caster = _caster;
        damage = _damage;
        if (damage == null) { damage = new StableDamage(); }
        init = true;
    }
    void Update()
    {
        if (!init) { return; }
        if (Time.time >= startTime + duration) {
            GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            init = false;
        }
    }

    public void OnTriggerStay(Collider other) {
        if (!init) { return; }
        if (Time.frameCount % 60 != 0) { return; }
        StableCombatChar scc = other.GetComponent<StableCombatChar>();
        if (scc == null) { return; }
        if (scc.team == caster.team) { return; }
        scc.TakeDamage(new StableDamage() { mind = 3 }, caster);
    }
}
