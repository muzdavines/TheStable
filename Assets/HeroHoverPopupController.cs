using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroHoverPopupController : MonoBehaviour {
    public GameObject panel;
    public TextMeshProUGUI charName, archetype, goals;

    void Start() {
        Close();
    }
    public void Display(Character thisChar) {
        panel.SetActive(true);
        charName.text = thisChar.name;
        archetype.text = thisChar.archetype.ToString();
        goals.text = thisChar.seasonStats.goals.ToString();
    }

    public void Close() {
        panel.SetActive(false);
    }
}
