using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRayHelper : MonoBehaviour
{
    public Vector3 start, dir;

    void Start()
    {
        
    }

    public void SetRay(Vector3 _start, Vector3 _dir) {
        start = _start;
        dir = _dir;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(start, dir, Color.yellow);
    }
}
