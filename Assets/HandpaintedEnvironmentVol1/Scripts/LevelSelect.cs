using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    //public GameObject CharObject;
    //public GameObject MenuCamera;
    //public Canvas canvas;
    private int increment = 1;
    // Start is called before the first frame update
    void Start()
    {
        //canvas.enabled = false;
    }

    public void LoadRedForest()
    {
        SceneManager.LoadScene("RedForest_Demo");
    }
    public void LoadGreenForest()
    {
        SceneManager.LoadScene("FantasyForest_Demo");
    }
    public void LoadAlienForest()
    {
        SceneManager.LoadScene("AlienWorld_Demo");
    }
    public void LoadAutumnForest()
    {
        SceneManager.LoadScene("Autumn_Forest_Demo");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            SceneManager.LoadScene("FantasyForest_Demo");
        }
        if (Input.GetKeyDown("2"))
        {
            SceneManager.LoadScene("RedForest_Demo");
        }
        if (Input.GetKeyDown("3"))
        {
            SceneManager.LoadScene("Autumn_Forest_Demo");
        }
        if (Input.GetKeyDown("4"))
        {
            SceneManager.LoadScene("AlienWorld_Demo");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            increment++;
            //CharObject.GetComponent<FirstPersonController>().m_MouseLook.lockCursor = true;
            //CharObject.GetComponent<FirstPersonController>().enabled= !CharObject.GetComponent<FirstPersonController>().enabled;

            //canvas.enabled = !canvas.enabled;
            

            
        }
        /*if (canvas.enabled.Equals(true))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
      
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }*/

    }
}
