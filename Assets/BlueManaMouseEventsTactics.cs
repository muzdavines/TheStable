using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueManaMouseEventsTactics : BlueManaMouseEvents {
    private TacticsFieldPositionController controller;
    
    public override void Start() {
        base.Start();
        controller = GetComponent<TacticsFieldPositionController>();
    }

    public override void OnMouseEnter() {
        print("MouseEnter");
       Character toDisplay;
       if (controller.currentHero == null) {
           return;}
       toDisplay = controller.currentHero;
       
        if (hover) {
            hover.Display(toDisplay);
        }
    }

    public override void OnMouseExit() {
        if (hover) {
            hover.Close();
        }
    }
}
