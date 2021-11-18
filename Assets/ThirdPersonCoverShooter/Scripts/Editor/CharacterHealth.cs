using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(CharacterHealth))]
    [CanEditMultipleObjects]
    public class CharacterHealthEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var areThereHitColliders = false;

            foreach (var object_ in targets)
                if (hasHitColliders(((CharacterHealth)object_).transform))
                {
                    areThereHitColliders = true;
                    break;
                }

            if (areThereHitColliders)
                if (GUILayout.Button("Clear hit colliders"))
                {
                    Undo.RecordObjects(targets, "Clear hit colliders");

                    foreach (var object_ in targets)
                    {
                        var health = (CharacterHealth)object_;
                        removeHitColliders(health.transform);
                        health.IsRegisteringHits = true;
                    }
                }

            if (GUILayout.Button("Setup hit colliders"))
            {
                Undo.RecordObjects(targets, "Setup hit colliders");

                foreach (var object_ in targets)
                {
                    var health = (CharacterHealth)object_;
                    removeHitColliders(health.transform);
                    health.IsRegisteringHits = false;

                    var animator = health.GetComponent<Animator>();
                    if (animator == null) continue;

                    addColliderBetween(1, animator.GetBoneTransform(HumanBodyBones.Hips), animator.GetBoneTransform(HumanBodyBones.Chest), 0.2f, 1);
                    addColliderBetween(1, animator.GetBoneTransform(HumanBodyBones.Chest), animator.GetBoneTransform(HumanBodyBones.Neck), 0.2f, 1);
                    addColliderBetween(5, animator.GetBoneTransform(HumanBodyBones.Neck), animator.GetBoneTransform(HumanBodyBones.Head), 0.15f, 2, 1);

                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), 0.1f);
                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal), 0.1f, 1);

                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.RightUpperArm), animator.GetBoneTransform(HumanBodyBones.RightLowerArm), 0.1f);
                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.RightLowerArm), animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal), 0.1f, 1);

                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), 0.1f);
                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), animator.GetBoneTransform(HumanBodyBones.LeftFoot), 0.1f);
                    addColliderBetween(0.5f, animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.LeftToes), 0.1f);

                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), 0.1f);
                    addColliderBetween(0.8f, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), animator.GetBoneTransform(HumanBodyBones.RightFoot), 0.1f);
                    addColliderBetween(0.5f, animator.GetBoneTransform(HumanBodyBones.RightFoot), animator.GetBoneTransform(HumanBodyBones.RightToes), 0.1f);
                }
            }
        }

        private void addColliderBetween(float damage, Transform a, Transform b, float radius, float extension = 2, float offset = 0)
        {
            if (a == null || b == null)
                return;

            var obj = new GameObject("HitBox");
            obj.transform.parent = a;
            obj.transform.localPosition = Vector3.zero;

            var health = obj.AddComponent<BodyPartHealth>();
            health.DamageScale = damage;

            var capsule = obj.AddComponent<CapsuleCollider>();
            capsule.radius = radius;
            capsule.isTrigger = true;

            var diff = b.position - a.position;
            var height = diff.magnitude;
            capsule.height = height + radius * extension;

            var local = findLocal(a.transform, diff);

            capsule.center = local * height * 0.5f + local * radius * offset;

            var x = Mathf.Abs(local.x);
            var y = Mathf.Abs(local.y);
            var z = Mathf.Abs(local.z);

            if (x > y && x > z)
                capsule.direction = 0;
            else if (y > z)
                capsule.direction = 1;
            else
                capsule.direction = 2;
        }

        private Vector3 findLocal(Transform transform, Vector3 diff)
        {
            if (diff.sqrMagnitude > float.Epsilon)
                diff.Normalize();

            var current = transform.forward;
            compare(ref current, -transform.forward, diff);
            compare(ref current, transform.up, diff);
            compare(ref current, -transform.up, diff);
            compare(ref current, transform.right, diff);
            compare(ref current, -transform.right, diff);

            return current;
        }

        private void compare(ref Vector3 current, Vector3 value, Vector3 diff)
        {
            var dot0 = Vector3.Dot(current, diff);
            var dot1 = Vector3.Dot(value, diff);

            if (dot1 > dot0)
                current = value;
        }

        private void removeHitColliders(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                var health = child.GetComponent<BodyPartHealth>();

                if (health == null)
                    removeHitColliders(child);
                else
                    GameObject.DestroyImmediate(child.gameObject);
            }
        }

        private bool hasHitColliders(Transform transform)
        {
            if (transform.GetComponent<BodyPartHealth>())
                return true;

            for (int i = 0; i < transform.childCount; i++)
                if (hasHitColliders(transform.GetChild(i)))
                    return true;

            return false;
        }
    }
}
