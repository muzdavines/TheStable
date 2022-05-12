using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ball : MonoBehaviour
{
    public StableCombatChar holder;
    public bool isHeld;
    
    public Rigidbody body;
    SphereCollider col;
    public Vector3 passTargetPosition;
    public StableCombatChar lastHolder;
    public float velocity { get { return body.velocity.magnitude; } }

    float shootErrorAdjustment = 2.5f;
    bool heatSeek;
    public Transform heatSeekTarget;

    private void Start() {
        body = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();
        //body.AddForce(new Vector3(Random.Range(0, 10), 0, 0));
    }
    public void Update() {
        if (heatSeek && heatSeekTarget !=null) {
            print("#TODO#Lerp towards target");
        }
        if (Time.frameCount % 60 == 0) {
            if (transform.position.y <= -1) {
                body.velocity = Vector3.zero;
                transform.position = new Vector3(transform.position.x, 10, transform.position.z);
            }
        }
    }
    public void StopBall() {
        body.velocity = Vector3.zero;
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
        if (ignoreCollisions) { Debug.Log("#Ball#Cannot pickup, ignoring collisions"); return false; }
        if (isHeld) { Debug.Log("#Ball#Cannot pickup, already held."); return false; }
        Debug.Log("#Ball#Picked up by: " + picker.myCharacter.name);
        holder = picker;
        lastHolder = holder;
        heatSeek = false;
        heatSeekTarget = null;
        isHeld = true;
        col.enabled = !isHeld;
        body.isKinematic = isHeld;
        body.interpolation = RigidbodyInterpolation.None;
        transform.parent = holder._rightHand;
        transform.localPosition = new Vector3(0, 0, 0);
        return true;
    }
    public void Shoot(Vector3 goalTarget, float error, float shotPower) {
        BeginIgnoreCollisions(.4f);
        Vector3 errorAdjustment = Random.insideUnitCircle * error * shootErrorAdjustment;
        Release();
        MoveToLaunchPosition(goalTarget, shotPower);
        body.velocity = Vector3.zero;

        Vector3 targetPos = goalTarget - errorAdjustment;
        body.AddForce((targetPos - transform.position).normalized * 1000 * shotPower);
    }

    public void PassTo(StableCombatChar passTarget, float shotPower = 1) {
        Release();
        MoveToLaunchPosition(passTarget.transform.position, shotPower);
        LaunchBall(passTarget.transform, true, shotPower);
    }

    private Vector3 GetLaunchVelocity(float flightTime, Vector3 startingPoint, Vector3 endPoint) {
        Vector3 gravityNormal = Physics.gravity.normalized;
        Vector3 dx = Vector3.ProjectOnPlane(endPoint, gravityNormal) - Vector3.ProjectOnPlane(startingPoint, gravityNormal);
        Vector3 initialVelocityX = dx / flightTime;

        Vector3 dy = Vector3.Project(endPoint, gravityNormal) - Vector3.Project(startingPoint, gravityNormal);
        Vector3 g = 0.5f * Physics.gravity * (flightTime * flightTime);
        Vector3 initialVelocityY = (dy - g) / flightTime;
        return initialVelocityX + initialVelocityY;
    }

    private void LaunchBall(Transform target, bool leadTarget = false, float shotPower = 1f) {
        BeginIgnoreCollisions();
        Rigidbody projectile = body;
        projectile.velocity = Vector3.zero;
        float thisDist = Vector3.Distance(projectile.transform.position, target.position);
        //Debug.Log(thisDist);
        float hangtime = Mathf.Clamp(thisDist / 15f, 1f, 7f);
        //Debug.Log(hangtime);
        Vector3 futurePositionOfTarget = leadTarget ? target.GetComponent<NavMeshAgent>().velocity * hangtime : Vector3.zero;
        passTargetPosition = target.position + futurePositionOfTarget * shotPower;
        projectile.AddForce(GetLaunchVelocity(hangtime, projectile.position, target.position + futurePositionOfTarget) * shotPower, ForceMode.VelocityChange);
    }
    public bool ignoreCollisions = false;
    public void BeginIgnoreCollisions(float duration = .4f) {
        ignoreCollisions = true;
        Physics.IgnoreLayerCollision(14, 10, true);
        StartCoroutine(DelayLayerCollisionActive(duration, 10));
    }

    public void GetDropped() {
        Release();
        body.AddForce(new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), Random.Range(-2, 2)) * 100);
    }
    public void GetSwatted(Goal goal) {
        Release();
        body.velocity = Vector3.zero;
        body.AddForce(new Vector3(Random.Range(-2.5f,2.5f)*100,500, Random.Range(-2.5f, 2.5f)*100));
    }
    void Release() {
        heatSeek = false;
        heatSeekTarget = null;
        isHeld = false;
        col.enabled = true;
        body.isKinematic = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        transform.parent = null;
        holder = null;
    }
    IEnumerator DelayLayerCollisionActive(float duration, int layerToTurnOn) {
        yield return new WaitForSeconds(duration);
        ignoreCollisions = false;
        Physics.IgnoreLayerCollision(14, layerToTurnOn, false);
    }
    void MoveToLaunchPosition(Vector3 launchTarget, float amount = 1f)
    {
        transform.LookAt(launchTarget);
        transform.position += transform.forward * amount;
    }
    public void OnCollisionEnter(Collision collision) {
        Debug.Log("#BallCollision#" + collision.transform.name);
        passTargetPosition = Vector3.zero;
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
