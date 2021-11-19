using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionFinalDetails : MonoBehaviour
{
    public bool successful;
    public List<string> narrative = new List<string>();
    public int goldReward;
    public Finance.Business businessReward;
    public Move moveReward;
    public float finalMod;
    void Start()
    {
        DontDestroyOnLoad(gameObject);

    }

    
}
