using UnityEngine;
using UnityEditor;
using com.ootii.Helpers;
using com.ootii.Input;
using System.IO;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[CanEditMultipleObjects]
[CustomEditor(typeof(UnityInputSystemSource))]
public class UnityInputSystemSourceEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private UnityInputSystemSource mTarget;
    private SerializedObject mTargetSO;

#if ENABLE_INPUT_SYSTEM

    // Activators that can be selected
    private string[] mActivators = new string[] { "None", "Left Mouse Button", "Right Mouse Button", "Left or Right Mouse Button", "Middle Mouse Button" };

    // Determines if we remove existing entries
    private bool mRemoveExistingEntries = false;

#endif 

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (UnityInputSystemSource)target;
        mTargetSO = new SerializedObject(target);
    }

    /// <summary>
    /// This function is called when the scriptable object goes out of scope.
    /// </summary>
    private void OnDisable()
    {
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Input System Source");

        EditorHelper.DrawInspectorDescription("Input source for Unity's new Input System package. You must setup Unity's Player Input component.", MessageType.None);

        GUILayout.Space(5);

#if ENABLE_INPUT_SYSTEM

        bool lNewIsPlayerInputEnabled = EditorGUILayout.Toggle(new GUIContent("Is Input Enabled", "Determines if we'll get input from the mouse, keyboard, and gamepad."), mTarget.IsEnabled);
        if (lNewIsPlayerInputEnabled != mTarget.IsEnabled)
        {
            mIsDirty = true;
            mTarget.IsEnabled = lNewIsPlayerInputEnabled;
        }

        GUILayout.Space(5f);

        if (EditorHelper.ObjectField<PlayerInput>("Player Input", "Unity's Player Input component", mTarget.PlayerInput, mTarget))
        {
            mIsDirty = true;
            mTarget.PlayerInput = EditorHelper.FieldObjectValue as PlayerInput;
        }

        if (EditorHelper.TextField("Move Action", "Name of the Player Input 'move' action", mTarget.MoveAction, mTarget))
        {
            mIsDirty = true;
            mTarget.MoveAction = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Look Action", "Name of the Player Input 'look' action", mTarget.LookAction, mTarget))
        {
            mIsDirty = true;
            mTarget.LookAction = EditorHelper.FieldStringValue;
        }

        GUILayout.Space(5f);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("View Activator", "Determines what button enables viewing."), GUILayout.Width(EditorGUIUtility.labelWidth));
        int lNewViewActivator = EditorGUILayout.Popup(mTarget.ViewActivator, mActivators);
        if (lNewViewActivator != mTarget.ViewActivator)
        {
            mIsDirty = true;
            mTarget.ViewActivator = lNewViewActivator;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        // Show the events
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Options"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowOptions = !mTarget.EditorShowOptions;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent(mTarget.EditorShowOptions ? "-" : "+"), EditorStyles.boldLabel))
        {
            mTarget.EditorShowOptions = !mTarget.EditorShowOptions;
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical(EditorHelper.GroupBox);
        EditorHelper.DrawInspectorDescription("Tools to help manage the Input System Actions.", MessageType.None);

        if (mTarget.EditorShowOptions)
        {
            GUILayout.BeginVertical(EditorHelper.Box);

            mRemoveExistingEntries = EditorGUILayout.Toggle(new GUIContent("Replace Existing Entries", "Determines if remove existing entries before adding."), mRemoveExistingEntries);

            GUILayout.Space(5f);

            if (GUILayout.Button("Create Input Actions", EditorStyles.miniButton, GUILayout.MinWidth(30f)))
            {
                CreateInputEntries("Player", mRemoveExistingEntries);
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndVertical();

#else

        EditorHelper.DrawInspectorDescription("Unity's new Input System package is not installed. Use Unity's Package Manager to install it.", MessageType.Warning);

#endif

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mTarget);

            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            // Pushes the values back to the runtime so it has the changes
            mTargetSO.ApplyModifiedProperties();

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }

#if ENABLE_INPUT_SYSTEM

    /// <summary>
    /// Generates all the input entries that the ootii assets need.
    /// </summary>
    /// <param name="rGroup">Group we'll create the entries under</param>
    protected void CreateInputEntries(string rGroup, bool rRemoveExistingEntries)
    {
        if (mTarget == null || mTarget._PlayerInput == null) 
        { 
            return; 
        }


        InputActionAsset lInputActions = mTarget._PlayerInput.actions;
        if (lInputActions == null)
        {
            return;
        }

        string lPath = AssetDatabase.GetAssetPath(lInputActions);

        InputActionMap lActionMap = lInputActions.FindActionMap(rGroup);
        if (lActionMap == null)
        {
            return;
        }

        CreateInputEntry(lActionMap, "Jump", InputActionType.Button, "<Keyboard>/space", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Run", InputActionType.Button, "<Keyboard>/shift", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Change Stance", InputActionType.Button, "<Keyboard>/t", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Interact", InputActionType.Button, "<Keyboard>/f", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Cover Toggle", InputActionType.Button, "<Keyboard>/m", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "StrafeLeft", InputActionType.Button, "<Keyboard>/m", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "StrafeRight", InputActionType.Button, "<Keyboard>/m", "Keyboard&Mouse", "Button", rRemoveExistingEntries);

        CreateInputEntry(lActionMap, "Inventory Toggle", InputActionType.Button, "<Keyboard>/0", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateCompositeInputEntry(lActionMap, "Inventory Shift", InputActionType.Value, "<Keyboard>/minus", "<Keyboard>/equals", "Keyboard&Mouse", "Button", rRemoveExistingEntries);

        CreateInputEntry(lActionMap, "Combat Attack", InputActionType.Button, "<Mouse>/leftButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Combat Block", InputActionType.Button, "<Keyboard>/leftAlt", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Combat Lock", InputActionType.Button, "<Keyboard>/t", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Combat Reload", InputActionType.Button, "<Keyboard>/r", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Combat Throw", InputActionType.Button, "<Keyboard>/f", "Keyboard&Mouse", "Button", rRemoveExistingEntries);

        CreateInputEntry(lActionMap, "Spell Casting Cast", InputActionType.Button, "<Mouse>/leftButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Spell Casting Continue", InputActionType.Button, "<Mouse>/leftButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Spell Casting Cancel", InputActionType.Button, "<Keyboard>/escape", "Keyboard&Mouse", "Button", rRemoveExistingEntries);

        CreateInputEntry(lActionMap, "Camera Rotate Character", InputActionType.PassThrough, "<Mouse>/rightButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Zoom", InputActionType.PassThrough, "<Mouse>/scroll", "Keyboard&Mouse", "Vector2", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Aim", InputActionType.PassThrough, "<Mouse>/rightButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Follow", InputActionType.Button, "<Keyboard>/space", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Grip", InputActionType.PassThrough, "<Mouse>/leftButton", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Forward", InputActionType.Button, "<Keyboard>/w", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Back", InputActionType.Button, "<Keyboard>/s", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Left", InputActionType.Button, "<Keyboard>/a", "Keyboard&Mouse", "Button", rRemoveExistingEntries);
        CreateInputEntry(lActionMap, "Camera Right", InputActionType.Button, "<Keyboard>/d", "Keyboard&Mouse", "Button", rRemoveExistingEntries);

        EditorUtility.SetDirty(lInputActions);
        
        // Update JSON.
        string lNewJSON = lInputActions.ToJson();
        var lOldJSON = File.ReadAllText(lPath);
        if (lNewJSON != lOldJSON)
        {
            // This is a hack due to a bug in the creating of actions that doesn't set the name
            // for composite actions. We'll force the name here
            int lIndex = lNewJSON.IndexOf("\"action\": \"Inventory Shift\"");
            if (lIndex >= 0)
            {
                string lTempJSON = lNewJSON.Substring(0, lIndex);
                int lBracketIndex = lTempJSON.LastIndexOf("{");

                lIndex = lTempJSON.LastIndexOf("\"name\": \"\"");
                if (lIndex >= 0 && lIndex > lBracketIndex)
                {
                    lNewJSON = lNewJSON.Remove(lIndex, 10);
                    lNewJSON = lNewJSON.Insert(lIndex, "\"name\": \"1DAxis\"");
                }
            }

            // Make path relative to project folder.
            string lProjectPath = Application.dataPath;
            if (lPath.StartsWith(lProjectPath) && lPath.Length > lProjectPath.Length && (lPath[lProjectPath.Length] == '/' || lPath[lProjectPath.Length] == '\\'))
            {
                lPath = lPath.Substring(0, lProjectPath.Length + 1);
            }

            AssetDatabase.MakeEditable(lPath);

            // Write out the new settings and reload the asset
            File.WriteAllText(lPath, lNewJSON);
            AssetDatabase.ImportAsset(lPath);
        }

        //EditorUtility.DisplayDialog("Input Source", "Entries created. Close Unity Input Action Editor and re-open.", "ok");
    }

    /// <summary>
    /// Generates a specific input entry.
    /// </summary>
    /// <param name="rMap"></param>
    /// <param name="rAction"></param>
    /// <param name="rKey"></param>
    protected void CreateInputEntry(InputActionMap rMap, string rAction, InputActionType rType, string rBinding, string rGroups, string rLayout = "", bool rReplace = false)
    {
        InputAction lAction = rMap.FindAction(rAction);
        if (lAction != null) 
        {
            if (!rReplace) { return; }
            lAction.RemoveAction();
        }

        lAction = rMap.AddAction(rAction, rType, rBinding, "", "", rGroups, rLayout);
    }

    /// <summary>
    /// Generates a specific input entry.
    /// </summary>
    /// <param name="rMap"></param>
    /// <param name="rAction"></param>
    /// <param name="rKey"></param>
    protected void CreateCompositeInputEntry(InputActionMap rMap, string rAction, InputActionType rType, string rNegBinding, string rPosBinding, string rGroups, string rLayout = "", bool rReplace = false)
    {
        InputAction lAction = rMap.FindAction(rAction);
        if (lAction != null)
        {
            if (!rReplace) { return; }
            lAction.RemoveAction();
        }

        lAction = rMap.AddAction(name:rAction, rType);

        //Debug.Log(lAction.ToString());

        // Unity has a bug where they do not set the "name" of the composite
        // and that creates an error in thier editor.
        lAction.AddCompositeBinding("1DAxis")
            .With("negative", rNegBinding, rGroups)
            .With("positive", rPosBinding, rGroups);

        //InputBinding lBindings = new InputBinding("1DAxis", rAction, null, null, null, "xxx");
        //lAction.AddBinding(lBindings);

        //Debug.Log(lAction.ToString());
    }

#endif
}
