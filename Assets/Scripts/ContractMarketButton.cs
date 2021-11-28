using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractMarketButton : MainMenuButton
{
    public override void UpdateIndicator() {
        base.UpdateIndicator();
        int num = Game.instance.contractMarket.Count;
        string s = num > 0 ? num + "" : "";
        indicator.text = s;
    }
}
