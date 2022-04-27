using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BuzzStageDisplay : MonoBehaviour
{
    public Image playerImage, otherImage;
    public TextMeshProUGUI playerText, otherText;
    [SerializeField]
    public Roll playerRol, otherRol;
    public float speed = 5;
    IEnumerator pScore, oScore, pScoreComplete, oScoreComplete;
    bool displaying;
    public void Display(Roll player, Roll other) {
        gameObject.SetActive(true);
        displaying = true;
        playerRol = player;
        otherRol = other;
        print("Display called " + player.total + "  "+player.max + "  "+player.total/player.max);
        pScore = DisplayScore(playerImage, playerText, player.total, player.max, 4f, true);
        oScore = DisplayScore(otherImage, otherText, other.total, other.max, 1f);
        pScoreComplete = DisplayScore(playerImage, playerText, player.total, player.max, 0f, true);
        oScoreComplete = DisplayScore(otherImage, otherText, other.total, other.max, 0f);
        StartCoroutine(pScore);
        StartCoroutine(oScore);
        // pScore = StartCoroutine(DisplayScore(playerImage, playerText, player.total, player.max, 4f));
        // oScore = StartCoroutine(DisplayScore(otherImage, otherText, other.total, other.max, 1f));
    }

    public IEnumerator DisplayScore(Image thisImage, TextMeshProUGUI thisText, float thisScore, float thisMax, float delay = 0f, bool shouldUpdateSuccess = false) {
        yield return new WaitForSeconds(delay);
        float targetFill = thisScore / thisMax;
        while (thisImage.fillAmount < targetFill && thisImage.fillAmount < 1) {
            thisImage.fillAmount += Time.deltaTime * speed;
            yield return null;
        }
        thisImage.fillAmount = thisScore / thisMax;
        thisText.text = thisScore.ToString("F0");
        if (shouldUpdateSuccess) {
            displaying = false;
            FindObjectOfType<BuzzPanelController>().UpdateSuccess();
           
        }
    }
    public void Start() {
        
    }
    public void Update() {
        if (!displaying) { return; }
        if (Input.GetKeyDown(KeyCode.Space)){
            displaying = false;
            StopCoroutine(pScore);
            StopCoroutine(oScore);
            speed = 1000;
            StartCoroutine(pScoreComplete);
            StartCoroutine(oScoreComplete);
        }
    }
    public void Reset() {
        playerImage.fillAmount = 0;
        otherImage.fillAmount = 0;
        playerText.text = "";
        otherText.text = "";
        
    }
}
