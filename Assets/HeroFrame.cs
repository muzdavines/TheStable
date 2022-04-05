using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroFrame : MonoBehaviour
{
    public Image portrait;
    public HeroPlaystyleSelectController playstyleController;
    public HeroUIController controller;
    public StableCombatChar myChar;
    public Text heroName;
    public Slider health, stamina, balance, mind;
    public void Init(StableCombatChar _myChar) {
        myChar = _myChar;
        playstyleController.Init(myChar);
        heroName.text = myChar.myCharacter.name;
        health.maxValue = myChar.maxHealth;
        stamina.maxValue = myChar.maxStamina;
        balance.maxValue = myChar.maxBalance;
        mind.maxValue = myChar.maxMind;
        health.value = myChar.health;
        stamina.value = myChar.stamina;
        balance.value = myChar.balance;
        mind.value = myChar.mind;
    }


}
