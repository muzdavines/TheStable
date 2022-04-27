using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuzzPanelController : MonoBehaviour {
    public List<BuzzStageDisplay> stages = new List<BuzzStageDisplay>();
    public CanvasGroup group;
    public float fadeSpeed = 1;
    public Image successBar;
    List<int> attitudes;
    void Start() {
        Reset();
        foreach (var s in stages) {
            s.gameObject.SetActive(false);
        }
        UpdateSuccess();
    }
    public void Display(Roll player, Roll other, int attitude, int index) {
        if (other.threshold <= 1) {
            return;
        }
        StartCoroutine(FadeIn());
        if (index>= stages.Count) { print("Not enough stages, Buzz");return; }
        stages[index].gameObject.SetActive(true);
        stages[index].Display(player, other);
        attitudes.Add(attitude);
    }
    public void UpdateSuccess() {
        int thisAttitude = 0;
        if (attitudes == null || attitudes.Count == 0) {
            thisAttitude = 0;
            attitudes = new List<int>();
        }
        else {
            thisAttitude = attitudes[0];
        }
        successBar.fillAmount = (.01f * thisAttitude) + .5f;
        if (successBar.fillAmount > .5f) {
            successBar.color = Color.green;
        } else {
            successBar.color = Color.red;
        }
        if (attitudes == null || attitudes.Count == 0) { }
        else {
            attitudes.RemoveAt(0);
        }
    }
    public void Reset() {
        foreach (var s in stages) {
            s.Reset();
            s.gameObject.SetActive(false);
        }
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn() {
        
        while (group.alpha < 1) {
            group.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
    IEnumerator FadeOut() {
        while (group.alpha > 0) {
            group.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}

