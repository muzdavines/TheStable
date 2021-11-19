using CoverShooter;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CharacterCreator : EditorWindow {
    
    public GameObject prototype;
    public GameObject player;
    public Transform rightHand;
    
    public GameObject weapon;

    [MenuItem("Laser/Player Creator")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(CharacterCreator));
    }

    private void OnGUI() {
        prototype = (GameObject)EditorGUI.ObjectField(new Rect(3, 50, position.width - 6, 20), "Prototype: ", prototype, typeof(GameObject));
        player = (GameObject)EditorGUI.ObjectField(new Rect(3, 100, position.width - 6, 20),"Player: ",player,typeof(GameObject) );
        weapon = (GameObject)EditorGUI.ObjectField(new Rect(3, 150, position.width - 6, 20), "Weapon: ", weapon, typeof(GameObject));
        rightHand = (Transform)EditorGUI.ObjectField(new Rect(3, 200, position.width - 6, 20), "Right Hand: ", rightHand, typeof(Transform));
        if (GUI.Button(new Rect(3, 300, position.width - 6, 20), "Create")) {
            if (player == null) { Debug.Log("No Player Object Set."); return; }
            Create();

        }

       
        //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
       // DisplayArray();
        
    }

   

    void DisplayArray() {
        Debug.Log("DisplayArray");
        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty("weapons");

        EditorGUILayout.PropertyField(serialProp, true);
        serialObj.ApplyModifiedProperties();
        Debug.Log("Modified");
    }

    void Create() {
        
        var components = new List<Component>();
        foreach (var component in prototype.GetComponents<Component>()) {
            Debug.Log(component.GetType());
            UnityEditorInternal.ComponentUtility.CopyComponent(component);
            if (player.GetComponent(component.GetType())) { continue; }
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(player);
        }
        player.GetComponent<CharacterMotor>().Grenade.Left = null;
        player.GetComponent<CharacterMotor>().Grenade.Right = null;
        
        
            GameObject w = Instantiate<GameObject>(weapon);
            w.transform.parent = rightHand;
            w.transform.localPosition = Vector3.zero;
            w.transform.rotation = Quaternion.identity;
            
            
        
        WeaponDescription[] playerWeapons = new WeaponDescription[1];
        
        
        playerWeapons[0] = new WeaponDescription() { RightItem = w };
        
        player.GetComponent<CharacterInventory>().Weapons = playerWeapons;

    }
}