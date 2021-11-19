using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class FreeAgentMarket
{
    public List<Character> market = new List<Character>();
    
    public bool PurchaseHero(int index, Stable stable) {
        Character c = market[index];
        Finance finance = stable.finance;
        if (finance.gold < c.contract.signingBonus) {
            Debug.Log("Can't Afford Signing Bonus");
            return false;
        }
        //transaction
        Finance.Ledger.LedgerEntry entry1 = new Finance.Ledger.LedgerEntry() { amount = c.contract.signingBonus, account = LedgerAccount.Gold, entryClass = LedgerEntryClass.Asset, type = LedgerEntryType.Credit };
        Finance.Ledger.LedgerEntry entry2 = new Finance.Ledger.LedgerEntry() { amount = c.contract.signingBonus, account = LedgerAccount.Personnel, entryClass = LedgerEntryClass.Expense, type = LedgerEntryType.Debit};
        List<Finance.Ledger.LedgerEntry> ledgerEntries = new List<Finance.Ledger.LedgerEntry>();
        ledgerEntries.Add(entry1);
        ledgerEntries.Add(entry2);
        finance.AddTransaction(ledgerEntries);
        return true;
    }

    public void UpdateMarket() {
        market = new List<Character>();
        for (int i = 0; i < 6; i++) {
            market.Add(new Character() { swordsmanship = 20, health = 5,  modelName = "CharWarrior2", name = Names.Warrior[Random.Range(0, Names.Warrior.Length)], contract = new EmploymentContract() { signingBonus = Random.Range(500, 2000), weeklySalary = Random.Range(50, 150), weeksLeft = 48 }, weapon = new Weapon() {damage = 20 }, armor = new Armor() { health = 2, condition = 100 } });
        }
    }

    public void TestMarket() {
        market.Add(new Character() { name = "Joe", contract = new EmploymentContract() { signingBonus = 1000, weeklySalary = 100, weeksLeft = 10 } });
        PurchaseHero(0, Game.instance.playerStable);
    }
}
