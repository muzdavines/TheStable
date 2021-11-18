using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Settings of a bone to be manipulated by IK.
    /// </summary>
    public struct IKBone
    {
        /// <summary>
        /// Defines bone's influence in a bone chain.
        /// </summary>
        public float Weight;

        /// <summary>
        /// Non-Unity representation of the transform to be modified.
        /// </summary>
        public IKTransform Link;

        /// <summary>
        /// Bone in the skeleton to transform.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Sibling bone in the skeleton to transform.
        /// </summary>
        public Transform Sibling;

        public IKBone(Transform transform, float weight)
        {
            Weight = weight;
            Link = null;
            Transform = transform;
            Sibling = null;
        }

        public IKBone(Transform transform, Transform sibling, float weight)
        {
            Weight = weight;
            Link = null;
            Transform = transform;
            Sibling = sibling;
        }
    }

    /// <summary>
    /// Representation of a transform manipulated by IK.
    /// </summary>
    public class IKTransform
    {
        /// <summary>
        /// Local accumulated change in rotation.
        /// </summary>
        public Quaternion Change = Quaternion.identity;

        /// <summary>
        /// Link to the Transform that IKTransform represents.
        /// </summary>
        public Transform Link;

        /// <summary>
        /// Link to the parent transform representation.
        /// </summary>
        public IKTransform Parent;

        /// <summary>
        /// Link to the child transform representation.
        /// </summary>
        public IKTransform Child;

        /// <summary>
        /// Current calculated position in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current calculated rotation in world space.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Saved position in world space of the linked bone.
        /// </summary>
        public Vector3 SavedPosition;

        /// <summary>
        /// Saved rotation in world space of the linked bone.
        /// </summary>
        public Quaternion SavedRotation;

        /// <summary>
        /// Saved forward vector in world space of the linked bone.
        /// </summary>
        public Vector3 SavedOffsetedForward;

        /// <summary>
        /// Saved position in local space of the linked bone.
        /// </summary>
        public Vector3 SavedLocalPosition;

        /// <summary>
        /// Does the bone have an offset;
        /// </summary>
        public bool HasOffset;

        /// <summary>
        /// Offset position.
        /// </summary>
        public Vector3 OffsetPosition;

        /// <summary>
        /// Offset orientation.
        /// </summary>
        public Quaternion OffsetOrientation;

        private Quaternion _storedTotalChange;

        /// <summary>
        /// Prepare the transform for manipulation.
        /// </summary>
        public void Reset(Transform link, Transform parent)
        {
            _storedTotalChange = Quaternion.identity;

            Change = Quaternion.identity;
            Link = link;
            SavedPosition = Link.position;
            SavedRotation = Link.rotation;
            SavedOffsetedForward = Link.forward;
            HasOffset = false;

            if (parent != null)
                SavedLocalPosition = Quaternion.Inverse(parent.rotation) * (parent.TransformPoint(link.localPosition) - parent.position);

            Parent = null;
            Child = null;
        }

        /// <summary>
        /// Prepare the transform for manipulation.
        /// </summary>
        public void Reset(Transform link, Transform parent, Vector3 offsetPosition, Quaternion offsetOrientation)
        {
            _storedTotalChange = Quaternion.identity;

            Change = Quaternion.identity;
            Link = link;
            SavedPosition = Link.position;
            SavedRotation = Link.rotation;
            
            HasOffset = true;
            OffsetPosition = offsetPosition;
            OffsetOrientation = offsetOrientation;

            SavedOffsetedForward = OffsetOrientation * Link.forward;

            if (parent != null)
                SavedLocalPosition = Quaternion.Inverse(parent.rotation) * (parent.TransformPoint(link.localPosition) - parent.position);

            Parent = null;
            Child = null;
        }

        /// <summary>
        /// Calculate position and rotation for the transform and recursively call child transforms.
        /// </summary>
        public void Calc()
        {
            if (Parent == null)
                _storedTotalChange = Change;
            else
                _storedTotalChange = Parent._storedTotalChange * Change;

            if (HasOffset)
            {
                var offsetOrientation = _storedTotalChange * OffsetOrientation;
                Rotation = offsetOrientation * SavedRotation;

                var offset = offsetOrientation * OffsetPosition;

                if (Parent == null)
                    Position = SavedPosition + offset;
                else
                    Position = Parent.Position + Parent.Rotation * SavedLocalPosition + offset;
            }
            else
            {
                Rotation = _storedTotalChange * SavedRotation;

                if (Parent == null)
                    Position = SavedPosition;
                else
                    Position = Parent.Position + Parent.Rotation * SavedLocalPosition;
            }

            if (Child != null)
                Child.Calc();
        }

        /// <summary>
        /// Forward vector of the transform.
        /// </summary>
        public Vector3 Forward
        {
            get { return _storedTotalChange * SavedOffsetedForward; }
        }
    }

    /// <summary>
    /// Object that calculates IK transformations.
    /// </summary>
    public class IK
    {
        /// <summary>
        /// Current object to base IK transformations on.
        /// </summary>
        public Transform TargetParentBone;

        /// <summary>
        /// Relative position of the target object relative to the parent bone in the skeleton hierarchy.
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// Orientation of the relative position.
        /// </summary>
        public Quaternion OffsetOrientation = Quaternion.identity;

        /// <summary>
        /// Chain of bones that are manipulated.
        /// </summary>
        public IKBone[] Bones;

        private IKTransform _target;
        private IKTransform[] _transforms = new IKTransform[16];
        private float _updateTime;

        /// <summary>
        /// IK constructor.
        /// </summary>
        public IK()
        {
            for (int i = 0; i < _transforms.Length; i++)
                _transforms[i] = new IKTransform();
        }

        public Vector3 GetTargetPosition()
        {
            var offset = OffsetOrientation * Offset;
            return TargetParentBone.transform.position + TargetParentBone.rotation * offset;
        }

        /// <summary>
        /// Manipulates bones till the Target is looking towards the given target position.
        /// Store results and update again only after a certain amount of time has passed.
        /// </summary>
        public void UpdateAim(Vector3 targetPosition, float delay, float weight, int minIterations, int maxIterations)
        {
            if (Time.realtimeSinceStartup - _updateTime >= delay)
            {
                CalcAim(targetPosition, minIterations, maxIterations);
                _updateTime = Time.realtimeSinceStartup;
            }

            AssignTransforms(weight);
        }

        /// <summary>
        /// Manipulates bones till the Target is at the given target position.
        /// Store results and update again only after a certain amount of time has passed.
        /// </summary>
        public void UpdateMove(Vector3 targetPosition, float delay, float weight, int minIterations, int maxIterations)
        {
            if (Time.realtimeSinceStartup - _updateTime >= delay)
            {
                CalcMove(targetPosition, minIterations, maxIterations);
                _updateTime = Time.realtimeSinceStartup;
            }

            AssignTransforms(weight);
        }

        /// <summary>
        /// Manipulates bones till the Target is looking towards the given target position.
        /// </summary>
        /// <param name="targetPosition">Position to aim at.</param>
        /// <param name="iterations">Number of iterations.</param>
        public void CalcAim(Vector3 targetPosition, int minIterations, int maxIterations)
        {
            if (!prepareTransforms())
                return;

            int i = 0;

            while (i < minIterations || (i < maxIterations && Vector3.Dot((targetPosition - _target.Position).normalized, _target.Forward) < 0.9f))
            {
                for (int b = 0; b < Bones.Length - 1; b++)
                    solveAimBone(targetPosition, Bones[b], (i + 1) / (float)Bones.Length);

                solveAimBone(targetPosition, Bones[Bones.Length - 1], 1.0f);                

                i++;
            }
        }

        /// <summary>
        /// Manipulates bones till the Target is at the given target position.
        /// </summary>
        /// <param name="targetPosition">Position to move Targe to.</param>
        /// <param name="iterations">Number of iterations.</param>
        public void CalcMove(Vector3 targetPosition, int minIterations, int maxIterations)
        {
            if (!prepareTransforms())
                return;

            int i = 0;

            while (i < minIterations || (i < maxIterations && Vector3.Distance(targetPosition, _target.Position) > 0.01f))
            {
                for (int b = 0; b < Bones.Length - 1; b++)
                    solveMoveBone(targetPosition, Bones[b], (i + 1) / (float)Bones.Length);

                solveMoveBone(targetPosition, Bones[Bones.Length - 1], 1.0f);

                i++;
            }
        }

        /// <summary>
        /// Calculates transformation of a single bone, used to aim Target to a target position.
        /// </summary>
        private void solveAimBone(Vector3 targetPosition, IKBone bone, float weightMultiplier = 1.0f)
        {
            if (bone.Link == null)
                return;

            var weight = bone.Weight * weightMultiplier;
            var desired = (targetPosition - _target.Position).normalized;

            var offset = Quaternion.FromToRotation(_target.Forward, desired);

            bone.Link.Change = Quaternion.Lerp(bone.Link.Change, offset * bone.Link.Change, weight);
            bone.Link.Calc();
        }

        /// <summary>
        /// Calculates transformation of a single bone, used to move Target to a target position.
        /// </summary>
        private void solveMoveBone(Vector3 targetPosition, IKBone bone, float weightMultiplier = 1.0f)
        {
            if (bone.Link == null)
                return;

            var weight = bone.Weight * weightMultiplier;
            var current = bone.Link.Position;
            var offset = Quaternion.FromToRotation((_target.Position - current).normalized, (targetPosition - current).normalized);

            bone.Link.Change = Quaternion.Lerp(bone.Link.Change, offset * bone.Link.Change, weight);
            bone.Link.Calc();
        }

        /// <summary>
        /// Assign calculated transformations to bones. 
        /// </summary>
        private void AssignTransforms(float weight)
        {
            for (int i = Bones.Length - 1; i >= 0; i--)
            {
                var bone = Bones[i];

                if (bone.Transform != null && bone.Link != null)
                    bone.Transform.rotation = Quaternion.Lerp(bone.Transform.rotation, bone.Link.Change * bone.Transform.rotation, weight);

                if (bone.Sibling != null && bone.Link != null)
                    bone.Sibling.rotation = Quaternion.Lerp(bone.Sibling.rotation, bone.Link.Change * bone.Sibling.rotation, weight);
            }
        }

        /// <summary>
        /// Remember original transformations of all bones.
        /// </summary>
        private bool prepareTransforms()
        {
            if (Bones.Length == 0 || TargetParentBone == null)
                return false;

            for (int i = 0; i < Bones.Length; i++)
                Bones[i].Link = null;

            var transformIndex = 0;

            _target = _transforms[transformIndex++];
            _target.Reset(TargetParentBone, TargetParentBone.parent, Offset, OffsetOrientation);

            int lastBone = Bones.Length;
            findBone(_target, ref lastBone);

            var transform = TargetParentBone.parent;
            var current = _target;

            while (transform != null && lastBone > 0)
            {
                var parentNode = _transforms[transformIndex++];
                var parent = transform.parent;

                parentNode.Reset(transform, parent);

                findBone(parentNode, ref lastBone);

                current.Parent = parentNode;
                parentNode.Child = current;

                transform = parent;
                current = parentNode;
            }

            current.Calc();

            return true;
        }

        private void findBone(IKTransform transform, ref int last)
        {
            for (int i = last - 1; i >= 0; i--)
                if (transform.Link == Bones[i].Transform)
                {
                    Bones[i].Link = transform;
                    last = i;
                    break;
                }
        }
    }
}