using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores base settings for a Module.
    /// </summary>
    [Serializable]
    public abstract class ModuleSettings
    {
        /// <summary>
        /// Brief description about this Module.
        /// </summary>
        public string Notes;

        /// <summary>
        /// Specifies whether to run a data validator check for this Module Setting.
        /// Note that if parent Stage's doesn't have data validator check enabled, then this Module Setting wouldn't be checked regardless.
        /// </summary>
        public bool DataValidatorEnabled;

        /// <summary>
        /// Padding constrain
        /// </summary>
        public Borders ConstrainPadding;

        /// <summary>
        /// If true, Module will be guaranteed to be within the Canvas.
        /// </summary>
        public bool ConstrainToCanvas;

        /// <summary>
        /// Position module will have
        /// This will be used if the POSITION_TYPE is Custom
        /// </summary>
        public Vector2 CustomPosition;

        /// <summary>
        /// Fade In Settings for this Module
        /// </summary>
        public FadeInEffectSettings FadeInEffectSettings = new FadeInEffectSettings();

        /// <summary>
        /// Float Settings for this Module
        /// </summary>
        public FloatEffectSettings FloatEffectSettings = new FloatEffectSettings();

        /// <summary>
        /// Fly In Settings for this Module
        /// </summary>
        public FlyInEffectSettings FlyInEffectSettings = new FlyInEffectSettings();

        /// <summary>
        /// Specifies how a Module is placed in relation to its UI Target.
        /// </summary>
        public PlacementType PlacementType = PlacementType.Center;

        /// <summary>
        /// The positioning mode given Module should go with.
        /// </summary>
        public PositionMode PositionMode = PositionMode.TransformBased;

        /// <summary>
        /// Position Offset affects how the final position of the module would look like.
        /// </summary>
        public Vector2 PositionOffset;

        /// <summary>
        /// Scale Pulsing Settings for this Module
        /// </summary>
        public ScalePulsingEffectSettings ScalePulsingEffectSettings = new ScalePulsingEffectSettings();

        /// <summary>
        /// The target canvas where this Module will operate in.
        /// </summary>
        public Canvas TargetCanvas;

        /// <summary>
        /// Specify which type of target should this module target.
        /// </summary>
        public TargetType TargetType = TargetType.CanvasSpace;

        /// <summary>
        /// The transform of a GameObject to which this Module will position itself.
        /// </summary>
        public Transform TransformTarget;

        /// <summary>
        /// The transform of UI element which this Module will use for positioning.
        /// </summary>
        public RectTransform UITarget;

        /// <summary>
        /// If true, the position of this Module will be updated every frame.
        /// </summary>
        public bool UpdateEveryFrame;
    }
}