using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StableMainMenu : MonoBehaviour
{
    public Button stableContracts;

    public void StableContract() {
        stableContracts.onClick.Invoke();
    }
}
