using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PracticeTarget : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject moveTarget;
    public void Start() {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(moveTarget.transform.position);
    }
}
