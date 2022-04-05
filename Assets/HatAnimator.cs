using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatAnimator : MonoBehaviour
{
    
    public void Animate() {
        GetComponent<Rigidbody>().isKinematic = false;
        transform.parent = null;
        Physics.gravity = new Vector3(0, -1f, 0);
        GetComponent<Rigidbody>().velocity = new Vector3(0, .5f, 0);
        GetComponent<Rigidbody>().AddTorque(new Vector3(1, 1, 1));
    }
}
