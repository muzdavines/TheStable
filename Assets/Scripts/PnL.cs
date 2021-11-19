using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PnL : MonoBehaviour, UIElement
{
    public Text text;

    public void OnEnable() {
        int revenue, personnel, operations, equipment, buildings, net;
        Finance f = Game.instance.playerStable.finance;
        int year = Game.instance.gameDate.year;
        revenue = f.GetTotal(LedgerAccount.Revenue, Finance.DateScope.Year, year);
        personnel = f.GetTotal(LedgerAccount.Personnel, Finance.DateScope.Year, year);
        operations = f.GetTotal(LedgerAccount.Operations, Finance.DateScope.Year, year);
        equipment = f.GetTotal(LedgerAccount.Equipment, Finance.DateScope.Year, year);
        buildings = f.GetTotal(LedgerAccount.Buildings, Finance.DateScope.Year, year);
        net = revenue - (personnel + operations + equipment + buildings);

        //add Revenue
        //add Personnel
        //add Operations
        //add Equipment
        //add Buildings
        //add Net Income
        text.text = "Revenue: " + revenue + "\nPersonnel: " + personnel + "\nOperations: " + operations + "\nEquipment: " + equipment + "\nBuildings: " + buildings + "\nNet Income: " + net;
        
    }

    public void UpdateOnAdvance() {
        OnEnable();
    }
}
