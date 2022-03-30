using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu]
public class StableSO : ScriptableObject {
    [SerializeField]
    public Stable stable;
#if UNITY_EDITOR
    public void CreateHero() {
        Debug.Log("Create");
        string path = "Assets/_Characters/Resources/" + stable.stableName;
        if (!AssetDatabase.IsValidFolder(path)) {
            AssetDatabase.CreateFolder("Assets/_Characters/Resources", stable.stableName);
        }
        Character thisHero = new Character();
        tier = Mathf.Clamp(tier, 1, 4);
        thisHero.name = Names.Warrior[Random.Range(0, Names.Warrior.Length)];
        thisHero.GenerateCharacter(createType, tier);
        thisHero.currentPosition = Position.NA;
        stable.heroes.Add(thisHero);
        AssetDatabase.CreateAsset(thisHero, "Assets/_Characters/Resources/" + stable.stableName + "/" + thisHero.name + ".asset");
        AssetDatabase.SaveAssets();
    }
    public void InitStable() {
        foreach (Character c in stable.heroes) {
            c.activeForNextMission = false;
            c.activeInLineup = false;
        }
    }
#endif
    public Character.Archetype createType;
    public int tier;
} 
