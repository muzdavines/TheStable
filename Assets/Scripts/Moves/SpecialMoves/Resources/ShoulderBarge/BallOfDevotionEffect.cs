using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOfDevotionEffect : MonoBehaviour {
    private StableCombatChar passer;
    private float passTime;
    private bool stopRunning;
    public void Init(StableCombatChar thisChar) {
        Debug.Log("#BallOfDevotion#Init Effect");
        passer = thisChar;
        passTime = Time.time;
    }

    void Update() {
        if (stopRunning) {
            return;
        }
        if (Time.time > passTime + 10) {
            Destroy(this);
            return;
        }
        if (passer.ball.holder == null) {
            return;
        }
        if (passer.ball.holder == passer) {
            return;
        }

        if (passer.ball.holder.team == passer.team) {
            Destroy(this);
            return;
        }
        
        stopRunning = true;
        Activate();
    }

    public void Activate() {
        passer.ball.holder.TakeDamage(new StableDamage() { health = 4, mind = 50, isKnockdown = true }, passer, true);
        var effect = Instantiate(Resources.Load<GameObject>("BallOfDevotionEffect"));
        effect.transform.position = passer.ball.transform.position;
        Destroy(effect, 5f);
        Destroy(this, .1f);
    }
}
