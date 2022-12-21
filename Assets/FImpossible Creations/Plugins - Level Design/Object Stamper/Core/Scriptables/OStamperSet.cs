using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [CreateAssetMenu(fileName = "OS_", menuName = "FImpossible Creations/Procedural Generation/Object Stamper Preset", order = 10100)]
    public partial class OStamperSet : ScriptableObject
    {
        [Tooltip("Tag used for ignoring placement on stamp objects")]
        public string StampersetTag = "";
        [FPD_Header("Placement Randomization Settings", 2, 5)]
        [Tooltip("Randomize positioning basing on model's bounds scale for raycasting")]
        [Range(0f, 1f)]
        public float RandomizePosition = 0.2f;

        public Vector3 RandPositionAxis = new Vector3(1f, 0f, 1f);
        //public MinMaxF RandomizePosition = new MinMaxF(0f, 0.1f);

        [Space(8)]
        [Tooltip("Randomize rotation")]
        public Vector2 RotationRanges = new Vector2(-180, 180);

        public Vector3 RandRotationAxis = Vector3.up;

        //public MinMaxF RandomizeRotation = new MinMaxF(-180f, 180f);
        [Tooltip("How dense can be rotation changes during randomization process")]
        public Vector3 AngleStepForAxis = Vector3.one;

        [Space(8)]
        [Tooltip("Randomize positioning basing on model's bounds scale for raycasting")]
        [Range(0f, 1f)]
        public float RandomizeScale = 0f;

        [Tooltip("Axis on which object should scale, leave Y and Z as 1 and change X to negative value for uniform scale!")]
        public Vector3 RandScaleAxis = Vector3.one;
        //public MinMaxF RandomizeScale = new MinMaxF(0f, 0f);

        [FPD_Header("Raycasting Settings", 8, 5)]
        public LayerMask RayCheckLayer = ~(0 << 0);

        [Tooltip("Local space direction to check alignment for prefabs in list (can be overrided)")]
        public Vector3 RayCheckDirection = Vector3.down;

        [Range(0f, 1f)]
        public float RaycastAlignment = 1f;
        [Range(-0.5f, 1f)] [Tooltip("Offsetting placing object on floor/wall to avoid Z-fight on flat models")]
        public float AlignOffset = 0f;

        //[Range(1, 5)]
        public EOSPlacement PlacementMode = EOSPlacement.LayAlign;

        //public int RaycastDensity = 1;
        public bool RaycastWorldSpace = true;
        //[Tooltip("When obstacle hit occured then algorithm will check ground below (world Vector down) to place object on floor in front of obstacle (for example wall)\nIt will work nicely for furniture which needs to stand under wall")]
        //public bool DropDown = false;
        [Range(0f, 1.15f)] [Tooltip("When spawning checking if object is not overlapping through OverlapCheckMask collision objects")]
        public float OverlapCheckScale = 0f;
        public LayerMask OverlapCheckMask = 0;

        //public float RaycastUpOffset = 0.1f;

        [Tooltip("Raycast length distance is multiplier of stamper set reference bounds size")]
        public float RayDistanceMul = 1.5f;

        [Space(8)]
        [Tooltip("Reference bounding box to compute emitting coordinates correctly (local - based on shared meshes)")]
        public Bounds ReferenceBounds;

        [HideInInspector] public List<OSPrefabReference> Prefabs;

        [HideInInspector] public EOSRaystriction StampRestriction = EOSRaystriction.None;
        [Tooltip("Give spawn info in the generated objects (adding StampStigma component) in order for other stamps to detect details of this generated object")]
        [HideInInspector] public bool IncludeSpawnDetails = true;

        public List<OStamperSet> RestrictionSets;
        [HideInInspector] public int PlacementLimitCount = 0;

        [FPD_Header("Physical Restrictions", 8, 5)]
        [HideInInspector] [Range(0, 90)] public int MaxSlopeAngle = 60;

        [HideInInspector] [Range(0f, 1f)] public float MinimumStandSpace = 0.2f;

        [FPD_Header("GameObject Tag Based", 8, 5)]
        public List<string> AllowJustOnTags = new List<string>();

        public List<string> DisallowOnTags = new List<string>();

        [Space(5)]
        public List<string> IgnoreCheckOnTags = new List<string>();

        // Rest of the code inside partial classes -------------------
    }
}