using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour
{

    public float targetDistance = 0.5F;
    public float smoothTime = 10;
    public Transform target;

    private float distance = 1F;


    float initialDistanceX;
    float initialDistanceZ;
    void Start()
    {
        initialDistanceX = transform.position.x - target.position.x;
        initialDistanceZ = transform.position.z - target.position.z;
    }
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.transform.position.x + initialDistanceX,
                                            target.transform.position.y,
                                           target.transform.position.z + initialDistanceZ),Time.deltaTime *10);
    }

}

