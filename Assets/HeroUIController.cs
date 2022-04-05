using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUIController : MonoBehaviour
{
    public HeroFrame[] heroes;
    public Coach myCoach;
    public void Init(Coach _myCoach) {
        myCoach = _myCoach;
        for (int x = 0; x < myCoach.players.Length; x++) {
            heroes[x].Init(myCoach.players[x]);
        }
    }

}

public enum PlayStyle { Play, Fight }
