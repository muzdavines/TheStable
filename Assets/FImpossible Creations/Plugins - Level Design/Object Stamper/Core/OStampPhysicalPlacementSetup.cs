#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace FIMSpace.Generating
{

    [System.Serializable]
    public class OStampPhysicalPlacementSetup
    {

        [Tooltip("If you want to use physical simulation placement, toggle this")]
        public bool Enabled = false;

        [Space(4)]
        [Tooltip("Minimum physical tick simulations before finishing physics placement")]
        public int MinimumIterations = 70;
        [Tooltip("If physics simulation would push object too far it will swap it back")]
        public float LimitDistanceFromOrigin = 8f;
        [Tooltip("If objects in simulation are still dynamic then don't stop the simulation but execute it until objects become calm")]
        public bool SimulateUntilStopped = true;


        public void CopySettingsFromTo(OStampPhysicalPlacementSetup from, OStampPhysicalPlacementSetup to)
        {
            to.Enabled = from.Enabled;
            to.MinimumIterations = from.MinimumIterations;
            to.LimitDistanceFromOrigin = from.LimitDistanceFromOrigin;
            to.SimulateUntilStopped = from.SimulateUntilStopped;
        }


        [SerializeField, HideInInspector]
        public bool _Editor_Foldout = false;


        /// <summary>
        /// Simulate multiple objects (cheaper than simulating per object)
        /// </summary>
        public void ProceedOn(List<GameObject> o)
        {
            if (o == null) return;
            if (o.Count == 0) return;

            DoBackupFor(o);
            PrepareSimulationList(o);

            try
            {
                if (PrepareSimulation())
                {
                    //UnityEngine.Debug.Log("Simulating " + objectsToSimulate.Count + " objects with " + _simCollideWith.Count + " obstacle objects");

                    ApplySimulation();

                    //UnityEngine.Debug.DrawRay(o[0].transform.position, Vector3.right, Color.green, 1.01f);
                }
                else
                {

                    #region Editor Code

#if UNITY_EDITOR
                    UnityEngine.Debug.Log("[Stamper Physics Simulation] No physical enviro detected! (" + o[0].name + ")");

                    Debug.DrawRay(_searchAreaCenter - Vector3.forward * _searchAreaRadius, Vector3.forward * _searchAreaRadius * 2f, Color.red, 1f);
                    Debug.DrawRay(_searchAreaCenter - Vector3.right * _searchAreaRadius, Vector3.right * _searchAreaRadius * 2f, Color.red, 1f);
                    Debug.DrawRay(_searchAreaCenter - Vector3.up * _searchAreaRadius, Vector3.up * _searchAreaRadius * 2f, Color.red, 1f);

#endif

                    #endregion

                }
            }
            catch (Exception exc)
            {
                UnityEngine.Debug.Log("[Stamper Physics Sim] Error Occured, check log below!");
                UnityEngine.Debug.LogException(exc);
                RestoreBackuped();
            }

            MovedSimulationObjects(preSimScene);
            FinishSimulation();
        }


        /// <summary>
        /// Simulate physics on single object (more expensive than simulating list of object)
        /// </summary>
        public void ProceedOn(GameObject o)
        {
            if (o == null) return;
            if (sList == null) sList = new List<GameObject>(); else sList.Clear();
            sList.Add(o);
            ProceedOn(sList);
        }



        #region Physics Simulation Utilities


        List<Collider> isolatedCollisions;
        Scene preSimScene;
        Scene simScene;
        PhysicsScene simPhysScene;
        bool preAutoSim;

        /// <summary>
        /// Returns false if no physical environment detected
        /// </summary>
        bool PrepareSimulation()
        {
            preAutoSim = Physics.autoSimulation;
            if (objectsToSimulate.Count == 0) return false;
            preSimScene = objectsToSimulate[0].gameObject.scene;

            UnloadPhysSimScene();

            isolatedCollisions = GetCollisionAround(objectsToSimulate);


            #region Restore search related stuff

            // Restore after overlap search
            for (int m = 0; m < _simMyColliders.Count; m++) _simMyColliders[m].enabled = true;

            #endregion


            if (isolatedCollisions == null) return false;
            if (isolatedCollisions.Count == 0) return false;

            simScene = GenerateSimulationScene();

            for (int c = 0; c < isolatedCollisions.Count; c++)
            {
                GameObject inst = GameObject.Instantiate(isolatedCollisions[c].gameObject);
                SceneManager.MoveGameObjectToScene(inst, simScene);
            }

            PrepareSimulationObjects();

            simPhysScene = simScene.GetPhysicsScene();

            return true;
        }


        void ApplySimulation()
        {
            Physics.autoSimulation = false;

            for (int i = 0; i < MinimumIterations; i++)
            {
                simPhysScene.Simulate(Time.fixedDeltaTime);
                OnSimulationFixedUpdate();
            }

            if (SimulateUntilStopped)
            {
                int iters = 0;

                float mul = 1f;
                mul = 1f / (float)objectsToSimulate.Count;
                if (mul < 0.4f) mul = 0.4f;

                while (CheckSimulatedObjectsVeclocity().magnitude > 0.05f * mul)
                {
                    simPhysScene.Simulate(Time.fixedDeltaTime);
                    OnSimulationFixedUpdate();

                    iters += 1;
                    if (iters > MinimumIterations + 1000)
                    {
                        break; // Safety limit
                    }
                }

                if (iters > MinimumIterations + 500)
                    UnityEngine.Debug.LogWarning("[Object Stamper Physical Placement Simulation] " + objectsToSimulate[0].name + " Physics Simulation Group required " + iters + " physics iterations. Maybe you need to tweak some physical placement settings?");

            }


        }

        void MovedSimulationObjects(Scene target)
        {
            for (int i = 0; i < objectsToSimulate.Count; i++)
            {
                if (target == preSimScene)
                {
                    objectsToSimulate[i].transform.SetParent(null, true);
                    SceneManager.MoveGameObjectToScene(objectsToSimulate[i], target);
                    var backup = backups[objectsToSimulate[i]];
                    objectsToSimulate[i].transform.SetParent(backup.Parent, true);
                }
                else
                {
                    objectsToSimulate[i].transform.SetParent(null, true);
                    SceneManager.MoveGameObjectToScene(objectsToSimulate[i], target);
                }
            }
        }

        void FinishSimulation()
        {
            Physics.autoSimulation = preAutoSim;

            if (generatedRigidbodies != null)
                for (int g = 0; g < generatedRigidbodies.Count; g++)
                {
                    FGenerators.DestroyObject(generatedRigidbodies[g]);
                }

            if (generatedColliders != null)
                for (int g = 0; g < generatedColliders.Count; g++)
                {
                    FGenerators.DestroyObject(generatedColliders[g]);
                }

            if (_mMeshNonConvexColliders != null)
                for (int g = 0; g < _mMeshNonConvexColliders.Count; g++)
                {
                    _mMeshNonConvexColliders[g].convex = false;
                }

            UnloadPhysSimScene();
        }

        void UnloadPhysSimScene()
        {
#pragma warning disable 0618
            if (simScene.isLoaded) SceneManager.UnloadScene(simScene);
#pragma warning restore 0618
        }


        public static Scene GenerateSimulationScene()
        {
            Scene s;

            #region Simulation Scene Creation

            CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                s = SceneManager.CreateScene("PhysSim", csp);
            }
            else
            {
                s = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            }
#else
            s = SceneManager.CreateScene("PhysSim", csp);
#endif

            #endregion

            return s;
        }


        public Vector3 CheckSimulatedObjectsVeclocity()
        {
            Vector3 sum = Vector3.zero;

            for (int i = 0; i < simulatingRigidbodies.Count; i++)
            {
                sum += simulatingRigidbodies[i].velocity;
            }

            return sum;
        }


        /// <summary>
        /// Checking simulated objects conditions like limit distance from origin etc.
        /// </summary>
        public void OnSimulationFixedUpdate()
        {
            for (int i = 0; i < objectsToSimulate.Count; i++)
            {
                GameObject sim = objectsToSimulate[i];
                var backup = backups[sim];
                Vector3 origin = backup.Position;

                float dist = Vector3.Distance(sim.transform.position, origin);

                if (dist > LimitDistanceFromOrigin)
                {
                    backup.RestoreBackupedTransform(sim, false);
                }
            }
        }


        /// <summary>
        /// Ensuring for collider and rigidbody presence in target simulation objects
        /// </summary>
        public void PrepareSimulationObjects()
        {
            if (_mMeshNonConvexColliders == null) _mMeshNonConvexColliders = new List<MeshCollider>(); else _mMeshNonConvexColliders.Clear();
            if (simulatingRigidbodies == null) simulatingRigidbodies = new List<Rigidbody>(); else simulatingRigidbodies.Clear();
            if (generatedRigidbodies == null) generatedRigidbodies = new List<Rigidbody>(); else generatedRigidbodies.Clear();
            if (generatedColliders == null) generatedColliders = new List<Collider>(); else generatedColliders.Clear();

            for (int i = 0; i < objectsToSimulate.Count; i++)
            {
                Collider col = objectsToSimulate[i].GetComponent<Collider>();
                if (!col) col = objectsToSimulate[i].GetComponentInChildren<Collider>();

                if ( col is MeshCollider)
                {
                    MeshCollider mCol = col as MeshCollider;
                    if ( mCol.convex == false)
                    {
                        _mMeshNonConvexColliders.Add(mCol);
                        mCol.convex = true;
                    }
                }

                if (!col)
                {
                    Mesh meshForCol = null;
                    MeshFilter filt = FTransformMethods.FindComponentInAllChildren<MeshFilter>(objectsToSimulate[i].transform);

                    if (filt) meshForCol = filt.sharedMesh;

                    if (meshForCol)
                    {
                        MeshCollider mCol = objectsToSimulate[i].AddComponent<MeshCollider>();
                        mCol.convex = true;
                        col = mCol;
                        generatedColliders.Add(col);
                    }
                }

                Rigidbody rig = objectsToSimulate[i].GetComponent<Rigidbody>();
                if (!rig)
                {
                    rig = objectsToSimulate[i].AddComponent<Rigidbody>();
                    generatedRigidbodies.Add(rig);
                }

                simulatingRigidbodies.Add(rig);
            }

            MovedSimulationObjects(simScene);
        }

        #endregion



        #region Area Utilities

        List<GameObject> objectsToSimulate = null;
        List<Rigidbody> simulatingRigidbodies = null;
        List<Collider> generatedColliders = null;
        List<MeshCollider> _mMeshNonConvexColliders = null;
        List<Rigidbody> generatedRigidbodies = null;

        /// <summary> List for single object simulation </summary>
        List<GameObject> sList = null;

        public void PrepareSimulationList(List<GameObject> objects = null)
        {
            if (objectsToSimulate == null)
                objectsToSimulate = new List<GameObject>();
            else
                objectsToSimulate.Clear();

            if (objects != null)
            {
                for (int i = 0; i < objects.Count; i++) objectsToSimulate.Add(objects[i]);
            }
        }


        List<Collider> _simCollideWith;
        List<Collider> _simMyColliders;
        List<Renderer> _simMyRenderers;

        Vector3 _searchAreaCenter;
        float _searchAreaRadius;

        List<Collider> GetCollisionAround(List<GameObject> toSimulate)
        {

            if (toSimulate.Count == 0) return null;
            if (toSimulate[0] == null) return null;

            if (_simCollideWith == null) _simCollideWith = new List<Collider>(); else _simCollideWith.Clear();
            if (_simMyColliders == null) _simMyColliders = new List<Collider>(); else _simMyColliders.Clear();
            if (_simMyRenderers == null) _simMyRenderers = new List<Renderer>(); else _simMyRenderers.Clear();

            Bounds averageBounds = new Bounds();


            #region Collect self colliders and disable to exclude from overlap search

            for (int i = 0; i < toSimulate.Count; i++)
            {
                Transform t = toSimulate[i].transform;

                //foreach (var coll in t.GetComponents<Collider>())
                //{
                //    _simMyColliders.Add(coll);

                //    Bounds b = coll.bounds; // Unity says it's world bounds but in real it's local o_O
                //    b.center = coll.transform.TransformPoint(b.center);

                //    if (averageBounds.center == Vector3.zero) averageBounds = b;
                //    else averageBounds.Encapsulate(b.center);
                //}

                foreach (Transform child in t.GetComponentsInChildren<Transform>())
                {
                    foreach (var coll in child.GetComponents<Collider>())
                    {
                        _simMyColliders.Add(coll);

                        //Bounds b = coll.bounds; // Unity says it's world bounds but in real sometimes it's local and sometimes not o_O
                        //b.center = coll.transform.TransformPoint(b.center);

                        if (averageBounds.center == Vector3.zero) averageBounds = new Bounds(coll.transform.position, Vector3.one * 0.5f);
                        else averageBounds.Encapsulate(coll.transform.position);
                    }
                }
            }

            // Disable to ignore from overlap search
            for (int m = 0; m < _simMyColliders.Count; m++) _simMyColliders[m].enabled = false;

            #endregion


            _searchAreaCenter = averageBounds.center;
            _searchAreaRadius = averageBounds.extents.magnitude;

            //UnityEngine.Debug.Log("myColls = " + _simMyColliders.Count + " bounds center = " + averageBounds.center);
            if (_simMyColliders.Count == 0) return null;

            RaycastHit firstGround;
            Physics.Raycast(averageBounds.center + Vector3.up * averageBounds.extents.magnitude, Physics.gravity.normalized, out firstGround);
            //UnityEngine.Debug.DrawRay(averageBounds.center + Vector3.up * averageBounds.size.magnitude, Physics.gravity.normalized * 10f, Color.yellow, 1.01f);
            float finalRadius = averageBounds.extents.magnitude;

            //UnityEngine.Debug.Log("firstGround = " + firstGround.transform);
            if (firstGround.transform == null)
            {
                //UnityEngine.Debug.Log("NO GROUND");
                return null; 
            }

            float toGround = firstGround.distance;
            if (toGround < 5f) toGround = 5f;
            if (toGround > 40f) toGround = 40f;

            if (toGround > finalRadius) finalRadius = toGround;
            else if (finalRadius < toGround) finalRadius = toGround;

            _searchAreaRadius = finalRadius;


            #region Prepare layer mask

            int myLayer = toSimulate[0].gameObject.layer;
            LayerMask layerMask = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(myLayer, i)) layerMask = layerMask | 1 << i;
            }

            #endregion


            var oCols = Physics.OverlapSphere(averageBounds.center, finalRadius, layerMask);
            //UnityEngine.Debug.Log("Radius " + finalRadius);

            for (int c = 0; c < oCols.Length; c++)
            {
                if (oCols[c].isTrigger) continue;

                var col = oCols[c];
                if (_simCollideWith.Contains(col)) continue;
                if (_simMyColliders.Contains(col)) continue;

                bool isChild = false;
                for (int w = 0; w < _simCollideWith.Count; w++)
                {
                    if (FGenerators.IsChildOf(col.transform, _simCollideWith[w].transform))
                    {
                        isChild = true;
                        break;
                    }
                }

                if (isChild) continue;

                _simCollideWith.Add(col);
            }


            //UnityEngine.Debug.Log("_simCollideWith count = " + _simCollideWith.Count);
            return _simCollideWith;
        }


        #endregion



        #region Backup Utilities

        Dictionary<GameObject, TransformBackup> backups;

        struct TransformBackup
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Transform Parent;
            public bool WasStatic;

            public void SetBackup(GameObject o)
            {
                if (o == null) return;
                Parent = o.transform.parent;
                WasStatic = o.isStatic;
                Position = o.transform.position;
                Rotation = o.transform.rotation;
            }

            public void RestoreBackupedTransform(GameObject o, bool setParent = true)
            {
                if (o == null) return;

                if (setParent) o.transform.SetParent(Parent, true);

                o.transform.position = Position;
                o.transform.rotation = Rotation;

                if (setParent) o.isStatic = WasStatic;
            }
        }

        GameObject singleBackupFor;
        TransformBackup singleBackup;

        public void DoBackupFor(GameObject o)
        {
            if (backups != null) backups.Clear();

            singleBackupFor = o;
            singleBackup.Position = o.transform.position;
            singleBackup.Rotation = o.transform.rotation;
        }

        public void DoBackupFor(List<GameObject> objects)
        {
            if (backups == null) backups = new Dictionary<GameObject, TransformBackup>(); else backups.Clear();

            for (int i = 0; i < objects.Count; i++)
            {
                var b = new TransformBackup();
                b.SetBackup(objects[i]);
                backups.Add(objects[i], b);
            }
        }

        public void RestoreBackuped()
        {
            if (backups != null)
            {
                if (backups.Count > 0)
                {
                    foreach (var item in backups)
                    {
                        item.Value.RestoreBackupedTransform(item.Key);
                    }

                    return;
                }
            }

            if (singleBackupFor != null) return;

            singleBackupFor.transform.position = singleBackup.Position;
            singleBackupFor.transform.rotation = singleBackup.Rotation;
        }


        #endregion



        #region Editor Code

#if UNITY_EDITOR

        public void _Editor_DrawSetup(SerializedProperty sp, bool drawEnableToggle)
        {
            if (Enabled == false)
            {
                if (GUILayout.Button("Physical Simulation Placement is disabled", EditorStyles.helpBox))
                {
                    Enabled = true;
                }

                return;
            }

            SerializedProperty spc = sp.Copy();
            spc.Next(true);
            if (drawEnableToggle == false) spc.Next(false);

            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc); spc.Next(false);
            EditorGUILayout.PropertyField(spc);
        }

        const string _tooltip = "Settings for physics collision simulation placement for spawned objects.\nCan be used without raycasting!";
        public void _Editor_DrawSetupToggle(SerializedProperty sp)
        {
            string fold = FGUI_Resources.GetFoldSimbol(_Editor_Foldout, false);

            EditorGUILayout.BeginHorizontal();

            string title = "";
            if (sp.boolValue) // Physical Placement Enabled
            {
                title = "  " + fold + "  Draw Physical Placement Setup";
            }
            else // Physical placement is disabled - different info text
            {
                title = "  Enable Physical Placement Setup";
            }

            if (GUILayout.Button(new GUIContent(title, FGUI_Resources.Tex_Physics, _tooltip), EditorStyles.label, GUILayout.Height(18)))
            {
                if (sp.boolValue)
                {
                    _Editor_Foldout = !_Editor_Foldout;
                }
                else
                {
                    sp.boolValue = true;
                    _Editor_Foldout = true;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.Width(30));

            EditorGUILayout.EndHorizontal();
        }

#endif

        #endregion

    }
}