using UnityEngine;
using UnityEditor;

namespace FIMSpace.Generating
{
    //public partial class ObjectsStamperWindow : EditorWindow
    //{
    //    public static ObjectsStamperWindow Get;
    //    OStamperSet tempPreset;
    //    OStamperSet projectPreset;
    //    OStamperSet selectedPreset;
    //    bool repaint = false;

    //    [MenuItem("Window/FImpossible Creations/Level Design/Objects Stamper", false, 101)]
    //    static void Init()
    //    {
    //        ObjectsStamperWindow window = (ObjectsStamperWindow)EditorWindow.GetWindow(typeof(ObjectsStamperWindow));
    //        window.titleContent = new GUIContent("Objects Stamper", Resources.Load<Texture>("SPR_ObjStamperSmall"));
    //        window.Show();
    //        if (window.tempPreset == null) window.tempPreset = CreateInstance<OStamperSet>();
    //        Get = window;
    //    }

    //    void OnGUI()
    //    {
    //        pfListScroll = EditorGUILayout.BeginScrollView(pfListScroll);

    //        Get = this;
    //        if (projectPreset == null) selectedPreset = tempPreset;
    //        else selectedPreset = projectPreset;

    //        DrawObjectsSet(selectedPreset);
    //        GUILayout.Space(3);

    //        if (repaint)
    //        {
    //            SceneView.RepaintAll();
    //            repaint = false;
    //        }

    //        EditorGUILayout.EndScrollView();
    //    }

    //}
}