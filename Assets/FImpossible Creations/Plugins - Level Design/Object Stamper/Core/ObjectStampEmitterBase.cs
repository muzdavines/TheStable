#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using FIMSpace.Generating;
using UnityEngine;

public abstract class ObjectStampEmitterBase : MonoBehaviour
{

    public bool SpawnOnStart = true;
    public bool RandomizeOnStart = true;
    public bool RaycastSpawn = true;
    public bool UseRestrictions = true;
    [Range(0, 32)] public int MaxRetryAttempts = 6;
    public bool SpawnEvenRetriesFails = false;
    [Tooltip("Spawned objects will not be attached to stamp emitter transform but to it's parent transform or no parent")]
    public bool AlwaysDetachSpawned = false;

    [HideInInspector] public bool _editorDrawSpawnSettings = true;

    /// <summary> Variable used by emitter when spawning objects to share computed data </summary>
    protected RaycastHit spawningRaycast;
    /// <summary> Variable used by emitter when spawning objects to share computed data </summary>
    protected OStamperSet.PlacementVolumeRaycastingData spawningVolume;
    /// <summary> Variable used by emitter when spawning objects to share computed data </summary>
    protected OStamperSet.RaycastingRestrictionsCheckResult spawningResult;


    protected abstract OStamperSet GetStamperSet();
    protected abstract ObjectStamperEmittedInfo GetSpawnInfo();
    public abstract void SpawnIfNotEmittedYet();


    protected Vector3 GetRayOrigin(bool local = true)
    { return GetStamperSet().GetRayOrigin(GetSpawnInfo(), transform, local); }

    protected Vector3 GetCastVector(bool local = true)
    { return GetStamperSet().GetCastVector(GetSpawnInfo(), transform, local); }


    /// <summary>
    /// Calls InstatiatePrefab if requirements met 
    /// [] Used: RaycastSpawn, retry attempts on SpawnTryFindPlace
    /// For physical simulation, call IG_CallAfterGenerated(); after SpawnEmitPrefab!
    /// </summary>
    public GameObject SpawnEmitPrefab(OStamperSet set = null)
    {
        if (set == null) set = GetStamperSet();

        if (RaycastSpawn) // Raycasted placement for spawned prefabs
        {
            for (int i = 0; i < 1 + MaxRetryAttempts; i++) // Trying to find place for spawning
            {
                SpawnTryFindRaycastedPlace(set);
                if (spawningResult.allow) break;
            }

            if (spawningResult.allow == false) // Failed to find good place to spawn
            {
                if (SpawnEvenRetriesFails == false) // If emitter is allowed to spawn target object even if no place to spawn found
                {
                    return null;
                }
            }
        }

        if (spawningResult.allow == false || RaycastSpawn == false) // Spawn in emitter position and rotation
        {
            return InternalInstatiatePrefab(false);
        }
        else // Alignment on raycasted founded place for the prefab
        {
            spawningRaycast = spawningResult.originHit;
            return InternalInstatiatePrefab(true);
        }
    }


    /// <summary>
    /// Use spawningVolume and spawningResult for computations
    /// Use spawningVolume.customPoint for custom spawn position
    /// </summary>
    protected virtual GameObject InternalInstatiatePrefab(bool raycasted, bool setParent = true)
    {
        Transform targetParent = transform;
        if (AlwaysDetachSpawned) targetParent = transform.parent;

        if (raycasted)
        {
            return GetSpawnInfo().Spawn(transform, targetParent, spawningRaycast, spawningVolume.customPoint, setParent);
        }
        else
        {
            return GetSpawnInfo().Spawn(transform, targetParent, null, null, setParent);
        }
    }


    /// <summary>
    /// Uses spawningVolume and spawningResult for computations and modifies them
    /// </summary>
    protected virtual void SpawnTryFindRaycastedPlace(OStamperSet set)
    {
        Physics.SyncTransforms();

        spawningVolume = set.GetRaycastingVolumeFor(GetSpawnInfo(), transform);

        if (UseRestrictions == false) // Raycast without restrictions checking
        {
            spawningResult = new OStamperSet.RaycastingRestrictionsCheckResult(true, "", spawningVolume.mainHit, spawningVolume.customPoint);
        }
        else // Restrictions check on raycasted
        {
            spawningResult = set.CheckRestrictionsOn(spawningVolume);

            if (spawningResult.allow == false)
            {
                var overlapCheckResult = set.CheckOverlapOnFullLineCast(GetSpawnInfo(), spawningVolume);

                if (overlapCheckResult.allow)
                {
                    spawningResult = overlapCheckResult;
                    spawningRaycast = overlapCheckResult.originHit;
                    spawningVolume.customPoint = overlapCheckResult.targetPosition;
                }
                else
                {
#if UNITY_EDITOR
                    //if (Application.isPlaying == false) Debug.Log("Not allowed position spawn override! " + spawningResult.info);
#endif
                }
            }
        }
    }

}



#if UNITY_EDITOR
[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(ObjectStampEmitterBase))]
public class ObjectsStampEmitterBaseEditor : UnityEditor.Editor
{
    public ObjectStampEmitterBase BaseGet { get { if (_baseget == null) _baseget = (ObjectStampEmitterBase)target; return _baseget; } }
    private ObjectStampEmitterBase _baseget;
    protected SerializedProperty sp_SpawnOnStart;
    protected bool repaint = false;


    protected virtual void OnEnable()
    {
        sp_SpawnOnStart = serializedObject.FindProperty("SpawnOnStart");
    }


    public override void OnInspectorGUI()
    {
#if UNITY_EDITOR
        FGUI_Inspector.LastGameObjectSelected = BaseGet.gameObject;

        serializedObject.Update();

        DrawProperties();

        serializedObject.ApplyModifiedProperties();

        if (repaint)
        {
            SceneView.RepaintAll();
            repaint = false;
        }
#endif
    }


    protected virtual void DrawProperties()
    {
#if UNITY_EDITOR
        FGUI_Inspector.FoldHeaderStart(ref BaseGet._editorDrawSpawnSettings, "Main Spawning Parameters", FGUI_Resources.BGInBoxStyle, FGUI_Resources.Tex_GearSetup, 20);

        if (BaseGet._editorDrawSpawnSettings)
        {
            GUILayout.Space(4);
            SerializedProperty sp = sp_SpawnOnStart.Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.PropertyField(sp);
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 122;
            sp.NextVisible(false); if (BaseGet.SpawnOnStart) EditorGUILayout.PropertyField(sp);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 98;
            sp.NextVisible(false); EditorGUILayout.PropertyField(sp);
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 100;
            sp.NextVisible(false); if (BaseGet.RaycastSpawn) EditorGUILayout.PropertyField(sp);
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 130;
            sp.NextVisible(false); if (BaseGet.RaycastSpawn && BaseGet.UseRestrictions) EditorGUILayout.PropertyField(sp);
            EditorGUIUtility.labelWidth = 160;
            sp.NextVisible(false); 
            if (BaseGet.RaycastSpawn && BaseGet.UseRestrictions)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 150;
                EditorGUILayout.PropertyField(sp);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = 0;

            _DrawLastProperties();
        }

        GUILayout.EndVertical();
#endif
    }

    protected virtual void _DrawLastProperties()
    {

    }

}
#endif
