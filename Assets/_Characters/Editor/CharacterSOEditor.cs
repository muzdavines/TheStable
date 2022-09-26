using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Character))]
public class CharacterSOEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Fix Gearset")) {
            FixGear();
        }
    }

    void FixGear() {
        var t = target as Character;
        t.myGearSet = Resources.Load<CharacterGearSet>("GearSets/" + t.archetype.ToString());
    }
}