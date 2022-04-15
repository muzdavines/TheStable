using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SummonFireGolemEffect : MonoBehaviour
{
    [SerializeField]
    public StableDamage damage;
    bool init = false;
    StableCombatChar caster;

    public void Init(StableCombatChar _caster, StableDamage _damage) {
        caster = _caster;
        damage = _damage;
        init = true;
        GameObject spawn = Instantiate<GameObject>(Resources.Load<GameObject>("SCUnit2"), transform.position, transform.rotation);
        spawn.GetComponent<NavMeshAgent>().ResetPath();
        Character thisChar = Resources.Load<Character>("FireGolem");
        var scc = spawn.GetComponent<StableCombatChar>();
        scc.myCharacter = Instantiate(thisChar);
        scc.team = caster.team;
        scc.fieldSport = true;
        scc.fieldPosition = Position.STC;
        scc.GetComponent<SCModelSelector>().Init(1, 2, false);
        scc.transform.position = _caster.transform.position + new Vector3(1, 0, 0);
        scc.Init();
        scc.playStyle = PlayStyle.Fight;
        var familiar = scc.gameObject.AddComponent<FireGolemFamiliar>();
        familiar.owner = caster;
        Destroy(gameObject, 5f);
    }
}
