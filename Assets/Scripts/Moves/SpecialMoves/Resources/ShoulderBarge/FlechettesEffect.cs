using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlechettesEffect : MonoBehaviour
{
    [SerializeField]
    public StableDamage damage;
    bool init = false;
    public float startTime;
    StableCombatChar caster;
    public GameObject projectilePrefab;
    public float projectileSpeed = 1;
    class ProjectileTargets {
        public StableCombatChar target;
        public GameObject proj;
    }
    List<ProjectileTargets> projs = new List<ProjectileTargets>();
    public void Init(StableCombatChar _caster, StableDamage _damage) {
        caster = _caster;
        damage = _damage;
        foreach (Collider col in Physics.OverlapSphere(transform.position, 15)) {
            var scc = col.GetComponent<StableCombatChar>();
            if (scc == null) { continue; }
            if (scc.team == caster.team) { continue; }
            ProjectileTargets thisProj = new ProjectileTargets();
            thisProj.target = scc;
            var go = Instantiate(projectilePrefab, transform.position, transform.rotation);
            go.SetActive(true);
            thisProj.proj = go;
            projs.Add(thisProj);
        }
        init = true;
    }
    public void Update() {
        if (!init) { return; }
       
        for (int x = 0; x < projs.Count; x++) {
            if (projs[x].target == null || projs[x].proj == null) {
                projs[x] = null;
                continue;
            }
            projs[x].proj.transform.LookAt(projs[x].target.position + new Vector3(0, 1, 0));
            projs[x].proj.transform.position += projs[x].proj.transform.forward * Time.deltaTime * projectileSpeed;
            if (Vector3.Distance(projs[x].proj.transform.position, projs[x].target.position) <= 1.25f) {
                projs[x].target.TakeDamage(new StableDamage() { mind = 2, balance = 2, stamina = 2, health = 1, isKnockdown = true }, caster);
                print("#TODO#Add Slow Effect");
                Destroy(projs[x].proj);
                projs[x] = null;
            }
        }
        projs.RemoveAll(x => x == null);
        if (projs.Count == 0) {
            Destroy(gameObject);
        }
    }
}
