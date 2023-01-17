using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCKatanaController : MonoBehaviour {
    public Transform blade;
    public Vector3 baseRotation;
    public Vector3 reverseRotation;
    public void Start() {
        
        //baserotation = 
        //reverseRotation = 207,-133,54
    }
    public void Normal() {
        blade.transform.localRotation = Quaternion.Euler(baseRotation);
    }

    public void Reverse() {
        blade.transform.localRotation = Quaternion.Euler(reverseRotation);
    }
}
