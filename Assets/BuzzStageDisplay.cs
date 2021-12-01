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
    public void Display(Roll player, Roll other) {
        playerRol = player;
        otherRol = other;
        print("Display called " + player.total + "  "+player.max + "  "+player.total/player.max);
        StartCoroutine(DisplayScore(playerImage, playerText, player.total, player.max, 4f));
        StartCoroutine(DisplayScore(otherImage, otherText, other.total, other.max, 1f));
    }

    public IEnumerator DisplayScore(Image thisImage, TextMeshProUGUI thisText, float thisScore, float thisMax, float delay = 0f) {
        yield return new WaitForSeconds(delay);
        float targetFill = thisScore / thisMax;
        while (thisImage.fillAmount < targetFill && thisImage.fillAmount < 1) {
            thisImage.fillAmount += Time.deltaTime * speed;
            yield return null;
        }
        thisImage.fillAmount = thisScore / thisMax;
        thisText.text = thisScore.ToString("F0");
    }
    public void Start() {
        Reset();
    }
    public void Reset() {
        playerImage.fillAmount = 0;
        otherImage.fillAmount = 0;
        playerText.text = "";
        otherText.text = "";
    }
}
