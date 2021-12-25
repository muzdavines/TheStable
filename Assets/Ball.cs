using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public StableCombatChar holder;
    public bool isHeld;
    public Rigidbody body;
    SphereCollider col;

    private void Start() {
        body = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        body.AddForce(new Vector3(Random.Range(-10, 10), 0, 0));
    }
    public void Update() {
        if (Time.frameCount % 60 == 0) {
            if (transform.position.y <= -1) {
                body.velocity = Vector3.zero;
                transform.position = new Vector3(transform.position.x, 10, transform.position.z);
            }
        }
    }
    public int TeamHolding() {
        if (holder != null) {
            return holder.team;
        } else { return -1; }
    }
    public float Distance(Vector3 pos) {
        return Vector3.Distance(pos, transform.position);
    }
    public bool PickupBall(StableCombatChar picker) {
        if (isHeld) { Debug.Log("Cannot pickup, already held."); return false; }
        holder = picker;
        isHeld = true;
        col.enabled = !isHeld;
        body.isKinematic = isHeld;
        transform.parent = holder._rightHand;
        transform.localPosition = new Vector3(0, 1, 0);
        return true;
    }
    public void Shoot(Goal goalTarget) {
        Release();
        transform.LookAt(goalTarget.transform);
        transform.position += transform.forward * 1;
        body.AddForce((goalTarget.transform.position- transform.position).normalized * 1000);
    }

    public void PassTo(StableCombatChar passTarget) {
        Release();
        transform.LookAt(passTarget.transform);
        transform.position += transform.forward * 1;
        body.AddForce((passTarget.transform.position - transform.position).normalized * 900);
    }

    public void GetDropped() {
        Release();
        body.AddForce(new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2)) * 300);
    }
    void Release() {
        isHeld = false;
        col.enabled = true;
        body.isKinematic = false;
        transform.parent = null;
        holder = null;
    }
    public void OnCollisionEnter(Collision collision) {
        Debug.Log("#BallCollision#" + collision.transform.name);
        collision.transform.GetComponent<StableCombatChar>()?.state.BallCollision(collision);
    }
}

public static class BallHelper {
    public static float Distance (this Ball ball, Transform other) {
        return Vector3.Distance(ball.transform.position, other.position);
    }
    public static float Distance (this Ball ball, StableCombatChar other) {
        return ball.Distance(other.transform);
    }
    public static bool PickupBall(this Ball ball, StableCombatChar picker) {
        return ball.PickupBall(picker);
    }
}
