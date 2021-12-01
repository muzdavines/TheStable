using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HeroEditController : MonoBehaviour
{
    public GameObject panel;
    public Character activeCharacter;
    public StableManagementController controller;
    public TextMeshProUGUI[] heroStats;
    public void OpenPanel(Character _active) {
        activeCharacter = _active;
        controller.OnClick(panel);
        
        panel.SetActive(true);
        SetHeroStats();
    }
    public void SetHeroStats() {
        int index = 0;
        int count = 0;
        string n = "\n";
        heroStats[0].text = activeCharacter.name+n + "<b><u><b>Hero Stats: </b></u>" + n;
        
        FieldInfo[] properties = typeof(Character).GetFields();
        Debug.Log(properties + " props");
        foreach (FieldInfo property in properties) {
            Type thisType;
            try { thisType = property.GetValue(activeCharacter).GetType(); }catch { thisType = null; }
            if (property!=null && thisType !=null && thisType == typeof(int)){
                heroStats[index].text += property.Name + ": " + property.GetValue(activeCharacter) + n;
                count++;
                if (count >= 15) {
                    count = 0;
                    index++;
                }
            }
        }
    }

    public void ListMoveClicked(Move move) {
        Character c = activeCharacter;
        if (c.activeMoves.Count >= 3) {
            print("Max Moves");
            return;
        }
        if (!move.moveWeaponType.HasFlag(c.weapon.weaponType)) {
            print("Wrong Weapon Type");
            return;
        }
        Move newMove = Instantiate(move);
        newMove.name = move.name;
        c.activeMoves.Add(newMove);
        Helper.UpdateAllUI();
    }

    public void ActiveMoveClicked(Move move) {
        print("Active List : " + move);
        Character c = activeCharacter;
        c.activeMoves.Remove(move);
        Helper.UpdateAllUI();
    }
    public void ListWeaponClicked(Weapon weapon) {
        activeCharacter.weapon.isOwned = false;
        activeCharacter.weapon = weapon;
        weapon.isOwned = true;
        Helper.UpdateAllUI();
    }
    public void ActiveWeaponClicked() {
        activeCharacter.weapon.isOwned = false;
        activeCharacter.weapon = activeCharacter.GetDefaultWeapon();
        Helper.UpdateAllUI();
    }
}
