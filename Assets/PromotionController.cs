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
        if (activeChar.mod.promoteOptions.Length == 1) {
            OnFirstClicked(true);
        }
        headers[0].transform.parent.gameObject.SetActive(false);
        headers[1].transform.parent.gameObject.SetActive(false);
        for (int i = 0; i < activeChar.mod.promoteOptions.Length; i++) {
            UpgradeModifier.Promote p = activeChar.mod.promoteOptions[i];
            headers[i].text = p.promoteButtonLabel;
            descriptions[i].text = p.selectionText.NewLine();
            headers[i].transform.parent.gameObject.SetActive(true);
        }
        
        mainText.text = activeChar.mod.promoteMainText;
        selectionText.text = activeChar.mod.promoteOptions[activeIndex].promoteDescription;
        buttonLabel.text = "Promote to " + activeChar.mod.promoteOptions[activeIndex].promoteArchetype.ToString();
        
    }
    public void OnFirstClicked(bool b) {
        if (b) { OnClick(0); } else {
            OnClick(1);
        }
    }
    
    public void OnClick(int index) {
        activeIndex = index;
        selectionText.text = activeChar.mod.promoteOptions[activeIndex].promoteDescription;
        buttonLabel.text = "Promote to " + activeChar.mod.promoteOptions[activeIndex].promoteArchetype.ToString();
    }

    public void Promote() {
        activeChar.Promote(activeChar.mod.promoteOptions[activeIndex].promoteArchetype);
        
        trainingController.Init(activeChar);
        trainingController.DelayUpdate();
        
    }

}
