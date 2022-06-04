using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceSmashEffect : MonoBehaviour
{
    Transform target;
    bool init;
    bool moveTowards;
    StableCombatChar attacker;
    public void Init(Transform _target, StableCombatChar _attacker) {
        target = _target;
        attacker = _attacker;
        init = true;
        moveTowards = true;
    }
    public bool finished;

    // Update is called once per frame
    void Update()
    {
        if (!init || finished) { return; }
        if (moveTowards) {
            transform.LookAt(target.position + new Vector3(0, 1, 0));
            transform.position += transform.forward * Time.deltaTime * 40;
            if (Vector3.Distance(transform.position, target.position) < 1.5f) {
                moveTowards = false;
                target.GetComponent<StableCombatChar>().TakeDamage(new StableDamage() { health = 4, isKnockdown = true }, attacker);
            }
        } else {
            transform.LookAt(attacker.position + new Vector3(0, 1, 0));
            transform.position += transform.forward * Time.deltaTime * 40;
            if (Vector3.Distance(transform.position, attacker.position) < 2f) {
                finished = true;
            }
        }
    }
}
