using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image mat;
    public int dir = 1;
    public bool shouldFade = false;
    public float fadeSpeed = 1;
    public void FadeIn() {
        dir = -1;
        shouldFade = true;

    }
    public void FadeOut() {
        dir = 1;
        shouldFade = true;
    }
    public void Update() {
        if (!shouldFade) { return; }
        float targetFloat = mat.color.a + Time.deltaTime * fadeSpeed * dir;
        mat.color = new Color(0, 0, 0, targetFloat);
        if (targetFloat > 1 || targetFloat < 0) {
            shouldFade = false;
        }
    }
}
