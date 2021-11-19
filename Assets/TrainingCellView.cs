using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedScrollerDemos.CellEvents;
public class TrainingCellView : EnhancedScrollerCellView {
    public Text trainingNameText;
    public Training thisTraining;
    Color defaultColor;
    public void SetData(Training data, bool clearColor = false) {
        defaultColor = GetComponent<Image>().color;
        thisTraining = data;
        if (data.moveToTrain != null) {
            trainingNameText.text = data.moveToTrain.name.Title();
        }
        else {
            trainingNameText.text = data.training.Title();
        }
        trainingNameText.text += "\nCost per Point: " + thisTraining.cost + "\nDuration: " + thisTraining.duration + " days";
        GetComponent<Image>().color = Helper.GetCellViewColor();
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
        FindObjectOfType<TrainingController>().SetActiveTraining(thisTraining);
        TrainingCellView[] cells = FindObjectsOfType<TrainingCellView>();
        foreach (TrainingCellView cell in cells) {
            cell.Deselect();
        }
        GetComponent<Image>().color = Color.green;
    }
    public void Deselect() {
        GetComponent<Image>().color = Helper.GetCellViewColor();
    }
}