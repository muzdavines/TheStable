using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeroInfoPanelController : MonoBehaviour, PopupElement
{
    public GameObject panel;
    public Text heroName, heroAge, heroContract;
    public Text sportAttributes;
    public Text combatAttributes;
    public void OnHover(Character c)
    {
        panel.SetActive(true);
        heroName.text = c.name;
        heroAge.text = "Age: "+c.age;
        heroContract.text = "Contract\nSigning Bonus: " + c.contract.signingBonus +"\nWeekly Salary: " + c.contract.weeklySalary + "\nWeeks Remaining: " + c.contract.weeksLeft;
        sportAttributes.text = "Match Attributes\n\n Archetype: " + c.archetype.ToString() + "\nSpeed: " + c.runspeed + "\nShooting: " + c.shooting + "\nPassing: " + c.passing + "\nCarrying: " + c.carrying + "\nTackling: " + c.tackling + "\n\n<b>Special Abilities</b>\n";
        foreach (var special in c.startingSpecialMoves) {
            sportAttributes.text += special + "\n";
        }
    }
    public void OnHoverExit()
    {
        panel.SetActive(false);
    }
    public void Close() {
        panel.SetActive(false);
    }
}
