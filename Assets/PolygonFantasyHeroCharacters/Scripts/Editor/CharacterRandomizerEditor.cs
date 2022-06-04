using System.Collections;
using System.Collections.Generic;
using PsychoticLab;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(CharacterRandomizer))]
public class CharacterRandomizerEditor : Editor
{
    public override void OnInspectorGUI(){

        DrawDefaultInspector();
        if (GUILayout.Button("Rogue")) {
            ((CharacterRandomizer)target).ShowRogue();
        }
        if (GUILayout.Button("Wizard")) {
            ((CharacterRandomizer)target).ShowWizard();
        }
        if (GUILayout.Button("Warrior")) {
            ((CharacterRandomizer)target).ShowWarrior();
        }
        if (GUILayout.Button("TestType")) {
            ((CharacterRandomizer)target).ShowTestType();
        }
    }
}
