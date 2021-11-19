using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLoadHelper : MonoBehaviour
{
    public List<GameObject> objectsToLoad;

    private void Start()
    {
        foreach (GameObject go in objectsToLoad)
        {
            Instantiate(go, Vector3.zero, Quaternion.identity, transform);
        }
    }
}
