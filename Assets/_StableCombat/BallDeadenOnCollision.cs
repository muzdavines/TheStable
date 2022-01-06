using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallDeadenOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        Rigidbody body = collision.transform.GetComponent<Rigidbody>();
        if (body != null) {
            body.velocity *= .1f;
        }
    }
}
