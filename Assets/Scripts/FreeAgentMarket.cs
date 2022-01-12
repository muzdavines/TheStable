using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class FreeAgentMarket
{
    [SerializeField]
    public List<Character> market = new List<Character>();
    [SerializeField]
    public List<CharacterSave> marketSave = new List<CharacterSave>();
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
            market.Add(new Character() { maxHealth = 4, maxStamina = 10, maxBalance = 10, maxMind = 10, swordsmanship = 20, health = 5,  modelName = "CharAristocrat", name = Names.Warrior[Random.Range(0, Names.Warrior.Length)], contract = new EmploymentContract() { signingBonus = Random.Range(500, 2000), weeklySalary = Random.Range(50, 150), weeksLeft = 48 }, startingMeleeWeapon = "FistsSO", activeMeleeMoves = new List<Move>() { Resources.Load<Move>("LeftJab") }, knownMoves = new List<Move>() { Resources.Load<Move>("LeftJab") } });
        }
    }

    public void TestMarket() {
        market.Add(new Character() { name = "Joe", contract = new EmploymentContract() { signingBonus = 1000, weeklySalary = 100, weeksLeft = 10 } });
        PurchaseHero(0, Game.instance.playerStable);
    }
    public void PrepForSave() {
        var saveList = new List<CharacterSave>();
        for (int i = 0; i < market.Count; i++) {
            saveList.Add(new CharacterSave().CopyValues(market[i]));
        }
        marketSave = saveList;
    }
    public void OnLoad() {
        if (marketSave != null && marketSave.Count > 0) {
            market = marketSave.LoadCharacters();
        }
        marketSave = new List<CharacterSave>();
        
    }
}
