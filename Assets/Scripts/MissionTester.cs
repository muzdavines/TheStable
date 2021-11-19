using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MissionTester : MonoBehaviour
{
    public bool active = false;
    // Start is called before the first frame update
    void Start()
    {
        if (!active) { Destroy(gameObject); return; }
        SceneManager.LoadScene("NewGameCreation");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
