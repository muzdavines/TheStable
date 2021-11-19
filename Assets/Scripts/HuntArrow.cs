using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntArrow : MonoBehaviour
{
    public Transform target;
    public float speed = 1;
    public bool success;
    public HuntedAnimal animal;
    public MissionCharacterStateHunt charHunt;
    private void Update() {
        if (target == null) { return; }
        transform.LookAt(target.position + new Vector3(0, 1, 0));
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Vector3.Distance(transform.position, target.position) < 2f) {
            if (success) {
                animal.Death();
            }
            else { animal.Escape(); }
            charHunt.ArrowComplete();
            Destroy(gameObject);
        }
    }
}
