using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementAnimationController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator anim;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update() {
        
        Debug.Log("Agent Speed: " + agent.speed);
        anim.SetFloat("MovementSpeed", agent.velocity.magnitude/agent.speed);
    }
}
