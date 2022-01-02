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
        body.AddForce(GetFireAngle(passTarget.transform) * body.mass, ForceMode.Impulse);
    }

    public Vector3 GetFireAngle(Transform fireTarget) {
        Vector3 p = fireTarget.position;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = 0 * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
        return finalVelocity;
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
 
public static class ProjectileMath {
    public static bool LaunchAngle(float speed, float distance, float yOffset, float gravity, out float angle0, out float angle1) {
        angle0 = angle1 = 0;

        float speedSquared = speed * speed;

        float operandA = Mathf.Pow(speed, 4);
        float operandB = gravity * (gravity * (distance * distance) + (2 * yOffset * speedSquared));

        // Target is not in range
        if (operandB > operandA)
            return false;

        float root = Mathf.Sqrt(operandA - operandB);

        angle0 = Mathf.Atan((speedSquared + root) / (gravity * distance));
        angle1 = Mathf.Atan((speedSquared - root) / (gravity * distance));

        return true;
    }

    /// <summary>
    /// Calculates the initial launch speed required to hit a target at distance with elevation yOffset.
    /// </summary>
    /// <param name="distance">Planar distance from origin to the target</param>
    /// <param name="yOffset">Elevation of the origin with respect to the target</param>
    /// <param name="gravity">Downwards force of gravity in m/s^2</param>
    /// <param name="angle">Initial launch angle in radians</param>
    public static float LaunchSpeed(float distance, float yOffset, float gravity, float angle) {
        float speed = (distance * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / Mathf.Cos(angle))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle) + 2 * yOffset * Mathf.Cos(angle));

        return speed;
    }

    public static Vector2[] ProjectileArcPoints(int iterations, float speed, float distance, float gravity, float angle) {
        float iterationSize = distance / iterations;

        float radians = angle;

        Vector2[] points = new Vector2[iterations + 1];

        for (int i = 0; i <= iterations; i++) {
            float x = iterationSize * i;
            float t = x / (speed * Mathf.Cos(radians));
            float y = -0.5f * gravity * (t * t) + speed * Mathf.Sin(radians) * t;

            Vector2 p = new Vector2(x, y);

            points[i] = p;
        }

        return points;
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
