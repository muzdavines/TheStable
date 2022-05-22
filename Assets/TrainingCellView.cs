using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
public class TrainingCellView : EnhancedScrollerCellView {
    public Text traitName, traitDescription, traitLevel, costToUpgrade;
    public Trait thisTrait;
    Color defaultColor;
    public void SetData(Trait data, bool clearColor = false) {
        thisTrait = data;
        traitName.text = thisTrait.traitName;
        traitDescription.text = thisTrait.description;
        traitLevel.text = thisTrait.level > 0 ? "Current Level: "+thisTrait.level : "";

        costToUpgrade.text = thisTrait.level > 0 ? "Cost To Upgrade: "+thisTrait.baseCost * thisTrait.level : "Cost to Add: " + thisTrait.baseCost * 4;
    }

    public void OnHoverEnter() {
       // print("MouseEnter " + thisChar.name);
       // GameObject.FindObjectOfType<HeroInfoPanelController>().OnHover(thisChar);


    }
    public void OnHoverExit() {
       // print("MouseExit" + thisChar.name);
       // GameObject.FindObjectOfType<HeroInfoPanelController>().OnHoverExit();
    }
    public virtual void OnClick() {

        FindObjectOfType<TrainingController>().TraitTraining(thisTrait);


        TrainingCellView[] cells = FindObjectsOfType<TrainingCellView>();
        foreach (TrainingCellView cell in cells) {
            cell.Deselect();
        }
        GetComponent<Image>().color = new Color(255, 255, 255, 255);
    }
    public void Deselect() {
        GetComponent<Image>().color = Helper.GetCellViewColor();
    }
}