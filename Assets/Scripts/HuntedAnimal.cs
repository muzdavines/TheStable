using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class HuntedAnimal : MonoBehaviour
{
    NavMeshAgent agent;
    public Transform moveTarget;
    Animator anim;
    Rigidbody rigid;
    public Transform escapeTarget;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if (Vector3.Distance(moveTarget.position, transform.position) < 1.0f) { agent.isStopped = true; }
        float speed = agent.velocity.magnitude;
        //print("Speed: " + agent.velocity);
        if (speed < .5f) { speed = 0; }
        anim.SetFloat("MoveSpeed", speed);
    }

    public void MoveTo(Transform t) {
        agent = GetComponent<NavMeshAgent>();
        agent.transform.LookAt(t);
        moveTarget = t;
        agent.isStopped = false;
        agent.SetDestination(moveTarget.position);
    }
    public void Escape() {
        MoveTo(escapeTarget);
        Destroy(gameObject, 5.0f);
    }

    public void Death() {
        anim.SetTrigger("Death");
        Destroy(gameObject, 10.0f);

    }
}
