using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class BlueManaEditor : EditorWindow
{

    [MenuItem("Blue Mana/CreateCharacter")]
    static void Init() {
        BlueManaEditor window = (BlueManaEditor)EditorWindow.GetWindow(typeof(BlueManaEditor));
        window.Show();
    }
    GameObject target;
    private void OnGUI() {
        target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
    }

    private void Create() {
        var body = target.AddComponent<Rigidbody>();
        body.mass = 60;
        body.isKinematic = true;
        var agent = target.AddComponent<NavMeshAgent>();
    }
}
