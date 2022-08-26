using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionActiveHeroesScrollerController : MissionHeroesScrollerController, UIElement {
    public bool tactics;
    public override void OnEnable() {
        _data = new List<Character>();
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (tactics) { if (!c.activeInLineup) { continue; } } else { if (!c.activeForNextMission) { continue; } }
            _data.Add(c);
        }
        myScroller.Delegate = this;
        StartCoroutine(DelayReload());
    }
    IEnumerator DelayReload() {
        yield return new WaitForEndOfFrame();
        myScroller.ReloadData();
    }
    public void UpdateOnAdvance() {
        OnEnable();
    }
}