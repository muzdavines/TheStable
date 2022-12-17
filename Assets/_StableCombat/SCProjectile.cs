using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(SphereCollider))]
[RequireComponent (typeof(Rigidbody))]
public class SCProjectile : MonoBehaviour
{
    public GameObject collisionEffect;
    public SCImpact impactEffect;
    public Transform myTarget;
    public Transform myPositionTarget;
    public StableDamage myDamage;
    public enum FireType { Direct, Lob, Seeker}
    public FireType fireType;
    public SphereCollider col;
    public Rigidbody body;
    public bool isKinematic;
    public float speed = 1f;
    public bool fired = false;
    public StableCombatChar launcherChar;
    public StableCombatChar targetChar;
    Transform _t;
    public bool noise;
    public int numProjectiles = 1;


    private void Start() {
        _t = transform;
        col = GetComponent<SphereCollider>();
        body = GetComponent<Rigidbody>();
        col.isTrigger = true;
        body.isKinematic = isKinematic;
        return;
        if (targetChar.fieldSport) {
            targetChar.coach.CheckDivineIntervention();
        }
    }
    private void Update() {
        if (!fired) { return; }

        if (noise) {
            Vector3 direction = Vector3.MoveTowards(transform.position, myTarget.position, 1f);

            // Add some random noise to the direction
            direction = direction * Random.Range(0.9f, 1.1f);

            // Move the projectile in the direction of the target
            transform.Translate(direction * speed * Time.deltaTime);
        }
        else {
            _t.position = Vector3.MoveTowards(_t.position, myTarget.position + new Vector3(0, 1, 0), speed * Time.deltaTime);
        }
        
        _t.LookAt(myTarget.position);
    }

    public void OnTriggerEnter(Collider other) {
        StableCombatChar otherChar = other.GetComponent<StableCombatChar>();
        if (otherChar == null) {
            return;
        }

        if (otherChar == launcherChar || otherChar.team == launcherChar.team) {
            return;
        }

        fired = false;
        col.enabled = false;


        if (otherChar != null) {
            otherChar.TakeDamage(myDamage, launcherChar);
        }

        if (collisionEffect != null) {
            Destroy(Instantiate<GameObject>(collisionEffect, transform.position, transform.rotation), 5.0f);
        }

        if (impactEffect != null) {
            Destroy(Instantiate<SCImpact>(impactEffect, transform.position, transform.rotation).Impact(launcherChar.team).gameObject, 5.0f);
        }


        Destroy(gameObject, .05f);
    }

    public void Fire(Transform target, StableDamage damage, StableCombatChar _launcher) {
        fired = true;
        myTarget = target;
        myDamage = damage;
        launcherChar = _launcher;
    }
    public void Fire(Vector3 target, StableDamage damage) {

    }
}
