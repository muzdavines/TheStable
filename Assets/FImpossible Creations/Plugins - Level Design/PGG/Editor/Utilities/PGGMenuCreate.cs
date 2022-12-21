#if UNITY_EDITOR

using FIMSpace.Generating;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FIMSpace.Hidden
{
    public static class PGGMenuCreate
    {

        [MenuItem("GameObject/Generators/Grid Painter", false, 1)]
        static void CreateGridPainterObject()
        {
            GameObject go = new GameObject();
            go.name = "Grid Painter";
            go.AddComponent<GridPainter>();

            PositionNewObject(go);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Generators/Build Planner Executor", false, 2)]
        static void CreatePlannerExecutorObject()
        {
            GameObject go = new GameObject();
            go.name = "Build Planner Executor";
            go.AddComponent<BuildPlannerExecutor>();

            PositionNewObject(go);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }


        [MenuItem("GameObject/Generators/_________________", false, 1000)]
        static void CreateSeparator() { }


        [MenuItem("GameObject/Generators/Stamp Emitter", false, 1002)]
        static void CreateEmitterObject()
        {
            GameObject go = new GameObject();
            go.name = "Multi-Stamp Emitter";
            go.AddComponent<ObjectStampEmitter>();

            PositionNewObject(go);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Generators/Multi Stamp Emitter", false, 1003)]
        static void CreateMultiEmitterObject()
        {
            GameObject go = new GameObject();
            go.name = "Multi-Stamp Emitter";
            go.AddComponent<ObjectStampMultiEmitter>();

            PositionNewObject(go);

            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }


        static void PositionNewObject(GameObject go)
        {
            var sceneCamera = SceneView.lastActiveSceneView.camera;

            if (sceneCamera != null)
                go.transform.position = sceneCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            else
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
            }
        }
    }
}

#endif