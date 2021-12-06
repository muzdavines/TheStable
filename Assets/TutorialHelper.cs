using HardCodeLab.TutorialMaster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHelper : MonoBehaviour
{
   public void CheckTutorial(Tutorial thisTutorial) {
        Game.instance.CheckIfTutorialComplete(thisTutorial);
   }
    public void TutorialComplete(Tutorial thisTutorial) {
        Game.instance.TutorialComplete(thisTutorial);
    }
}
