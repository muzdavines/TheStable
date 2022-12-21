using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    /// <summary>
    /// Checker Field is collection of multiple Int2D points represented by squares
    /// All positions behaves like children transforms in hierarchy
    /// </summary>
    [System.Serializable]
    public partial class CheckerField
    {
        /// <summary> Origin, child squares are attached to this position like child transforms </summary>
        public Vector2Int Position
        {
            set { MoveToPosition(value); }
            get { return parentPosition; }
        }

        [SerializeField] private Vector2Int parentPosition;
        /// <summary> Beware from using it, but it's useful with walls seapration feature in facility generator </summary>
        public Vector3 FloatingOffset = Vector3.zero;

        /// <summary> Structure of the checker out of single boxes represented by Vector2Ints </summary>
        //public List<Vector2Int> ChildPos = new List<Vector2Int>();
        public FCheckerGraph<CheckerPos> ChildPos = new FCheckerGraph<CheckerPos>();
        public List<CheckerData> Datas = new List<CheckerData>();

        /// <summary> Auto generated bounds when changing squares for much faster collision checking etc. </summary>
        [HideInInspector] public List<CheckerBounds> Bounding = new List<CheckerBounds>();
        public Vector2Int LastSettedSize { get; private set; }

        /// <summary> If some FieldSetup is going to be performed with use of this checker field, it can be useful to know that in some custom generating cases </summary>
        [NonSerialized] public FieldSetup HelperReference = null;

        // All methods are placed in partial classes in separated files ----
    }
}