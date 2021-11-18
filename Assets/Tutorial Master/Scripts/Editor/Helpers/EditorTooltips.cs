using System.Collections.Generic;

// ReSharper disable All

namespace HardCodeLab.TutorialMaster.EditorUI
{
    /// <summary>
    /// Stores all tooltips for every editor field
    /// </summary>
    public class EditorTooltips
    {
        /// <summary>
        /// The tooltips dictionary that stores all tooltips and their corresponding tooltips
        /// </summary>
        public static readonly Dictionary<string, string> TooltipsDictionary = new Dictionary<string, string>
        {
           // == LOCALIZATION SETINGS ==
           {
               "LocalizationData.Languages.Name",
               "Language Name"
           },

           // == GENERAL SETTINGS ==
           {
               "PlayOnStart",
               "If true, a specified Tutorial will start as soon as the game begins."
           },

           // == TUTORIAL SETTINGS ==
           {
               "Tutorials.Name",
               "Tutorial Name"
           },

           // == TUTORIAL EVENTS ==
           {
               "Tutorials.Events.OnTutorialStart",
               "Specifies a list of actions that will be invoked when this Tutorial Starts."
           },
           {
               "Tutorials.Events.OnTutorialEnd",
               "Specifies a list of actions that will be invoked when this Tutorial Ends."
           },
           {
               "Tutorials.Stages.Modules.Popups.Settings.PlacementType",
               "Determines the placement type"
           },
           {
               "Tutorials.Stages.Name",
               "The name of the Stage"
           },

           // == STAGE TRIGGER ==
           {
               "Tutorials.Stages.Trigger.Type",
               "Specify what kind of Trigger do you want this Stage to have"
           },
           {
               "Tutorials.Stages.Trigger.TriggerActivationDelay",
               "How long to wait before triggers are activated?"
           },
           {
               "Tutorials.Stages.Trigger.OnDelayEnd",
               "Specifies a list of actions that will be invoked when the Delay Ends"
           },
           {
               "Tutorials.Stages.Trigger.OnTriggerActivate",
               "Specifies a list of actions that will be invoked when this Trigger Activates"
           },
           {
               "Tutorials.Stages.Trigger.TriggerActivationEvent",
               "Additional event to do when the Trigger is activated"
           },
           {
               "Tutorials.Stages.Trigger.TriggerKey",
               "Id that will activate this Trigger"
           },
           {
               "Tutorials.Stages.Trigger.TriggerInput",
               "Input that will activate this Trigger"
           },
           {
               "Tutorials.Stages.Trigger.UIButtonTarget",
               "UI Button that will activate this Trigger when clicked"
           },
           {
               "Tutorials.Stages.Trigger.TriggerTimerAmount",
               "Timer that will cause a Trigger to be activated once elapsed"
           },

           // == STAGE EVENTS ==
           {
               "Tutorials.Stages.Events.OnStagePlay",
               "Specifies a list of actions that will be invoked when this Stage Starts"
           },
           {
               "Tutorials.Stages.Events.OnStageExit",
               "Specifies a list of actions that will be invoked when this Stage Stops"
           },

           // == STAGE AUDIO ==
           {
               "Tutorials.Stages.Audio.Timing",
               "Specify when do you want an Audio to play for this Stage"
           },
           {
               "Tutorials.Stages.Audio.Source",
               "Audio Source Component from which an Audio clip will be played"
           },
           {
               "Tutorials.Stages.Audio.Clip",
               "Which audio clip to play?"
           },

           // == STAGE MODULES ==
           // BASE MODULE SETTINGS
           {
               "Tutorials.Stages.Modules.Arrows.OverridePrefab",
               "If set to true, this module will not use one of the prefabs from module pool and would accept a custom one."
           },
           {
               "Tutorials.Stages.Modules.Settings.UpdateEveryFrame",
               "If true, Module position will be updated every frame. "
               + "Useful if you're dealing with dynamic or moving UI elements. "
               + "Use sparingly to avoid unwanted performance drops."
           },
           {
               "Tutorials.Stages.Modules.Module",
               "Module GameObject that will be utilized for this Stage. Be sure to not pick an already chosen Module to avoid any conflicts."
           },
           {
               "Tutorials.Stages.Modules.Settings.TargetType",
               "Which type of Target should this module support?"
           },
           {
               "Tutorials.Stages.Modules.Settings.TargetCanvas",
               "Canvas in which this Module will reside in"
           },
           {
               "Tutorials.Stages.Modules.Settings.PositionMode",
               "How will this Module be positioned?"
           },
           {
               "Tutorials.Stages.Modules.Settings.UITarget",
               "A UI element whose position and size would determine the positon of this Module"
           },
           {
               "Tutorials.Stages.Modules.Settings.CustomPosition",
               "A position where this Module will be placed within the Canvas"
           },
           {
               "Tutorials.Stages.Modules.Settings.TransformTarget",
               "Transform of a GameObject whose position would determine the positon of this Module"
           },
           {
               "Tutorials.Stages.Modules.Settings.ConstrainToCanvas",
               "If true, the positon of the Module will be strictly enforced to be inside of a Canvas it resides in"
           },
           {
               "Tutorials.Stages.Modules.Settings.PlacementType",
               "How this Module will be placed in relation to a specified UI Element?"
           },
           {
               "Tutorials.Stages.Modules.Settings.PositionOffset",
               "Additional offset for the position."
           },
           {
               "Tutorials.Stages.Modules.Settings.RotationOffset.x",
               "Rotation Offset for X Axis"
           },
           {
               "Tutorials.Stages.Modules.Settings.RotationOffset.y",
               "Rotation Offset for Y Axis"
           },
           {
               "Tutorials.Stages.Modules.Settings.RotationOffset.z",
               "Rotation Offset for Z Axis"
           },
           {
               "Tutorials.Stages.Modules.Settings.RotationOffset.w",
               "Rotation Offset for W Axis"
           },

           // MODULE PROPERTIES - ARROW
           {
               "Tutorials.Stages.Modules.Settings.PointDirection",
               "Direction in which to point an Arrow to"
           },
           {
               "Tutorials.Stages.Modules.Settings.PointTarget",
               "Transform of a target towards which this Arrow should rotate to"
           },

           // MODULE PROPERTIES - POPUP
           {
               "Tutorials.Stages.Modules.Settings.PopupImage",
               "Sprite that will be replaced on a Popup"
           },
           {
               "Tutorials.Stages.Modules.Settings.ShowButton",
               "If true, the button for this Popup will not be hidden"
           },
           {
               "Tutorials.Stages.Modules.Settings.ButtonClickEvent",
               "Specify what happens when a Popup button is clicked"
           },

           // MODULE PROPERTIES - HIGHLIGHTER
           {
               "Tutorials.Stages.Modules.Settings.SizeType",
               "How this Module should set its size?"
           },
           {
               "Tutorials.Stages.Modules.Settings.UITransformReference",
               "UI Transform whose size will be used for this Module"
           },
           {
               "Tutorials.Stages.Modules.Settings.SizeOffset",
               "Addtional sizing for this Module. "
               + "Great if you want to have a gap between highlighter and UI element."
           },
           {
               "Tutorials.Stages.Modules.Settings.CustomSize",
               "Custom size for this Module"
           },

           // MODULE PROPERTIES - IMAGE
           {
               "Tutorials.Stages.Modules.Settings.SpriteContent",
               "Sprite Image that will be used for this Image Module."
           },

           // == MODULE EFFECTS ==
           // FADE IN
           {
               "Tutorials.Stages.Modules.Settings.FadeInEffectSettings.Speed",
               "Specify how fast this Module will fade into visibility"
           },
           {
               "Tutorials.Stages.Modules.Settings.FadeInEffectSettings.CanInteract",
               "If true, this Module can be interacted with even during the fading process"
           },

           // FLOAT
           {
               "Tutorials.Stages.Modules.Settings.FloatEffectSettings.Speed",
               "Specify how fast this Module will be floating"
           },
           {
               "Tutorials.Stages.Modules.Settings.FloatEffectSettings.Direction",
               "Specify direction in which floating will occur"
           },
           {
               "Tutorials.Stages.Modules.Settings.FloatEffectSettings.FloatRange",
               "How far should Module float from its initial position?"
           },
           {
               "Tutorials.Stages.Modules.Settings.FloatEffectSettings.CustomPattern",
               "A custom pattern which this floating effect will follow. Note that results can be unpredictable if not set correctly."
           },
           {
               "Tutorials.Stages.Modules.Settings.FloatEffectSettings.FloatPattern",
               "What kind of pattern should floating effect utilize?"
           },

           // FLY IN
           {
               "Tutorials.Stages.Modules.Settings.FlyInEffectSettings.Speed",
               "How fast should Module fly into the scene?"
           },
           {
               "Tutorials.Stages.Modules.Settings.FlyInEffectSettings.CanInteract",
               "If true, this Module can be interacted with even during the fading process"
           },
           {
               "Tutorials.Stages.Modules.Settings.FlyInEffectSettings.FlyDirection",
               "From which direction should this Module fly in?"
           },
           {
               "Tutorials.Stages.Modules.Settings.FlyInEffectSettings.FlyDistance",
               "What's the distance this Module has to travel to reach its destination? "
               + "The further it is, the longer it would take and higher speed it would require to arrive."
           },

           // SCALE PULSING
           {
               "Tutorials.Stages.Modules.Settings.ScalePulsingEffectSettings.Speed",
               "How fast should this Module pulse? Setting it too high would make Module too painful too look at!"
           },
           {
               "Tutorials.Stages.Modules.Settings.ScalePulsingEffectSettings.WidthRange",
               "Width range of this Module"
           },
           {
               "Tutorials.Stages.Modules.Settings.ScalePulsingEffectSettings.HeightRange",
               "Height range of this Module"
           },
       };
    }
}