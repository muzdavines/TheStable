using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
[CustomEditor(typeof(StableSO))]
public class StableSOEditor : Editor {
    public VisualTreeAsset inspectorXML;
    public override VisualElement CreateInspectorGUI() {
        //return base.CreateInspectorGUI();
        var root = inspectorXML.CloneTree();
        root.Query<Button>("createhero").First().clicked += CreateHero;

        return root;
    }

    public void CreateHero() {
        StableSO s = target as StableSO;
        s.CreateHero();
    }

}
