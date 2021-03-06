using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Util_Trajectory : MonoBehaviour {
    [Header("Continious Path")]
    [SerializeField] List<Vector3> pathVertList = new List<Vector3>();
    [SerializeField] float time = 1;

    [Space]
    [SerializeField] float launchMagnitude = 1f;
    [SerializeField] float launchAngle = 45f;
    [SerializeField] bool preferSmallAng = false;
    [SerializeField] Vector3 velocity = Vector3.right;
    [SerializeField] Vector3 acceleration = Vector3.down;
    [SerializeField] Vector3 unityAccuracyFix = Vector3.zero;
    [SerializeField] int splits = 3;

    [Header("Dotted Path")]
    [SerializeField] bool timeBasedDot = false;
    [SerializeField] List<Vector3> dotList = new List<Vector3>();
    [SerializeField] float dotTimeSpace = 0.1f;
    [Space]
    [SerializeField] int dotCount = 10;
    [SerializeField] float dotDistSpace = 0.25f;
    [SerializeField] float dotCalcTimeStep = 0.1f;
    [SerializeField] bool usePathforDots = false;

    [Space]
    [SerializeField] float pathLength = 0;
    public float Distance_AtVel_DueToAcc_InTime(float u, float a, float t) {
        return u * t + 0.5f * a * t * t;
    }
    public float Vel_ForDistance_DueToAcc_InTime(float d, float a, float t) {
        return (d - Distance_AtVel_DueToAcc_InTime(0, a, t)) / time;
    }
    public float Magnitude_ToReachXY_InGravity_AtAngle(float x, float y, float g, float ang) {
        float sin2Theta = Mathf.Sin(2 * ang * Mathf.Deg2Rad);
        float cosTheta = Mathf.Cos(ang * Mathf.Deg2Rad);
        float inner = (x * x * g) / (x * sin2Theta - 2 * y * cosTheta * cosTheta);
        if (inner < 0) {
            return float.NaN;
        }
        float res = Mathf.Sqrt(inner);
        return res;
    }
    public float Angle_ToReachXY_InGravity_AtMagnitude(float x, float y, float g, float mag) {
        float innerSq = Mathf.Pow(mag, 4) - g * (g * x * x + 2 * y * mag * mag);
        if (innerSq < 0) {
            return float.NaN;
        }
        float innerATan;
        if (preferSmallAng) {
            innerATan = (mag * mag - Mathf.Sqrt(innerSq)) / (g * x);
        }
        else {
            innerATan = (mag * mag + Mathf.Sqrt(innerSq)) / (g * x);
        }

        float res = Mathf.Atan(innerATan) * Mathf.Rad2Deg;
        return res;
    }

    public void Calculate_Trajectory() {
        pathLength = 0;

        if (pathVertList == null) {
            pathVertList = new List<Vector3>();
        }

        if (pathVertList.Count > splits) {
            pathVertList.RemoveRange(splits, (pathVertList.Count - splits));
        }
        else if (pathVertList.Count < splits) {
            pathVertList.AddRange(new Vector3[splits - pathVertList.Count]);
        }
        float dt = 0;
        Vector3 d;
        for (int i = 0; i < splits; i++) {
            dt = (time / (splits - 1)) * i;
            d.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
            d.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
            d.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
            pathVertList[i] = d;
            if (i > 0) {
                pathLength += Vector3.Distance(pathVertList[i], pathVertList[i - 1]);
            }
        }
    }
    public void Calculate_Velocity(Vector3 targetPos) {
        Vector3 d = (targetPos - transform.position);
        velocity.x = Vel_ForDistance_DueToAcc_InTime(d.x, acceleration.x, time);
        velocity.y = Vel_ForDistance_DueToAcc_InTime(d.y, acceleration.y, time);
        velocity.z = Vel_ForDistance_DueToAcc_InTime(d.z, acceleration.z, time);
    }
    public void Calculate_Dots() {
        pathLength = 0;
        int dotNum = (timeBasedDot ? (int)(time / dotTimeSpace + 0.5f) : dotCount);

        if (dotList == null) {
            dotList = new List<Vector3>();
        }

        if (dotList.Count > dotNum) {
            dotList.RemoveRange(dotNum, (dotList.Count - dotNum));
        }
        else if (dotList.Count < dotNum) {
            dotList.AddRange(new Vector3[dotNum - dotList.Count]);
        }

        float dt = 0;
        Vector3 dot = Vector3.zero;
        Vector3 dir = Vector3.one;
        if (timeBasedDot) {
            for (int i = 0; i < dotNum; i++) {
                dt = (i + 1) * dotTimeSpace;
                dot.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
                dot.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
                dot.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
                dotList[i] = dot;
                if (i > 0) {
                    pathLength += Vector3.Distance(dotList[i], dotList[i - 1]);
                }
            }
        }
        else if (dotNum > 0) {
            bool usingPath = usePathforDots;
            int plIndx = 0;
            dotList[0] = Vector3.zero;
            for (int i = 1; i < dotNum; i++) {
                while (Vector3.Distance(dotList[i - 1], dot) < dotDistSpace) {
                    if (usingPath) {
                        if (plIndx < pathVertList.Count) {
                            dot = pathVertList[plIndx++];
                        }
                        else {
                            usingPath = false;
                            dt = time;
                        }
                    }
                    else {
                        dt += dotCalcTimeStep;
                        dot.x = Distance_AtVel_DueToAcc_InTime(velocity.x, acceleration.x, dt);
                        dot.y = Distance_AtVel_DueToAcc_InTime(velocity.y, acceleration.y, dt);
                        dot.z = Distance_AtVel_DueToAcc_InTime(velocity.z, acceleration.z, dt);
                    }
                }
                dir = (dot - dotList[i - 1]).normalized;
                dotList[i] = dotList[i - 1] + dir * dotDistSpace;

                if (i > 0) {
                    pathLength += Vector3.Distance(dotList[i], dotList[i - 1]);
                }
            }
        }

    }

    [Header("Ref")]
    [SerializeField] LineRenderer lineRendere = null;
    [SerializeField] Rigidbody projectile;
    [SerializeField] Transform target;

    [Header("Editor Setting")]
    [SerializeField] bool calc_Trajectory = false;
    [SerializeField] bool calc_Velocity = false;
    [SerializeField] bool calc_SuitalbeMagAng = false;
    [SerializeField] bool calc_Dots = false;
    [SerializeField] bool auto_calc = false;

    [Space]
    [SerializeField] bool fire = false;
    float magChange = 0, angChange = 0;
    Vector3 velChange = Vector3.zero;
    private void OnDrawGizmosSelected() {
        if (magChange != launchMagnitude) {
            if (calc_SuitalbeMagAng) {
                float x = (target.position.x - transform.position.x);
                float y = (target.position.y - transform.position.y);
                float g = -acceleration.y;
                float newAng = Angle_ToReachXY_InGravity_AtMagnitude(x, y, g, launchMagnitude);
                if (float.IsNaN(newAng)) {
                    launchMagnitude = magChange;
                }
                else {
                    magChange = launchMagnitude;
                    launchAngle = newAng;
                    velocity.x = Mathf.Cos(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                    velocity.y = Mathf.Sin(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                }
            }
            else {
                magChange = launchMagnitude;
                velocity.x = Mathf.Cos(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                velocity.y = Mathf.Sin(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
            }
        }

        if (angChange != launchAngle) {
            if (calc_SuitalbeMagAng) {
                float x = (target.position.x - transform.position.x);
                float y = (target.position.y - transform.position.y);
                float g = -acceleration.y;
                float newMag = Magnitude_ToReachXY_InGravity_AtAngle(x, y, g, launchAngle);
                if (float.IsNaN(newMag)) {
                    launchAngle = angChange;
                }
                else {
                    angChange = launchAngle;
                    launchMagnitude = newMag;
                    velocity.x = Mathf.Cos(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                    velocity.y = Mathf.Sin(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                }
            }
            else {
                angChange = launchAngle;
                velocity.x = Mathf.Cos(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
                velocity.y = Mathf.Sin(launchAngle * Mathf.Deg2Rad) * launchMagnitude;
            }
        }

        if (velChange != velocity) {
            velChange = velocity;
            launchMagnitude = velocity.magnitude;
            launchAngle = Vector3.SignedAngle(Vector3.right, velocity, Vector3.forward);
        }

        time = Mathf.Max(0.001f, time);
        dotCount = Mathf.Max(0, dotCount);
        dotTimeSpace = Mathf.Max(0.001f, dotTimeSpace);
        dotDistSpace = Mathf.Max(0.001f, dotDistSpace);
        dotCalcTimeStep = Mathf.Max(0.001f, dotCalcTimeStep);
        splits = Mathf.Max(2, splits);

        if (calc_Trajectory) {
            if (!auto_calc)
                calc_Trajectory = false;
            Calculate_Trajectory();
            lineRendere.positionCount = splits;
            lineRendere.SetPositions(pathVertList.ToArray());
        }

        if (calc_Velocity) {
            if (!auto_calc)
                calc_Velocity = false;
            Calculate_Velocity(target.transform.position);
        }

        if (calc_Dots) {
            if (!auto_calc)
                calc_Dots = false;
            Calculate_Dots();
        }

        if (fire) {
            fire = false;
            projectile.transform.position = transform.position;
            projectile.velocity = velocity + unityAccuracyFix;
        }
    }
    public Transform launchPoint;
    public void Update() {
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            projectile.transform.position = launchPoint.position;
            projectile.velocity = Vector3.zero;
            float thisDist = Vector3.Distance(projectile.transform.position, target.position);
            Debug.Log(thisDist);
            float hangtime = Mathf.Clamp(thisDist / 25f, .5f, 4f);
            Debug.Log(hangtime);
            Vector3 futurePositionOfTarget = target.GetComponent<NavMeshAgent>().velocity * hangtime;
            projectile.AddForce(GetLaunchVelocity(hangtime, projectile.position, target.position + futurePositionOfTarget), ForceMode.VelocityChange);
        }
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

    private void OnDrawGizmos() {
        if (dotList != null) {
            for (int i = 0; i < dotList.Count; i++) {
                Gizmos.DrawWireSphere(dotList[i], 0.05f);
            }
        }
    }
}