using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TacticsFieldPositionController : MonoBehaviour, IDropHandler {
    public Position pos;
    public Character currentHero;
    public MissionHeroesScrollerController activeHeroesScroller;
    public TextMeshProUGUI heroName, heroType;

    public void Awake() {
        heroName = transform.Find("Name").GetComponent<TextMeshProUGUI>();
        activeHeroesScroller = FindObjectOfType<MissionHeroesScrollerController>();
    }
    public void OnDrop(PointerEventData eventData) {
        print("Valid");
        print("Dropped By: " + eventData.selectedObject.name);
        var c = eventData.selectedObject.GetComponent<MissionHeroCellView>().thisChar;
        print(Game.instance.playerStable.NumberHeroesInLineup());
        if (!currentHero && Game.instance.playerStable.NumberHeroesInLineup() >= 6) {
            activeHeroesScroller.OnEnable();
            return;
        }
        Set(c);

    }

    public void OnClick() {
        if (currentHero) {
            currentHero.activeInLineup = false;
            currentHero.currentPosition = Position.NA;
            heroName.text = "";
            heroType.text = "";
            currentHero = null;
        }
        activeHeroesScroller.OnEnable();
    }

    void Set(Character c) {
        
        if (currentHero) {
            currentHero.activeInLineup = false;
            currentHero.currentPosition = Position.NA;
        }
        c.activeInLineup = true;
        c.currentPosition = pos;
        currentHero = c;
        activeHeroesScroller.OnEnable();
        heroName.text = c.name;
        heroType.text = c.archetype.ToString();
    }

    public void ManualSet(Character c) {
        Set(c);
    }
    public void Reset() {
        currentHero = null;
        heroName.text = "";
        heroType.text = "";
    }
}
