using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeroEditController : MonoBehaviour
{
    public GameObject panel;
    public Character activeCharacter;
    public StableManagementController controller;
    public void OpenPanel(Character _active) {
        activeCharacter = _active;
        controller.OnClick(panel);
        panel.SetActive(true);
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
