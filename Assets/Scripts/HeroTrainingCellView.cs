using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroTrainingCellView : HeroCellView {
    
    public override void OnClick() {
        FindObjectOfType<TrainingController>().SetActiveChar(thisChar);
        HeroTrainingCellView[] cells = FindObjectsOfType<HeroTrainingCellView>();
        foreach (HeroTrainingCellView cell in cells) {
            cell.Deselect();
        }

        GetComponent<Image>().color = Color.green;
        print("Find Training controller and send the Character, also change the color of the active Cellview Hero");
    }

    public void Deselect() {
        GetComponent<Image>().color = Helper.GetCellViewColor();
    }
    
}
