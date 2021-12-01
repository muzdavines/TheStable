using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzPanelController : MonoBehaviour {
    public List<BuzzStageDisplay> stages = new List<BuzzStageDisplay>();
    public CanvasGroup group;
    public float fadeSpeed = 1;
    void Start() {
        Reset();
        foreach (var s in stages) {
            s.gameObject.SetActive(false);
        }
    }
    public void Display(Roll player, Roll other, int index) {
        StartCoroutine(FadeIn());
        if (index>= stages.Count) { print("Not enough stages, Buzz");return; }
        stages[index].gameObject.SetActive(true);
        stages[index].Display(player, other);
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

