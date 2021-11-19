using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OnSuccessCamp : OnStepSuccess {
    public override void OnSuccess(int quality) {
        StartCoroutine(DisplayText(quality));
    }

    public IEnumerator DisplayText(int quality) {
        yield return new WaitForSeconds(6.0f);
        Text update = FindObjectOfType<MissionController>().update;
        string updateText = "";
        switch (quality) {
            case 1:
                updateText = "The camp contains only a meager campfire. The heroes must sleep on the ground, exposed to the elements. They are undefended.";
                break;
            case 2:
                updateText = "The camp has a fire and shelter. The heroes will sleep well but have no ameneties to bolster their spirits. They are undefended.";
                break;
            case 3:
                updateText = "The camp has fire and shelter. An outhouse provides an amenity but they remain undefended.";
                break;
            case 4:
                updateText = "The camp has been expertly built, with a fire, shelter, outhouse, and protective wall.";
                break;
            default:
                update.text += "\nCamp established.";
                break;

        }
        update.text = updateText;
    }
    public override void OnSuccess() {
        
    }
}
