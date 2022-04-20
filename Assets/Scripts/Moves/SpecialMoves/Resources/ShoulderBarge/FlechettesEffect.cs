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
    public List<StableCombatChar> targets;
    public List<GameObject> projectiles;
    public float projectileSpeed = 1;
    public void Init(StableCombatChar _caster, StableDamage _damage) {
        caster = _caster;
        damage = _damage;
        targets = new List<StableCombatChar>();
        projectiles = new List<GameObject>();
        foreach (Collider col in Physics.OverlapSphere(transform.position, 15)) {
            var scc = col.GetComponent<StableCombatChar>();
            if (scc == null) { continue; }
            if (scc.team == caster.team) { continue; }
            targets.Add(scc);
            var go = Instantiate(projectilePrefab, transform.position, transform.rotation);
            go.SetActive(true);
            projectiles.Add(go);
        }
        init = true;
    }
    public void Update() {
        if (!init) { return; }
        for (int x = 0; x < targets.Count; x++) {
            projectiles[x].transform.LookAt(targets[x].position + new Vector3(0, 1, 0));
            projectiles[x].transform.position += projectiles[x].transform.forward * Time.deltaTime * projectileSpeed;
            if (Vector3.Distance(projectiles[x].transform.position, targets[x].position) <= 1.25f) {
                targets[x].TakeDamage(new StableDamage() { mind = 2, balance = 2, stamina = 2, health = 1, isKnockdown = true });
                print("#TODO#Add Slow Effect");
                Destroy(projectiles[x]);
                projectiles[x] = null;
                targets[x] = null;
            }
        }
        projectiles.RemoveAll(x => x == null);
        targets.RemoveAll(x => x == null);
        if (targets.Count == 0) {
            Destroy(gameObject);
        }
    }
}
