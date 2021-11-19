using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public List<Image> dots;
    public int currentDots;
    public Image armor;
    public List<Image> currentAction;
    public Image stamina, balance, mind, health;
    private void Start() {
        SetCurrentAction(-1, Color.white);
    }
    public void RemoveDots(int num) {
        currentDots -= num;
        currentDots = Mathf.Clamp(currentDots, 0, 100);
        print("Current Dots "+currentDots);
        SetDots(currentDots);
    }
    public void SetDots(int health) {
        currentDots = health;
        print("#HealthBar# Trying to Set " + health + " dots. RETURNING FIX LATER");
        return;
        for (int i = 0; i < health; i++) {
            dots[i].gameObject.SetActive(true);
        }
        for (int x = health; x<dots.Count; x++) {
            dots[x].gameObject.SetActive(false);
        }
    }
    public void SetArmor(float pct) {
        armor.fillAmount = pct;
    }

    public void SetCurrentAction(int action, Color color) {
        foreach (Image i in currentAction) {
            i.gameObject.SetActive(false);
            i.color = Color.white;
        }
        if (action < 0) {
            return;
        }
        currentAction[action].gameObject.SetActive(true);
        currentAction[action].color = color;
    }

    public void Hide(bool shouldHide) {
        armor.enabled = !shouldHide;
        foreach (Image i in dots) {
            i.enabled = !shouldHide;
        }
        foreach (Image c in currentAction) {
            c.enabled = !shouldHide;
        }
        stamina.enabled = !shouldHide;
        mind.enabled = !shouldHide;
        balance.enabled = !shouldHide;
        health.enabled = !shouldHide;
    }


    public void IntentIs(Combat.Intent intent, Combat.Modifier mod) {
        Color color = Color.white;
        switch (mod) {
            case Combat.Modifier.Full:
                color = Color.red;
                break;
            case Combat.Modifier.Defensive:
                color = Color.blue;
                break;
        }
        SetCurrentAction((int)intent, color);
    }

    public void SetMeters(CharacterHealth h) {
        stamina.fillAmount = h.stamina / h.maxStamina;
        mind.fillAmount = h.mind / h.maxMind;
        balance.fillAmount = h.balance / h.maxBalance;
        health.fillAmount = h.Health / h.MaxHealth;
    }
}
