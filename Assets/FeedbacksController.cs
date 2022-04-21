using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbacksController : MonoBehaviour
{
    public MMFeedbacks goOnRun;
    public MMFeedbacks sendOnRun;
    public MMFeedbacks takeDamage;
    public MMFeedbacks playerHasBall;
    public MMFeedbacks shotAccuracy;
    public MMFeedbacks specialAbil;
    public void Init(StableCombatChar thisChar) {
        thisChar.goOnRun = goOnRun;
        thisChar.sendOnRun = sendOnRun;
        thisChar.takeDamage = takeDamage;
        thisChar.playerHasBall = playerHasBall;
        thisChar.shotAccuracy = shotAccuracy;
        thisChar.specialAbil = specialAbil;
}
}
