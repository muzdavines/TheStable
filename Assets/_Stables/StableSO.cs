using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class StableSO : ScriptableObject {
    [SerializeField]
    public Stable stable;
    public void CreateHero() {
        Debug.Log("Create");
       
    }
    public Character.Archetype createType;
    public int tier;
} 
