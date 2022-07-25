using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class NewsController : MonoBehaviour, UIElement {
    public GameObject panel;
    public NewsScrollerController newsControl;
    public TextMeshProUGUI newsNumber;
    public void OpenPanel() {
        FindObjectOfType<StableManagementController>().OnClick(panel);
        panel.SetActive(true);
        SetNews();
    }

    public void Start() {
        UpdateNumber();
    }

    public void SetNews() {
        newsControl.Start();
        UpdateNumber();
    }
    public void UpdateOnAdvance() {
        UpdateNumber();
    }

    public void UpdateNumber() {
        int x = 0;
        foreach (NewsItem n in Game.instance.news) {
            if (!n.read) {
                x++;
            }
        }
        newsNumber.text = x == 0 ? "" : "(" + x + ")";
    }
}
