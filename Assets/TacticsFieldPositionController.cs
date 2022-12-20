using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TacticsFieldPositionController : MonoBehaviour, IDropHandler {
    public Position pos;
    public Character currentHero;
    public MissionHeroesScrollerController activeHeroesScroller;
    public TextMeshProUGUI heroName;
    public void OnDrop(PointerEventData eventData) {
        print("Valid");
        print("Dropped By: " + eventData.selectedObject.name);
        var c = eventData.selectedObject.GetComponent<MissionHeroCellView>().thisChar;
        if (currentHero) {
            currentHero.activeInLineup = false;
            currentHero.currentPosition = Position.NA;
        }

        c.activeInLineup = true;
        c.currentPosition = pos;
        currentHero = c;
        activeHeroesScroller.OnEnable();
        heroName.text = c.name;
    }
}
