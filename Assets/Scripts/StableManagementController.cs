using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class StableManagementController : MonoBehaviour, UIElement
{
    /// <summary>
    /// Header UI Elements
    /// </summary>
    public Text headerActivePanel;
    public Text headerGold;
    public Text headerDate;
    public GameObject activePanel;
    public List<GameObject> lastPanels = new List<GameObject>();
    //---------------------------------


    void Start()
    {
        //if (!GameObject.FindObjectOfType<Game>()) { SceneManager.LoadScene("NewGameCreation"); return; }
        GameObject.Find("HomeButton").GetComponent<Button>().onClick.Invoke();
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Advance()
    {
        Game.instance.Advance();
    }

    public void UpdateHeader()
    {
        headerActivePanel.text = activePanel.name;
        headerGold.text = "Gold: "+Game.instance.playerStable.finance.gold.ToString();
        headerDate.text = Game.instance.gameDate.GetDateString();
        foreach (MainMenuButton b in FindObjectsOfType<MainMenuButton>()) {
            b.UpdateIndicator();
        }
    }
    public void Back()
    {
        if (lastPanels == null || lastPanels.Count == 0)
        {
            return;
        }
        activePanel.SetActive(false);
        activePanel = lastPanels.Last();
        activePanel.SetActive(true);
        lastPanels.RemoveAt(lastPanels.Count - 1);
        UpdateHeader();

    }
    public void OnClick (GameObject g)
    {
        if (activePanel != null)
        {
            
            if (g != activePanel)
            {
                activePanel.SetActive(false);
                GameObject last = activePanel;
                lastPanels.Add(last);
            }
        }
        activePanel = g;

        UpdateHeader();
        ClosePopUps();
      
    }
    void ClosePopUps() {
        foreach (PopupElement p in FindObjectsOfType<MonoBehaviour>().OfType<PopupElement>()) {
            p.Close();
        }

    }
    public void PrepForSave() {
        Game.instance.PrepForSave();
        gameObject.SendMessage("PreppedSave");
    }
    public void UpdateOnAdvance() {
        UpdateHeader();
    }
}
