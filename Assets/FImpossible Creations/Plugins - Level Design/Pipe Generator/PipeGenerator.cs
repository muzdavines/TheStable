using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/Level Design/Pipe Generator", 3)]
    public partial class PipeGenerator : MonoBehaviour, IGenerating
    {
        [SerializeField] [HideInInspector] private PipePreset projectPreset;
        [SerializeField] private PipePreset.PipePresetData componentPreset;
        public void SetPreset(PipePreset p) { projectPreset = p; }

        public PipePreset.PipePresetData PresetData { get { return projectPreset ? projectPreset.Data : componentPreset; } }

        [Tooltip("If pipe should be generated when game starts (if no pipe is generated in Editor mode)")]
        public bool GenerateOnStart = true;

        [FPD_Header("Segments path generate conditions")]
        public Transform DesiredEnding;
        public Vector3? CustomEndingPosition = null;
        //public Vector3 EndPosition { get { return CustomEndingPosition == null ? DesiredEnding.position : CustomEndingPosition.Value; } }
        public Vector3 EndPosition { get { if (DesiredEnding == null) { return CustomEndingPosition == null ? transform.position : CustomEndingPosition.Value; } else return CustomEndingPosition == null ? DesiredEnding.position : CustomEndingPosition.Value; } }
        public Vector3? CustomEndingDirection = null;
        //public Vector3 EndDirection { get { return CustomEndingDirection == null ? DesiredEnding.forward : CustomEndingDirection.Value; } }
        public Vector3 EndDirection { get { if (DesiredEnding == null) { return CustomEndingDirection == null ? transform.forward : CustomEndingDirection.Value; } else return CustomEndingDirection == null ? DesiredEnding.forward : CustomEndingDirection.Value; } }

        [Tooltip("Pipe can't finish in exact position of 'Desired Ending' point, but you can determinate how far from this point pipe ending process can start")]
        public float MaxDistanceToEnding = 4f;
        [Tooltip("How many times algorithm can iterate to find right path towards desired position")]
        public int MaxTries = 128;

        [Space(2)]
        [Tooltip("If you want to use raycasting and run finish-align process with additional segments")]
        public bool AlignFinish = true;
        [Tooltip("Collision mask for finding end align point")]
        public LayerMask AlignFinishOptionalsOn;

        [Space(4)]
        [Tooltip("Not allowing to generate pipe segments when couldn't find path towards target position, if untoggled pipe will be generate lastest unfinished segments-path")]
        public bool DontGenerateIfNotEnded = true;
        [Tooltip("If first iteration segments should have disabled collision checking, useful when you start pipe from inside of some collider")]
        public int FirstSegmentsWithoutCollision = 1;
        [Tooltip("Squasing / scaling down last segments to not go through walls when segment model is too long in it's size")]
        public bool AlignScaleForFinishingSegments = true;


        [FPD_Header("Align Start Properties")]
        [Tooltip("Raycast collision mask for start alignment search")]
        public LayerMask AlignStartOn;
        [Tooltip("How far finish collision point can be away from game object's position")]
        public float AlignStartMaxDistance = 2.5f;
        [Tooltip("You can choose in which directions start align point should be search")]
        public Vector3[] AlignStartDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, Vector3.back };


        [FPD_Header("Raycasting Collision")]
        [Tooltip("Collision mask for detecting obstacles in segments-path way")]
        public LayerMask ObstaclesMask;
        [Tooltip("Box cast scale for detecting obstacles, you can preview it's size with scene gizmos")]
        public float BoxcastScale = 0f;
        [Tooltip("Using bounding box object's mesh collision to avoid self collision when generating segments'path")]
        [Range(0.0f, 1f)] public float SelfCollisionScale = 0.75f;
        // [Tooltip("Checking if pipe segments is inside other collider")]
        //[Range(0.0f, 1f)] public float OverlapCheckScale = 0.0f;

        [FPD_Header("Extra Conditions")]
        [Tooltip("Using this collision mask you can define if pipe segments-path should be near for example to walls, then it will try finding path which is near to wall instead of pipe haning in air")]
        public LayerMask HoldMask;
        [Tooltip("You can choose directions on which pipe segments should hold on")]
        public Vector3[] HoldDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, -Vector3.up, Vector3.forward, -Vector3.forward };
        [Tooltip("How far segments can be to hold mask collision check, beware to not set it too low")]
        public float MinimalDistanceToHoldMask = 1.5f;

        [FPD_Header("Random Find Desired Position ON START")]
        public int RFindSeed = 0;
        public LayerMask RFindMask = ~(0 << 0);
        public Vector3[] RFindDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, -Vector3.up, Vector3.forward, -Vector3.forward };
        public bool WorldSpaceRFindDirs = true;
        public float RFindMinimumDistance = 5f;
        public float RFindMaxDistance = 25f;
        public bool FlattendRFindNormal = true;
        [Range(1, 32)] public int RFindTries = 16;
        [Range(1, 24)] public int RFindSteps = 14;

        private void Start()
        {
            if (RFindSeed == 0) RFindSeed = Random.Range(-99999, 99999);

            if (GenerateOnStart)
                if (AreGeneratedObjects == false)
                {
                    if (DesiredEnding == null ) 
                        FindRandomDesiredPoint(RFindSeed);

                    PipeGenerate();
                    //StartCoroutine(IEPreviewGenerationDebug());
                }
        }

        public void PipeGenerate()
        {
            if (AreGeneratedObjects == false)
                if (ValidateGenerationCorrectness())
                    GenerateObjects();
        }

        public void Generate()
        {
            if (RFindSeed == 0) RFindSeed = FGenerators.GetRandom(-99999, 999999);
           
            if (DesiredEnding == null)
            {
                Physics.SyncTransforms();

                CustomEndingPosition = null;
                FindRandomDesiredPoint(RFindSeed);

                if (!ValidateGenerationCorrectness())
                {
#if UNITY_EDITOR
                    if ( UnityEditor.Selection.Contains(gameObject.GetInstanceID()))
                        UnityEngine.Debug.Log("[Pipe Generator] Can't validate start/end position (can't find attachement points on scene)");
#endif
                    return;
                }
            }
            else
            {
                CustomEndingDirection = null;
                CustomEndingPosition = null;
            }

            PipeGenerate();
        }

        public void IG_CallAfterGenerated() { }


        public void PreviewGenerate()
        {
            if (DesiredEnding == null)
            {
                int seed = RFindSeed;
                if (seed == 0) seed = Random.Range(-99999, 99999);
                FindRandomDesiredPoint(seed);
            }
            else
            {
                CustomEndingDirection = null;
                CustomEndingPosition = null;
            }

            if (!ValidateGenerationCorrectness()) return;

            PipePreviewGeneration();
        }

        bool ValidateGenerationCorrectness()
        {
            if (DesiredEnding == null)
            {
                if (CustomEndingPosition == null) return false;
            }

            return true;
        }
    }

}