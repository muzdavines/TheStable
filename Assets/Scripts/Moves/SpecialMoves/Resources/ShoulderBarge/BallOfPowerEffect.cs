using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOfPowerEffect : MonoBehaviour {
    private StableCombatChar passer;
    private float passTime;
    private bool stopRunning;
    public void Init(StableCombatChar thisChar) {
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

        if (passer.ball.holder.team != passer.team) {
            Destroy(this);
            return;
        }
        if (passer.ball.holder == passer) {
            return;
        }
        stopRunning = true;
        Activate();
    }

    public void Activate() {
        passer.ball.holder.AbilityBuff(7, .5f,
            new CharacterAttribute[] {
                CharacterAttribute.carrying, CharacterAttribute.passing, CharacterAttribute.shooting,
                CharacterAttribute.tackling
            });
        var effect = Instantiate(Resources.Load<GameObject>("BallOfPowerEffect"));
        effect.transform.position = passer.ball.transform.position;
        Destroy(effect, 5f);
        Destroy(this, .1f);
    }
}
