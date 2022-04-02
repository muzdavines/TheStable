using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSetter : MonoBehaviour
{
    [SerializeField, Range(30, 120)]
    int target = 60;

    void Start()
    {
        QualitySettings.vSyncCount = 2;
        Application.targetFrameRate = target;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Application.targetFrameRate != target)
        {
            Application.targetFrameRate = target;
        }
    }
}
