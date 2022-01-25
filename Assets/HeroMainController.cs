using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroMainController : MonoBehaviour
{
    public Character activeChar;
    public TextMeshProUGUI heroName, heroType, games, goals, assists, tackles, kos, shooting, passing, tackling, carrying, melee, ranged, magic, speed, dex, agi, str;
    public StableManagementController controller;
    public GameObject panel;
    public GameObject movePanelMelee, movePanelRanged, weaponPanel, trainingPanel;
    public MoveListScrollerController moveListMelee, moveListRanged;
    public ActiveMoveListScrollerController activeMoveListMelee, activeMoveListRanged;
    public TrainingController training;
    public void Start() {
        panel.SetActive(false);
        movePanelMelee.SetActive(false);
        movePanelRanged.SetActive(false);
    }
    public void Init(Character _char) {
        activeChar = _char;
        heroName.text = activeChar.name;
        heroType.text = activeChar.archetype.ToString();
        games.text = activeChar.seasonStats.games.ToString();
        goals.text = activeChar.seasonStats.goals.ToString(); ;
        assists.text = activeChar.seasonStats.assists.ToString();
        tackles.text = activeChar.seasonStats.tackles.ToString();
        kos.text = activeChar.seasonStats.kos.ToString();
        shooting.text = activeChar.shooting.ToString();
        passing.text = activeChar.passing.ToString();
        tackling.text = activeChar.tackling.ToString();
        carrying.text = activeChar.carrying.ToString();
        melee.text = activeChar.melee.ToString();
        ranged.text = activeChar.ranged.ToString();
        magic.text = activeChar.magic.ToString();
        speed.text = activeChar.runspeed.ToString();
        dex.text = activeChar.dexterity.ToString();
        agi.text = activeChar.agility.ToString();
        str.text = activeChar.strength.ToString();
    }

    public void OpenPanel(Character _active) {
        controller.OnClick(panel);
        panel.SetActive(true);
        Init(_active);
    }

    public void OpenMovePanelMelee() {
        controller.OnClick(movePanelMelee);
        movePanelMelee.SetActive(true);
        activeMoveListMelee.Init(activeChar);
        moveListMelee.Init(activeChar);
    }
    public void OpenMovePanelRanged() {
        controller.OnClick(movePanelRanged);
        movePanelRanged.SetActive(true);
        activeMoveListRanged.Init(activeChar);
        moveListRanged.Init(activeChar);
    }

    public void OpenTraining() {
        controller.OnClick(training.gameObject);
        training.gameObject.SetActive(true);
        training.Init(activeChar);

    }

    public void ListMoveClicked(Move move) {
        Character c = activeChar;
        int currentCount = move.moveType == MoveType.Melee ? c.activeMeleeMoves.Count : c.activeRangedMoves.Count;
        if (currentCount >= 3) {
            print("Max Moves");
            return;
        }
        if (move.moveType == MoveType.Melee) {
            if (!move.moveWeaponType.HasFlag(c.meleeWeapon.weaponType)) {
                print("Wrong Weapon Type");
                return;
            }
        } else {
            if (!move.moveWeaponType.HasFlag(c.rangedWeapon.weaponType)) {
                print("Wrong Weapon Type");
                return;
            }
        }
        Move newMove = Instantiate(move);
        newMove.name = move.name;
        if (newMove.moveType == MoveType.Melee) {
            c.activeMeleeMoves.Add(newMove);
        } else {
            c.activeRangedMoves.Add(newMove);
        }
        Helper.UpdateAllUI();
    }

    public void ActiveMoveClicked(Move move) {
        print("Active List : " + move);
        Character c = activeChar;
        if (move.moveType == MoveType.Melee) {
            c.activeMeleeMoves.Remove(move);
        } else {
            c.activeRangedMoves.Remove(move);
        }
        Helper.UpdateAllUI();
    }
    public void ListWeaponClicked(Weapon weapon) {
        if (weapon.moveType == MoveType.Ranged) {
            activeChar.meleeWeapon.isOwned = false;
            activeChar.meleeWeapon = weapon;
        } else {
            activeChar.rangedWeapon.isOwned = false;
            activeChar.rangedWeapon = weapon;
        }
        weapon.isOwned = true;
        Helper.UpdateAllUI();
    }
    public void ActiveWeaponClicked(Weapon weapon) {
        if (weapon.moveType == MoveType.Melee) {
            activeChar.meleeWeapon.isOwned = false;
            activeChar.meleeWeapon = activeChar.GetDefaultMeleeWeapon();
        } else {
            activeChar.rangedWeapon.isOwned = false;
            activeChar.rangedWeapon = activeChar.GetDefaultRangedWeapon();
        }
        Helper.UpdateAllUI();
    }
}
