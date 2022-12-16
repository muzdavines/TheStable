using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroHoverPopupController : MonoBehaviour {
    public GameObject panel;
    public TextMeshProUGUI charName, archetype, shooting, passing, tackling, carrying, speed, games, goals, assists, tackles, kos;

    void Start() {
        Close();
    }
    public void Display(Character thisChar) {
        panel.SetActive(true);
        charName.text = thisChar.name;
        archetype.text = thisChar.archetype.ToString();
        goals.text = thisChar.seasonStats.goals.ToString();
        games.text = thisChar.seasonStats.games.ToString();
        goals.text = thisChar.seasonStats.goals.ToString(); ;
        assists.text = thisChar.seasonStats.assists.ToString();
        tackles.text = thisChar.seasonStats.tackles.ToString();
        kos.text = thisChar.seasonStats.kos.ToString();
        shooting.text = thisChar.shooting.ToString();
        passing.text = thisChar.passing.ToString();
        tackling.text = thisChar.tackling.ToString();
        carrying.text = thisChar.carrying.ToString();
        speed.text = thisChar.runspeed.ToString();
    }

    public void Close() {
        panel.SetActive(false);
    }
}
