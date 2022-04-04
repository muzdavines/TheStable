using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStage : MonoBehaviour
{
   public Button buttonListen;
    public float timer;
    float endTime;
    public enum Type { Timer, Button, AnyKey }
    public Type type;
    public TutorialSet mySet;
    public void Begin(TutorialSet _mySet) {
        mySet = _mySet;
        gameObject.SetActive(true);
        if (buttonListen != null) {
            buttonListen.onClick.AddListener(Clicked);
        }
        if (type == Type.Timer) {
            endTime = timer + Time.time;
        } else {
            endTime = Mathf.Infinity;
        }
    }
    public void Update() {
        switch (type) {
            case Type.Timer:
                break;
            case Type.Button:
                break;
            case Type.AnyKey:
                break;
        }
    }
    public void Clicked() {
        buttonListen.onClick.RemoveListener(Clicked);
        EndStage();
    }
    public void EndStage() {
        mySet.NextStage();
        gameObject.SetActive(false);
    }

}
