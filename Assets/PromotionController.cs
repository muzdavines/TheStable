using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromotionController : MonoBehaviour
{
    public Character activeChar;
    public TrainingController trainingController;
    public Text[] headers;
    public Text[] descriptions;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI selectionText;
    public int activeIndex;
    public Toggle firstButton;
    public TextMeshProUGUI buttonLabel;
    public void Init(Character _activeChar) {
        activeChar = _activeChar;
        activeIndex = firstButton.isOn ? 0 : 1;
        for (int i = 0; i < 2; i++) {
            UpgradeModifier.Promote p = activeChar.mod.promoteOptions[i];
            headers[i].text = p.promoteButtonLabel;
            descriptions[i].text = p.promoteDescription;
        }
        
        mainText.text = activeChar.mod.promoteMainText;
        selectionText.text = activeChar.mod.promoteOptions[activeIndex].selectionText.NewLine();
        buttonLabel.text = "Promote to " + activeChar.mod.promoteOptions[activeIndex].promoteArchetype.ToString();
        
    }
    public void OnFirstClicked(bool b) {
        if (b) { OnClick(0); } else {
            OnClick(1);
        }
    }
    
    public void OnClick(int index) {
        activeIndex = index;
        selectionText.text = activeChar.mod.promoteOptions[activeIndex].selectionText.NewLine();
        buttonLabel.text = "Promote to " + activeChar.mod.promoteOptions[activeIndex].promoteArchetype.ToString();
    }

    public void Promote() {
        activeChar.Promote(activeChar.mod.promoteOptions[activeIndex].promoteArchetype);
        
        trainingController.Init(activeChar);
        trainingController.DelayUpdate();
        
    }

}
