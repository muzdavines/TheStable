using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class MissionHelperHunt : MissionHelper
{

    public Transform targetForAnimalToRunTo;
    public Transform spawnLocForAnimal;
    public string animalToLoad;
    public bool randomizeAnimal;
    public GameObject animal;
    public override void Activate() {
        base.Activate();
        //spawn deer and make it run to a spot
        if (randomizeAnimal) { print("Create Random Animal List"); }
        animal = Instantiate<GameObject>(Resources.Load<GameObject>(animalToLoad));
        animal.GetComponent<NavMeshAgent>().enabled = false;
        animal.transform.position = spawnLocForAnimal.position;
        animal.GetComponent<NavMeshAgent>().enabled = true;
        animal.GetComponent<HuntedAnimal>().MoveTo(targetForAnimalToRunTo);
        animal.GetComponent<HuntedAnimal>().escapeTarget = spawnLocForAnimal;
        print("Hunt Activate");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
