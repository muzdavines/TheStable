using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketContractScrollerController : ContractScrollerController
{
    
    public override void Start() {
        _data = new List<MissionContract>();
        foreach (MissionContract c in Game.instance.contractMarket) {
            _data.Add(c);
        }
        
        print(_data.Count + " Scroller");
        myScroller.Delegate = this;
        myScroller.ReloadData();
    }
}
