using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatTestController))]

public class CombatEditor : Editor
{
    enum displayFieldType { DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields }
    displayFieldType DisplayFieldType;

    CombatTestController t;
    SerializedObject GetTarget;
    List<CombatTestController.CombatCharacter>[] ThisList;
    int ListSize;
    bool initialized = false;
    void OnEnable() {
       
        //Debug.Log("Enable");
        Repaint();

    }  

    void Refresh() {
        
    }

    public override void OnInspectorGUI() {
        //Update our list
        t = (CombatTestController)target;
        GetTarget = new SerializedObject(t);
        DrawDefaultInspector();
        ThisList = (GetTarget.targetObject as CombatTestController).teams;
        if (ThisList == null || ThisList.Length == 0) { EditorGUILayout.LabelField("Array Empty"); return; }
        for (int i = 0; i < ThisList.Length; i++) {
            UIHelp.DrawUILine(Color.red);
            EditorGUILayout.LabelField("Team " + (i + 1));
            for (int x = 0; x < ThisList[i].Count; x++) {
                
                CombatTestController.CombatCharacter c = ThisList[i][x];
                EditorGUILayout.TextField("Name: ", c.character.name);
                EditorGUILayout.IntField("Team: ", c.team);
                EditorGUILayout.FloatField("Melee: ", c.character.melee);
                EditorGUILayout.EnumPopup("Intent: ", c.intent);
                EditorGUILayout.EnumPopup("Modifier: ", c.modifier);
                EditorGUILayout.EnumPopup("State: ", c.state);
                EditorGUILayout.LabelField("Modifiers",EditorStyles.boldLabel);
                EditorGUILayout.FloatField("Damage Deal: ", c.damageDealtMod);
                EditorGUILayout.FloatField("Damage Taken: ", c.damageDealtMod);
                EditorGUILayout.FloatField("Parry: ", c.damageDealtMod);
                EditorGUILayout.FloatField("Dodge: ", c.damageDealtMod);
                EditorGUILayout.FloatField("Stamina: ", c.damageDealtMod);
                if (c.charTarget != null) {
                    EditorGUILayout.TextField("Target: ", c.charTarget.name);
                }
                EditorGUILayout.IntField("Target Index: ", c.charTargetIndex);
                EditorGUILayout.FloatField("Priority: ", c.priority);
                EditorGUILayout.Space();
                UIHelp.DrawUILine(Color.black, 1);
            }
        }
        
    }

       
 }



public static class UIHelp{
public static void DrawUILine(Color color, int thickness = 2, int padding = 10) {
    Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
    r.height = thickness;
    r.y += padding / 2;
    r.x -= 2;
    r.width += 6;
    EditorGUI.DrawRect(r, color);
}
}
