using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public void SaveGame() {
        //ES3AutoSaveMgr.Current.Save();
        ES3.Save("Game", GameObject.Find("Game"));
    }
    public void LoadGame() {
        Destroy(GameObject.Find("Game"));
        ES3.Load("Game", GameObject.Find("Game"));
        return;
        var thisGame = GameObject.Find("Game");
        if (thisGame != null) { Destroy(thisGame); }
        ES3AutoSaveMgr.Current.Load();
    }

    public void LoadInit() {
        SceneManager.LoadScene("StableManagement");
    }
    
}
