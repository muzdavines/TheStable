using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Animancer;

public class BlueManaEditor : EditorWindow
{

    [MenuItem("Blue Mana/CreateCharacter")]
    static void Init() {
        BlueManaEditor window = (BlueManaEditor)EditorWindow.GetWindow(typeof(BlueManaEditor));
        window.Show();
    }
    GameObject target;
    Transform rightHandFinger;
    Transform RH, LH, LL, RL;
    LinearMixerTransitionAsset movementMixer;
    private void OnGUI() {
        target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
        rightHandFinger = (Transform)EditorGUILayout.ObjectField(rightHandFinger, typeof(Transform), true);
        movementMixer = (LinearMixerTransitionAsset)EditorGUILayout.ObjectField(movementMixer, typeof(LinearMixerTransitionAsset), true);
    }

    private void Create() {
        var body = target.AddComponent<Rigidbody>();
        body.mass = 60;
        body.isKinematic = true;
        var agent = target.AddComponent<NavMeshAgent>();
        var capsule = target.AddComponent<CapsuleCollider>();
        capsule.radius = .3f;
        capsule.height = 2f;
        capsule.center = new Vector3(0,1f,0);
        target.AddComponent<MovementAnimationController>();
        var thisChar = target.AddComponent<StableCombatChar>();
        thisChar.distToTrackBall = 90;
        thisChar.distToTrackEnemy = 20;
        thisChar.distToTrackBallCarrier = 2;
        var feedbacks = Instantiate<GameObject>(Resources.Load<GameObject>("Feedbacks"));
        feedbacks.transform.parent = target.transform;
        feedbacks.transform.localPosition = Vector3.zero;
        feedbacks.transform.localRotation = Quaternion.Euler(Vector3.zero);
        feedbacks.GetComponent<FeedbacksController>().Init(thisChar);
        thisChar.RH = RH;
        thisChar.LH = LH;
        thisChar.RL = RL;
        thisChar.LL = LL;
        var anim = target.AddComponent<AnimancerController>();
        anim.animSet = Resources.Load<AnimancerAnimSet>("DefaultAnimSet");
        var animc = target.AddComponent<AnimancerComponent>();
        anim.anim = animc;
        animc.Animator = target.GetComponent<Animator>();
    }
}
