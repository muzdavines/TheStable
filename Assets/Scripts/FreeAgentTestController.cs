using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class FreeAgentTestController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Destroy(Game.instance.gameObject);
            SceneManager.LoadScene("NewGameCreation");
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            //Game.instance.playerStable.heroes[0].currentTraining.BeginTraining(Game.instance.playerStable.heroes[0]);
        }
        
    }
}
