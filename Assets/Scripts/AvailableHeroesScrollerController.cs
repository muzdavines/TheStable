using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvailableHeroesScrollerController : MissionHeroesScrollerController {
    public override void OnEnable() {
        _data = new List<Character>();
        foreach (Character c in Game.instance.playerStable.heroes) {
            if (c.activeForNextMission) { continue; }
            _data.Add(c);
        }
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
}