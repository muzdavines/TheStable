using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndOfSeasonController : MonoBehaviour {
    public Text seasonSummary, explanation, prompt;
    public GameObject promoteButton, dontpromoteButton, continueButton;
    public void Init() {
        explanation.text =
            "The Flonkball season is over. If you won your league and have enough funds to pay for a berth in the next higher league, you may choose to do so now.";
        bool playerWonLeague = Game.instance.leagues[0].PlayerIsFirst();
        seasonSummary.text = playerWonLeague ? "You won the league this year." : "You didn't win the league this year.";
        bool playerHasEnoughMoney = Game.instance.playerStable.finance.gold >= 10000;
        seasonSummary.text += playerHasEnoughMoney
            ? "\nYou have enough money (10,000) to get promoted"
            : "\nYou don't have enough money (10,000) to get promoted.";
        promoteButton.SetActive(playerWonLeague && playerHasEnoughMoney);
        dontpromoteButton.SetActive(playerWonLeague && playerHasEnoughMoney);
        continueButton.SetActive(!playerWonLeague || !playerHasEnoughMoney);
    }

    public void Promote() {
        Game.instance.leagues[0].endOfSeasonCompleted = true;
        Game.instance.playerStable.finance.AddExpense(10000, LedgerAccount.Operations, "Promotion");
        Game.instance.playerStable.leagueLevel++;
        Game.instance.leagues[0].NewSeason(Game.instance.gameDate.year+1);
        gameObject.SetActive(false);
        Game.instance.leagues[0].endOfSeasonCompleted = false;
        Helper.UpdateAllUI();
    }

    public void Continue() {
        Game.instance.leagues[0].endOfSeasonCompleted = true;
        Game.instance.leagues[0].NewSeason(Game.instance.gameDate.year + 1);
        gameObject.SetActive(false);
        Game.instance.leagues[0].endOfSeasonCompleted = false;
        Helper.UpdateAllUI();
    }
}
