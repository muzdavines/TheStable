using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCWeaponChild : MonoBehaviour
{
    public SCWeapon parent;
    public void OnTriggerEnter(Collider other) {
        parent.OnTriggerEnter(other);

    }
}
