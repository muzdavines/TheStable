using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SportIntroController : MonoBehaviour
{
    public Transform[] homeSpawn;
    public Transform[] awaySpawn;
    public MatchController controller;
    public void PlaceHeroes() {
        controller = FindObjectOfType<MatchController>();
        int homeCount = 0;
        int awayCount = 0;
        foreach (StableCombatChar p in controller.homeCoach.players) {
            p.IntroState(homeSpawn[homeCount++]);
        }
        foreach (StableCombatChar p in controller.awayCoach.players) {
            p.IntroState(awaySpawn[awayCount++]);
        }
    }
    public void Kickoff() {
        controller.Kickoff();
    }
}
