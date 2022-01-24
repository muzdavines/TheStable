using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(SphereCollider))]
[RequireComponent (typeof(Rigidbody))]
public class SCProjectile : MonoBehaviour
{
    public GameObject collisionEffect;
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
    StableCombatChar launcherChar;
    Transform _t;
    private void Start() {
        _t = transform;
        col = GetComponent<SphereCollider>();
        body = GetComponent<Rigidbody>();
        col.isTrigger = true;
        body.isKinematic = isKinematic;
    }
    private void Update() {
        if (!fired) { return; }
        _t.position = Vector3.MoveTowards(_t.position, myTarget.position+new Vector3(0,1,0), speed * Time.deltaTime);
        _t.LookAt(myTarget.position);
    }

    public void OnTriggerEnter(Collider other) {
        StableCombatChar otherChar = other.GetComponent<StableCombatChar>();
        if (otherChar == launcherChar) {
            return;
        }
        fired = false;
        col.enabled = false;
        if (otherChar != null) {
            otherChar.TakeDamage(myDamage);
        }
        if (collisionEffect != null) {
            Destroy(Instantiate<GameObject>(collisionEffect, transform.position, transform.rotation), 5.0f);
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
