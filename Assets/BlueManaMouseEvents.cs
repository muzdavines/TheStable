using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueManaMouseEvents : MonoBehaviour {
    private StableCombatChar currentHover;
    private HeroHoverPopupController hover;
    private GameObject blank;
    void Start() {
        hover = FindObjectOfType<HeroHoverPopupController>();
        blank = GameObject.Find("Blank");
    }
    public void OnMouseEnter() {
        if (!blank.activeInHierarchy) {
            return;
        }
        print("MouseEnter");
        StableCombatChar thisChar = GetComponent<StableCombatChar>();
        currentHover = thisChar;
        hover.Display(thisChar.myCharacter);
    }

    public void OnMouseExit() {
        print("MouseExit");
        currentHover = null;
        hover.Close();
    }

    public void OnMouseDown() {
        print("MouseClick");
        if (!blank.activeInHierarchy) {
            return;}
        if (currentHover != null) {
            hover.Close();
            FindObjectOfType<StableSceneController>().HeroClicked(currentHover);
        }
    }
}
