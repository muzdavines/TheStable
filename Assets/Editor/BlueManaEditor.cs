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
        rightHandFinger = (Transform)EditorGUILayout.ObjectField("RHandFinger",rightHandFinger, typeof(Transform), true);
        RH = (Transform)EditorGUILayout.ObjectField("RH",RH, typeof(Transform), true);
        LH = (Transform)EditorGUILayout.ObjectField("LH", LH, typeof(Transform), true);
        RL = (Transform)EditorGUILayout.ObjectField("RL", RL, typeof(Transform), true);
        LL = (Transform)EditorGUILayout.ObjectField("LL", LL, typeof(Transform), true);
        movementMixer = (LinearMixerTransitionAsset)EditorGUILayout.ObjectField(movementMixer, typeof(LinearMixerTransitionAsset), true);
        if (GUILayout.Button("Create")) {
            Create();
        }
    }

    private void Create() {
        Rigidbody body = target.GetComponent<Rigidbody>();
        body ??= target.AddComponent<Rigidbody>();
        body.mass = 60;
        body.isKinematic = true;
        var agent = target.GetComponent<NavMeshAgent>();
        agent ??= target.AddComponent<NavMeshAgent>();
        agent.radius = .3f;
        var capsule = target.GetComponent<CapsuleCollider>();
        capsule ??= target.AddComponent<CapsuleCollider>();
        capsule.radius = .3f;
        capsule.height = 2f;
        capsule.center = new Vector3(0,1f,0);
        MovementAnimationController animControl = target.GetComponent<MovementAnimationController>();
        animControl ??= target.AddComponent<MovementAnimationController>();
        var thisChar = target.GetComponent<StableCombatChar>();
        thisChar ??= target.AddComponent<StableCombatChar>();
        thisChar.distToTrackBall = 90;
        thisChar.distToTrackEnemy = 20;
        thisChar.distToTrackBallCarrier = 2;
        var feedbackControl = target.GetComponentInChildren<FeedbacksController>();
        if (feedbackControl == null) {
            var feedbacks = Instantiate<GameObject>(Resources.Load<GameObject>("Feedbacks"));
            feedbacks.transform.parent = target.transform;
            feedbacks.transform.localPosition = Vector3.zero;
            feedbacks.transform.localRotation = Quaternion.Euler(Vector3.zero);
            feedbacks.GetComponent<FeedbacksController>().Init(thisChar);
        }
        thisChar._rightHand = rightHandFinger;
        thisChar.RH = RH;
        thisChar.LH = LH;
        thisChar.RL = RL;
        thisChar.LL = LL;
        var anim = target.GetComponent<AnimancerController>();
        anim ??= target.AddComponent<AnimancerController>();
        anim.animSet = Resources.Load<AnimancerAnimSet>("DefaultAnimSet");
        anim.dodgeTackle = new ClipTransition[5];
        anim.skills = new ClipTransition[1];
        var animc = target.GetComponent<AnimancerComponent>();
        animc ??= target.AddComponent<AnimancerComponent>();
        anim.anim = animc;
        animc.Animator = target.GetComponent<Animator>();
        target.tag = "Character";
        target.layer = 10;
    }
}
