using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUIController : MonoBehaviour
{
    public HeroFrame[] heroes;
    public Coach myCoach;
    private void Start() {
       
    }
    public void Init(Coach _myCoach) {
        myCoach = _myCoach;
        for (int x = 0; x < myCoach.players.Length; x++) {
            heroes[x].Init(myCoach.players[x]);
            myCoach.players[x].uiController = heroes[x];
            heroes[x].gameObject.SetActive(true);
        }
        for (int z = myCoach.players.Length; z<heroes.Length; z++) {
            heroes[z].gameObject.SetActive(false);
        }
    }
    public void Init(List<StableCombatChar> _chars) {
        for (int z = 0; z < heroes.Length; z++) {
            heroes[z].gameObject.SetActive(false);
        }
        for (int x = 0; x < _chars.Count; x++) {
            heroes[x].Init(_chars[x]);
            _chars[x].uiController = heroes[x];
            heroes[x].gameObject.SetActive(true);
        }
    }


}

