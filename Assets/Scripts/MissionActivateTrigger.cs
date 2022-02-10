using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionActivateTrigger : MonoBehaviour
{
    public Collider col;
    public MissionPOI mission;
    // Start is called before the first frame update
    void Start()
    {
        mission = GetComponentInParent<MissionPOI>();
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other) {

        //POI activated, start task and broadcast to teammates to move here
        if (other.CompareTag("Character")) {
            print("MissionActivated");
            col.enabled = false;
            mission.StepActivated(other.GetComponent<StableCombatChar>());
        }
    }
}
