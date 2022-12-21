using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class OStamperSet
    {
        /// <summary>
        /// Choosing random prefab basing on their propability settings and generating spawn info
        /// </summary>
        public ObjectStamperEmittedInfo Emit(bool noRepetition = false, Transform parentSpace = null)
        {
            if (Prefabs == null) return new ObjectStamperEmittedInfo();
            int available = Prefabs.Count;
            if (available == 0) return new ObjectStamperEmittedInfo();
            if (available == 1) return GenerateInfoForPrefab(Prefabs[0], parentSpace);

            float propSum = 0f;
            for (int i = 0; i < available; i++) propSum += Prefabs[i].Propability;

            float selection = FGenerators.GetRandom(0f, propSum);
            //float selection = UnityEngine.Random.Range(0f, propSum);
            float progress = 0f;

            for (int i = 0; i < available; i++)
            {
                progress += Prefabs[i].Propability;
                if (selection < progress) return GenerateInfoForPrefab(Prefabs[i], parentSpace);
            }

            return GenerateInfoForPrefab(Prefabs[Prefabs.Count - 1], parentSpace);
        }


        public ObjectStamperEmittedInfo GenerateInfoForPrefab(OSPrefabReference reference, Transform parentSpace)
        {
            if (reference == null) return new ObjectStamperEmittedInfo();
            if (reference.GameObject == null) return new ObjectStamperEmittedInfo();

            ObjectStamperEmittedInfo info = new ObjectStamperEmittedInfo();
            info.SetReference = this;
            info.PrefabReference = reference;
            info.ChoosedPrefab = reference.GameObject;
            info = RefreshEmitInfo(info, parentSpace);

            return info;
        }


        public ObjectStamperEmittedInfo RefreshEmitInfo(ObjectStamperEmittedInfo info, Transform parentSpace)
        {
            // Position and scale randomization
            info.OffsetMul = R(RandomizePosition * RandPositionAxis);
            info.ScaleOffsetMul = Vector3.one;

            if (RandScaleAxis.x < 0 && RandScaleAxis.y == 1f && RandScaleAxis.z == 1f) // Uniform
            {
                info.ScaleOffsetMul += Vector3.one * R(-RandScaleAxis.x * RandomizeScale);
            }
            else // Non uniform
            {
                info.ScaleOffsetMul += R(RandomizeScale * RandScaleAxis);
            }

            info.RotOffset =
                Quaternion.Euler
                (
                    GetAngleFor(AngleStepForAxis.x, RandRotationAxis.x, GetRandomRotation(AngleStepForAxis.x)),
                    GetAngleFor(AngleStepForAxis.y, RandRotationAxis.y, GetRandomRotation(AngleStepForAxis.y)),
                    GetAngleFor(AngleStepForAxis.z, RandRotationAxis.z, GetRandomRotation(AngleStepForAxis.z))
                );

            if (parentSpace) info.RotOffset = parentSpace.rotation * info.RotOffset;

            return info;
        }

    }



    [System.Serializable]
    public struct ObjectStamperEmittedInfo
    {
        /// <summary> Reference to parent set with all settings and other prefabs </summary>
        public OStamperSet SetReference;
        /// <summary> Prefab helper data reference with local bounds etc. </summary>
        public OSPrefabReference PrefabReference;
        /// <summary> Randomly choosed prefab GameObject </summary>
        public GameObject ChoosedPrefab;

        /// <summary> Random position offset multiplier based on stamper set settings, should be applied on prefab local bounds </summary>
        public Vector3 OffsetMul;
        /// <summary> Random rotation offset based on stamper set settings </summary>
        public Quaternion RotOffset;
        /// <summary> Random scale multiplier based on stamper set settings </summary>
        public Vector3 ScaleOffsetMul;


        public Matrix4x4 GetMatrixFor(Transform transform, Vector3? customOrigin = null, Quaternion? customRotation = null)
        {
            Vector3 origin;

            if (SetReference.RaycastWorldSpace)
                origin = customOrigin == null ? transform.position /*+ GetPositionOffsetLocal()*/ : (Vector3)customOrigin;
            else
                origin = customOrigin == null ? transform.position /*+ transform.TransformVector(GetPositionOffsetLocal()) */: (Vector3)customOrigin;

            Quaternion rotOrigin = customRotation == null ? transform.rotation * RotOffset : (Quaternion)customRotation;
            
            Vector3 scaler = Vector3.Scale(transform.lossyScale, ScaleOffsetMul);

            return Matrix4x4.TRS(origin, rotOrigin, scaler);
        }


        /// <summary> OffsetMul must be computed first </summary>
        public Vector3 GetPositionOffsetLocal()
        {
            return Vector3.Scale(PrefabReference.ReferenceBounds.extents, OffsetMul);
        }


        public Vector3 GetWorldOffsetOnHit(Transform transform, RaycastHit hit, Vector3 originPoint)
        {
            Quaternion rotationOn = MapOffsetRotation(RotOffset, transform); // Mapped offset rotation
            Matrix4x4 offsetMatrix = Matrix4x4.TRS(originPoint, rotationOn, transform.lossyScale);

            Bounds currentWorldBounds = GeometryUtility.CalculateBounds(OSPrefabReference.GetBoundsCorners(PrefabReference.ReferenceBounds), offsetMatrix);

            rotationOn = GetRotationOn(transform, hit.normal); // Mapped offset rotation + on hit normal rotation
            Vector3 dirToMove = SetReference.RaycastWorldSpace ? SetReference.RayCheckDirection : rotationOn * SetReference.RayCheckDirection;

            Vector3 worldOffset = -Vector3.Scale(currentWorldBounds.extents, dirToMove);
            worldOffset -= rotationOn * Vector3.Scale(PrefabReference.ReferenceBounds.center, transform.lossyScale);

            return worldOffset;
        }


        public Quaternion MapOffsetRotation(Quaternion rotOffset, Transform transform)
        {
            Quaternion r = transform == null ? Quaternion.identity : transform.rotation;
            r *= rotOffset;
            Vector3 mapping = r.eulerAngles;

            mapping.x = OStamperSet.GetAngleFor(SetReference.AngleStepForAxis.x, 1f, mapping.x / SetReference.AngleStepForAxis.x);
            mapping.y = OStamperSet.GetAngleFor(SetReference.AngleStepForAxis.y, 1f, mapping.y / SetReference.AngleStepForAxis.y);
            mapping.z = OStamperSet.GetAngleFor(SetReference.AngleStepForAxis.z, 1f, mapping.z / SetReference.AngleStepForAxis.z);

            return Quaternion.Euler(mapping);
        }


        public Quaternion GetRotationOn(Transform transform, Vector3? normal = null)
        {
            Quaternion mappedRotation = MapOffsetRotation(RotOffset, transform);
            Quaternion rot = mappedRotation;

            if (SetReference.RaycastAlignment <= 0f || normal == null) return rot;

            Vector3 nm = (Vector3)normal;
            Quaternion onNormal = Quaternion.FromToRotation(-SetReference.RayCheckDirection.normalized, nm) * MapOffsetRotation(RotOffset, null);
            //Quaternion onNormal = Quaternion.FromToRotation(-SetReference.RayCheckDirection.normalized, nm) * mappedRotation;

            if (SetReference.RaycastAlignment >= 1f)
                return onNormal;
            else
                return Quaternion.Slerp(rot, onNormal, SetReference.RaycastAlignment);
        }


        public GameObject Spawn(Transform spawnerTransform, Transform parentForSpawned, RaycastHit? hit = null, Vector3? customPosition = null, bool setParent = true)
        {
            #region Null checks

            if (SetReference == null)
            {
                Debug.Log("[Object Stamper] No Set Reference! (" + spawnerTransform + ")");
                return null;
            }

            if (spawnerTransform == null && customPosition == null)
            {
                Debug.Log("[Object Stamper] No Position to place object anywhere! (" + spawnerTransform + ")");
                return null;
            }

            #endregion


            GameObject obj = FGenerators.InstantiateObject(ChoosedPrefab);
            obj.transform.localScale = Vector3.Scale(Vector3.Scale(obj.transform.localScale, ScaleOffsetMul), spawnerTransform.localScale);

            if (hit == null) // Position it on parent or custom position
            {
                if (customPosition == null)
                    obj.transform.position = spawnerTransform.position;
                else
                    obj.transform.position = (Vector3)customPosition;

                obj.transform.rotation = GetRotationOn(spawnerTransform);
            }
            else // Place object on raycasting
            {
                obj.transform.position = GetSpawnPosition(spawnerTransform, hit.Value, customPosition);
                obj.transform.rotation = GetRotationOn(spawnerTransform, hit.Value.normal);
            }

            if (setParent)
                obj.transform.SetParent(parentForSpawned, true);

            if (SetReference.IncludeSpawnDetails)
            {
                OStampStigma stigma = obj.AddComponent<OStampStigma>();
                stigma.ReferenceSet = SetReference;
                stigma.EmitInfo = this;
            }

            return obj;
        }


        /// <summary>
        /// Returning spawn position for hit with lay alignment or plant aligment
        /// </summary>
        public Vector3 GetSpawnPosition(Transform parent, RaycastHit hit, Vector3? customPoint = null)
        {
            Vector3 offset = Vector3.zero;

            if (hit.transform)
                if (SetReference.AlignOffset != 0f)
                    offset = hit.normal * SetReference.AlignOffset * SetReference.ReferenceBounds.extents.magnitude; // Additional offset to prevent z-fighting

            if (SetReference.PlacementMode == OStamperSet.EOSPlacement.PlantAlign) return hit.point + offset;

            Vector3 hitPos = customPoint == null ? hit.point : customPoint.Value;
            hitPos += offset;
            return hitPos;
        }


    }
}