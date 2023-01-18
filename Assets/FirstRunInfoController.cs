using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRunInfoController : MonoBehaviour
{
    public void Discord() {
        Application.OpenURL("https://discord.gg/BVhaUfuQZP");
    }

    public void Complete() {
        Game.instance.firstRunComplete = true;
        gameObject.SetActive(false);
    }
    public void Start() {
        if (Game.instance.firstRunComplete) {
            gameObject.SetActive(false);
        }
    }
}
