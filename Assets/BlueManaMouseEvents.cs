using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueManaMouseEvents : MonoBehaviour {
    private StableCombatChar currentHover;
    public  HeroHoverPopupController hover;
    private GameObject blank;
    public virtual void Start() {
        hover = FindObjectOfType<HeroHoverPopupController>();
        blank = GameObject.Find("Blank");
    }
    public virtual void OnMouseEnter() {
        if (!blank.activeInHierarchy) {
            return;
        }
        print("MouseEnter");
        StableCombatChar thisChar = GetComponent<StableCombatChar>();
        Character toDisplay;
        if (thisChar == null) {
            return;
        }
        toDisplay = thisChar.myCharacter;
        currentHover = thisChar;
        if (hover) {
            hover.Display(toDisplay);
        }
    }

    public virtual void OnMouseExit() {
        if (!blank.activeInHierarchy) {
            return;
        }
        print("MouseExit");
        currentHover = null;
        if (hover) {
            hover.Close();
        }
    }
    
    public virtual void OnMouseDown() {
        print("MouseClick");
        if (!blank.activeInHierarchy) {
            return;}
        if (currentHover != null) {
            hover.Close();
            FindObjectOfType<StableSceneController>().HeroClicked(currentHover);
        }
    }
}
