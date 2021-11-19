using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableContractsScrollerController : ContractScrollerController
{
    public override void Start() {
        _data = new List<MissionContract>();
        foreach (MissionContract c in Game.instance.playerStable.contracts) {
            _data.Add(c);
        }
        print(_data.Count + " Scroller");
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
}
