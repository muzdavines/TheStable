using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueManaMouseEvents : MonoBehaviour {
    private StableCombatChar currentHover;

    public void OnMouseEnter() {
        print("MouseEnter");
        StableCombatChar thisChar = GetComponent<StableCombatChar>();
        currentHover = thisChar;
    }

    public void OnMouseExit() {
        print("MouseExit");
        currentHover = null;
    }

    public void OnMouseDown() {
        print("MouseClick");
        if (currentHover != null) {
            FindObjectOfType<StableSceneController>().HeroClicked(currentHover);
        }
    }
}
