using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DailyPopup : MonoBehaviour
{
    public Text text;
    public GameObject panel;

    public void Start() {
        panel.gameObject.SetActive(false);
    }
    public void Popup(string s) {
        text.text = s;
        StartCoroutine(Display());
        panel.gameObject.SetActive(true);
    }

    IEnumerator Display() {
        yield return new WaitForSeconds(4.0f);
        panel.gameObject.SetActive(false);

    }
}
