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
        heroName.text = c.myName;
        heroAge.text = "Age: "+c.age;
        heroContract.text = "Contract\nSigning Bonus: " + c.contract.signingBonus +"\nWeekly Salary: " + c.contract.weeklySalary + "\nWeeks Remaining: " + c.contract.weeksLeft;
        sportAttributes.text = "Match Attributes\n\n Position: "+c.archetype.ToString() + "\nSpeed: " + c.runspeed + "\nDodging: " + c.carrying + "\nTackling: " + c.tackling + "\nBlocking: " + c.blocking;
    }
    public void OnHoverExit()
    {
        panel.SetActive(false);
    }
    public void Close() {
        panel.SetActive(false);
    }
}
