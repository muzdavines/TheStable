using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Finance {
    [System.Serializable]
    public class Ledger {
        [System.Serializable]
        public class LedgerEntry {

            public LedgerEntryType type;
            public LedgerAccount account;
            public LedgerEntryClass entryClass;
            public int amount;
            public string tag;
            public string description;
            public bool reconciled = false;
            public Game.GameDate date;
        }
        
    }
    public enum DateScope { AllTime, Year, Last12};
    [System.Serializable]
    public class Business {
        public enum Benefit { None, Gold, HeroAccess, ItemAccess, Prestige, SkillTrainingAccess, TrainingBuff}
        public Benefit benefit;
        public string description;
        /// <summary>
        /// This number is either the amount of gold per turn, or the index of the hero or training buff
        /// </summary>
        public int number; 
        /// <summary>
        /// -1 for ongoing, otherwise the number of weeks
        /// </summary>
        public int duration;
        public string GetInfo() {
            return description + "\n" + number + " " + benefit.ToString() + "\n" + duration + " weeks.";
        }
        
    }

    public int GetTotal(LedgerAccount account, DateScope scope = Finance.DateScope.AllTime, int year= 1000) {
        int total = 0;
        switch (scope) {
            case DateScope.Year:
            foreach (Ledger.LedgerEntry l in ledger) {
                if (l.date.year != year || l.account != account) { continue; }
                    total += l.amount;
            }
            break;
        }
    return total;
    }

    public void AddExpense (int amount, LedgerAccount expenseAccount, string desc = "", string tag = "") {
        Finance.Ledger.LedgerEntry entry1 = new Finance.Ledger.LedgerEntry() { amount = amount, account = LedgerAccount.Gold, entryClass = LedgerEntryClass.Asset, type = LedgerEntryType.Credit, description = desc };
        Finance.Ledger.LedgerEntry entry2 = new Finance.Ledger.LedgerEntry() { amount = amount, account = expenseAccount, entryClass = LedgerEntryClass.Expense, type = LedgerEntryType.Debit, description = desc };
        List<Finance.Ledger.LedgerEntry> ledgerEntries = new List<Finance.Ledger.LedgerEntry> {
            entry1,
            entry2
        };
        AddTransaction(ledgerEntries);
    }
    public void AddRevenue(int amount, string desc = "", string tag = "") {
        Finance.Ledger.LedgerEntry entry1 = new Finance.Ledger.LedgerEntry() { amount = amount, account = LedgerAccount.Gold, entryClass = LedgerEntryClass.Asset, type = LedgerEntryType.Debit, description = desc};
        Finance.Ledger.LedgerEntry entry2 = new Finance.Ledger.LedgerEntry() { amount = amount, account = LedgerAccount.Revenue, entryClass = LedgerEntryClass.Revenue, type = LedgerEntryType.Credit, description = desc };
        List<Finance.Ledger.LedgerEntry> ledgerEntries = new List<Finance.Ledger.LedgerEntry> {
            entry1,
            entry2
        };
        AddTransaction(ledgerEntries);
    }
    public void AddTransaction(List<Ledger.LedgerEntry> entries) {
        //make sure the transaction is balanced
        int balance = 0;
        foreach (Ledger.LedgerEntry l in entries) {
            Debug.Log(l.account + "  " + l.amount + "  " + l.entryClass + "  " + l.Modifier() + "  " + l.type);
            balance += (l.amount * l.Modifier());
            
        }
        if (balance != 0) {
            Debug.Log("Transaction is not balanced: " + balance);
            return;
        }
        foreach (Ledger.LedgerEntry e in entries) {
            e.reconciled = true;
            e.date = Game.instance.gameDate;
            ledger.Add(e);
            if (!accounts.ContainsKey(e.account)) {
                accounts[e.account] = 0;
            }
            accounts[e.account] += (e.amount * e.Modifier());
        }
    }

    public void AddBusiness(Business b) {
        businesses.Add(b);
        Debug.Log("#Business#Added " + b.description);
    }
    public void ProcessBusinesses() {
        foreach (Business b in businesses) {
            b.duration--;
            switch (b.benefit) {
                case Business.Benefit.Gold:
                    AddRevenue(b.number, b.description, "Business Revenue");
                    break;
            }
        }
        businesses.RemoveAll(BusinessIsExpired);
    }
    bool BusinessIsExpired(Business b) {
        if (b.duration <= 0) {
            return true;
        }
        return false;
    }
    public List<Ledger.LedgerEntry> ledger = new List<Ledger.LedgerEntry>();
    public List<Business> businesses = new List<Business>();
    public int gold { get { return accounts.ContainsKey(LedgerAccount.Gold) ? accounts[LedgerAccount.Gold] : 0; } }
    public Dictionary<LedgerAccount, int> accounts = new Dictionary<LedgerAccount, int>();
}

public static class LedgerHelper {
        public static int Modifier(this Finance.Ledger.LedgerEntry l) {
        if (l.type == LedgerEntryType.Debit) {
            return 1;
        }
        else {
            return -1;
        }
    }
}
public enum LedgerEntryType { Debit, Credit}
public enum LedgerAccount { Gold, Buildings, Personnel, Operations, Equipment, Revenue}
public enum LedgerEntryClass { Asset, Liability, Expense, Revenue}