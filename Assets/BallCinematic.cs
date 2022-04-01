using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCinematic : MonoBehaviour
{
    public Transform target;
    public float force;
    public void Shoot() {
        Physics.gravity = new Vector3(0,-1f,0);
        var body = GetComponent<Rigidbody>();
        transform.LookAt(target);
        transform.position += transform.forward * 1;
        body.velocity = Vector3.zero;
        Vector3 targetPos = target.position;
        body.AddForce((targetPos - transform.position).normalized * force);
    }
   
}
