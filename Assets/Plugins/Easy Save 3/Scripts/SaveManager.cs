using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public void Load() {
        var thisGame = GameObject.Find("Game");
        if (thisGame != null) { Destroy(thisGame); }
        var GO = ES3.Load("Game", GameObject.Find("Game)"));
        GO.SendMessage("OnLoad");
    }

    public void Save() {
        ES3.Save("Game", GameObject.Find("Game"));
    }
}
