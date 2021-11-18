/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EnergyBarToolkit {

[SelectionBase]
[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class FilledRendererUGUI : EnergyBarUGUIBase {

    #region Fields

    public Sprite spriteBar;
    public Material spriteBarMaterial;

    public ColorType spriteBarColorType;
    public Color spriteBarColor = Color.white;
    public Gradient spriteBarGradient;

    public GrowDirection growDirection = GrowDirection.LeftToRight;

    public float radialOffset = 0;
    public float radialLength = 1;

    public Vector3 barImageScale = new Vector3(1, 1, 1);
    public Vector3 barImageOffset = new Vector3(0, 0, 0);

    public Vector2 barImageRepeater = new Vector2(1, 1);  // Cipsoft change

    // blink effect
    public bool effectBlink = false;
    public float effectBlinkValue = 0.2f;
    public float effectBlinkRatePerSecond = 1f;
    public Color effectBlinkColor = new Color(1, 1, 1, 0);
    public BlinkOperator effectBlinkOperator = BlinkOperator.LessOrEqual;

    // sprite follow effect
    public bool effectFollow = false;
    public GameObject effectFollowObject;
    public Vector3 effectFollowOffset = Vector3.zero;
    public AnimationCurve effectFollowScaleX = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve effectFollowScaleY = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve effectFollowScaleZ = AnimationCurve.Linear(0, 1, 1, 1);
    public AnimationCurve effectFollowRotation = AnimationCurve.Linear(0, 0, 1, 0);
    public Gradient effectFollowColor;

    // tiled sprite effect
    public bool effectTiled = false;
    public Sprite effectTiledSprite;
    public Vector2 effectTiledTiling = new Vector2(2, 2);
    public Vector2 effectTiledStartOffset = new Vector2(0, 0);
    public Vector2 effectTiledOffsetChangeSpeed = new Vector2(-1f, 0);
    public Color effectTiledTint = Color.white;

    [SerializeField]
    private Image2 effectTilledImageBarMask;

    [SerializeField]
    private Image2 effectTilledImageTiles;


    [SerializeField]
    private int lastRebuildHash;
    [SerializeField]
    private bool dirty;

    [SerializeField]
    [FormerlySerializedAs("barImage")]
    private Image2 imageBar;

    [SerializeField]
    [FormerlySerializedAs("burnImage")]
    private Image2 imageBurn;

    #endregion

    #region Properties

    /// <summary>
    /// Set this to true if you want your bar blinking regardless of blinking effect configuration.
    /// Bar will be blinking until this value is set to false.
    /// </summary>
    public bool forceBlinking { get; set; }

    private bool Blink { get; set; }

    // return current bar color based on color settings and effect
    private float _effectBlinkAccum;

    #endregion

    #region Public Methods

    public override void SetNativeSize() {
        if (imageBar == null) {

            // try to create the bar now
            Rebuild();
            if (imageBar == null) {
                Debug.LogWarning("Cannot resize bar that has not been created yet");
                return;
            }
        }

        int num1 = Mathf.RoundToInt(imageBar.sprite.rect.width);
        int num2 = Mathf.RoundToInt(imageBar.sprite.rect.height);
        rectTransform.anchorMax = rectTransform.anchorMin;
        rectTransform.sizeDelta = new Vector2(num1, num2);
    }

    public bool GrowDirectionSupportedByFollowEffect(GrowDirection growDirection) {
        switch (growDirection) {
            case GrowDirection.LeftToRight:
            case GrowDirection.RightToLeft:
            case GrowDirection.TopToBottom:
            case GrowDirection.BottomToTop:
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Unity Slots

    protected override void Start() {
        base.Start();

        if (imageBar == null) {
            dirty = true;
        }
    }

    protected override void Update() {
        base.Update();

        UpdateRebuild();
        UpdateNonIntrusive();
    }

    #endregion

    #region Update Methods

    private void UpdateSize() {
        var thisRectTransform = rectTransform;

        Vector2 newSize = thisRectTransform.rect.size;

        for (int i = 0; i < createdChildren.Count; ++i) {
            var child = createdChildren[i];
            var otherRectTransform = child.GetComponent<RectTransform>();
            SetSize(otherRectTransform, newSize);
        }
    }

    // updates bar properties that do not need it to be rebuild
    private void UpdateNonIntrusive() {
        UpdateSize();
        UpdateBarScaleAndOffset();
        UpdateValue();
        UpdateBlinkEffect();
        UpdateBurnEffect();
        UpdateTiledEffect();
        UpdateFollowEffect();
        UpdateColor();
        UpdateRadialOffset();
    }

    private void UpdateBarScaleAndOffset() {
        if (imageBar != null) {
            var pivot = rectTransform.pivot;
            var rect = imageBar.rectTransform.rect;
            float ox = -(pivot.x - 0.5f) * rect.width;
            float oy = -(pivot.y - 0.5f) * rect.height;
            var computedLocalPosition = new Vector3(barImageOffset.x + ox, barImageOffset.y + oy, barImageOffset.z);

            MadTransform.SetLocalScale(imageBar.transform, barImageScale);
            
            MadTransform.SetLocalPosition(imageBar.transform, computedLocalPosition);

            if (effectTilledImageBarMask != null) {
                MadTransform.SetLocalScale(effectTilledImageBarMask.transform, barImageScale);
                MadTransform.SetLocalPosition(effectTilledImageBarMask.transform, computedLocalPosition);
            }
        }
        if (imageBurn != null)
        {
            var pivot = rectTransform.pivot;
            var rect = imageBurn.rectTransform.rect;
            float ox = -(pivot.x - 0.5f) * rect.width;
            float oy = -(pivot.y - 0.5f) * rect.height;
            var computedLocalPosition = new Vector3(barImageOffset.x + ox, barImageOffset.y + oy, barImageOffset.z);

            MadTransform.SetLocalScale(imageBurn.transform, barImageScale);
            
            MadTransform.SetLocalPosition(imageBurn.transform, computedLocalPosition);
        }
    }

    private void UpdateTiledEffect() {
        if (!effectTiled) {
            return;
        }

        if (effectTilledImageTiles.uvTiling != effectTiledTiling) {
            effectTilledImageTiles.uvTiling = effectTiledTiling;
            effectTilledImageTiles.SetVerticesDirty();
        }

        effectTilledImageTiles.color = effectTiledTint;

        if (Application.isPlaying) {
            effectTilledImageTiles.uvOffset += effectTiledOffsetChangeSpeed * Time.deltaTime;
            effectTilledImageTiles.SetVerticesDirty();
        } else {
            if (effectTilledImageTiles.uvOffset != effectTiledStartOffset) {
                effectTilledImageTiles.uvOffset = effectTiledStartOffset;
                effectTilledImageTiles.SetVerticesDirty();
            }
        }

        
    }

    private void UpdateValue() {
        if (imageBar == null) {
            return;
        }

        if (!Mathf.Approximately(imageBar.fillValue, ValueF2)) {
            imageBar.fillValue = ValueF2;
            imageBar.SetAllDirty();

            if (effectTilledImageBarMask != null) {
                effectTilledImageBarMask.fillValue = ValueF2;
                effectTilledImageBarMask.SetAllDirty();
            }
        }
    }

    private void UpdateBlinkEffect() {
        if (forceBlinking) {
            Blink = EnergyBarCommons.Blink(effectBlinkRatePerSecond, ref _effectBlinkAccum);
        } else if (CanBlink()) {
            Blink = EnergyBarCommons.Blink(effectBlinkRatePerSecond, ref _effectBlinkAccum);
        } else {
            Blink = false;
        }
    }

    private bool CanBlink() {
        if (!effectBlink) {
            return false;
        }

        switch (effectBlinkOperator) {
            case BlinkOperator.LessThan:
                return ValueF2 < effectBlinkValue;
            case BlinkOperator.LessOrEqual:
                return ValueF2 <= effectBlinkValue;
            case BlinkOperator.GreaterThan:
                return ValueF2 > effectBlinkValue;
            case BlinkOperator.GreaterOrEqual:
                return ValueF2 >= effectBlinkValue;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateBurnEffect() {
        if (imageBurn == null) {
            return;
        }

        if (Mathf.Approximately(ValueFBurn, ValueF2)) {
            imageBurn.enabled = false;
        } else {
            imageBurn.enabled = true;
            imageBurn.color = GetBurnColor();
            imageBurn.fillValue = ValueFBurn;
        }
    }

    private void UpdateFollowEffect() {
        if (!effectFollow) {
            return;
        }

        UpdateEffectFollowDefaults();

        Color color = effectFollowColor.Evaluate(ValueF2);
        float scaleX = effectFollowScaleX.Evaluate(ValueF2);
        float scaleY = effectFollowScaleY.Evaluate(ValueF2);
        float scaleZ = effectFollowScaleZ.Evaluate(ValueF2);
        float rotation = effectFollowRotation.Evaluate(ValueF2) * 360;

        if (effectFollowObject != null) {
            var worldPos = imageBar.transform.TransformPoint(EdgePosition());
            GameObject obj = effectFollowObject;
            MadTransform.SetPosition(obj.transform, worldPos);
            MadTransform.SetLocalScale(obj.transform, new Vector3(scaleX, scaleY, scaleZ));
            if (obj.GetComponent<Renderer>() != null) {
                obj.GetComponent<Renderer>().sharedMaterial.color = color;
            }
            var newEulerAngles = new Vector3(0, 0, rotation);
            if (obj.transform.localEulerAngles != newEulerAngles) {
                obj.transform.localEulerAngles = newEulerAngles;
            }

            // if object contains Graphic, then manipulate the color
            var graphics = obj.GetComponent<Graphic>();
            if (graphics) {
                graphics.color = color;
            }

        }


    }

    private void UpdateEffectFollowDefaults() {
        if (effectFollowScaleX.length == 0) {
            effectFollowScaleX = AnimationCurve.Linear(0, 1, 1, 1);
        }

        if (effectFollowScaleY.length == 0) {
            effectFollowScaleY = AnimationCurve.Linear(0, 1, 1, 1);
        }

        if (effectFollowScaleZ.length == 0) {
            effectFollowScaleZ = AnimationCurve.Linear(0, 1, 1, 1);
        }

        if (effectFollowRotation.length == 0) {
            effectFollowRotation = AnimationCurve.Linear(0, 0, 1, 0);
        }
    }

    private Vector2 EdgePosition() {
        if (spriteBar == null) {
            return Vector2.zero;
        }

        var drawingDimensions = imageBar.GetDrawingDimensions(false);

        float left = drawingDimensions.x;
        float right = drawingDimensions.z;
        float bottom = drawingDimensions.y;
        float top = drawingDimensions.w;
        float width = right - left;
        float height = top - bottom;

        float centerX = left + width / 2;
        float centerY = bottom + height / 2;
        Vector2 pos;

        switch (growDirection) {
            case GrowDirection.LeftToRight:
                pos = new Vector2(left + width * ValueF2, centerY);
                break;
            case GrowDirection.RightToLeft:
                pos = new Vector2(left + width * ValueF2, centerY);
                break;
            case GrowDirection.BottomToTop:
                pos = new Vector2(centerX, bottom + height * ValueF2);
                break;
            case GrowDirection.TopToBottom:
                pos = new Vector2(centerX, bottom + height * ValueF2);
                break;
            default:
                pos = Vector2.zero;
                break;
        }

        return new Vector2(pos.x + effectFollowOffset.x, pos.y + effectFollowOffset.y);
    }

    private Color GetBurnColor() {
        Color outColor = effectBurnSprite.color;
        if (Blink) {
            outColor = new Color(0, 0, 0, 0);
        }

        return ComputeColor(outColor);
    }

    private void UpdateColor() {
        var computedColor = ComputeBarColor();
        imageBar.color = computedColor;

        if (effectTiled) {
            effectTilledImageTiles.color = Multiply(new Color(1, 1, 1, computedColor.a), effectTiledTint);
        }
    }

    private void UpdateRadialOffset() {
        if (imageBar == null) {
            return;
        }

        imageBar.radialFillOffset = radialOffset;
        imageBar.radialFillLength = radialLength;

        if (imageBurn) {
            imageBurn.radialFillOffset = radialOffset;
            imageBurn.radialFillLength = radialLength;
        }
    }

    private Color ComputeBarColor() {
        Color outColor = Color.white;

        if (growDirection == GrowDirection.ColorChange) {
            outColor = spriteBarGradient.Evaluate(energyBar.ValueF);
        } else {
            switch (spriteBarColorType) {
                case ColorType.Solid:
                    outColor = spriteBarColor;
                    break;
                case ColorType.Gradient:
                    outColor = spriteBarGradient.Evaluate(energyBar.ValueF);
                    break;
                default:
                    MadDebug.Assert(false, "Unkwnown option: " + spriteBarColorType);
                    break;
            }
        }

        if (Blink) {
            outColor = effectBlinkColor;
        }

        return ComputeColor(outColor);
    }

    #endregion

    #region Rebuild Methods

    public void UpdateRebuild() {
        if (RebuildNeeded()) {
            Rebuild();
        }
    }

    private bool RebuildNeeded() {
        int ch = MadHashCode.FirstPrime;
        ch = MadHashCode.Add(ch, spriteBar != null ? spriteBar.GetInstanceID() : 0);
        ch = MadHashCode.Add(ch, spriteBarMaterial != null ? spriteBarMaterial.GetInstanceID() : 0);
        ch = MadHashCode.AddList(ch, spritesBackground);
        ch = MadHashCode.AddList(ch, spritesForeground);
        ch = MadHashCode.Add(ch, (int) spriteBarColorType);
        ch = MadHashCode.Add(ch, (int) growDirection);
        ch = MadHashCode.Add(ch, label);
        ch = MadHashCode.Add(ch, effectBurn);
        ch = MadHashCode.Add(ch, effectBurnSprite);
        ch = MadHashCode.Add(ch, rectTransform.pivot);
        ch = MadHashCode.Add(ch, effectTiled);
        ch = MadHashCode.Add(ch, effectTiledSprite);

        ch = MadHashCode.Add(ch, barImageRepeater);   // Cipsoft change

        //ch = HashAdd(ch, panel);
        //ch = HashAdd(ch, textureMode);
        //ch = HashAddArray(ch, texturesBackground);
        //ch = HashAddTexture(ch, textureBar);
        //ch = HashAddArray(ch, texturesForeground);
        //ch = HashAdd(ch, atlas);
        //ch = HashAddArray(ch, atlasTexturesBackground);
        //ch = HashAdd(ch, atlasTextureBarGUID);
        //ch = HashAddArray(ch, atlasTexturesForeground);
        //ch = HashAdd(ch, guiDepth);
        //ch = HashAdd(ch, growDirection);
        //ch = HashAdd(ch, effectBurn);
        //ch = HashAddTexture(ch, effectBurnTextureBar);
        //ch = HashAdd(ch, atlasEffectBurnTextureBarGUID);
        //ch = HashAdd(ch, labelEnabled);
        //ch = HashAdd(ch, labelFont);
        //ch = HashAdd(ch, effectFollow);
        //ch = HashAdd(ch, premultipliedAlpha);

        if (ch != lastRebuildHash || dirty) {
            lastRebuildHash = ch;
            dirty = false;
            return true;
        } else {
            return false;
        }
    }

    public void Rebuild() {
        RemoveCreatedChildren();

        BuildBackgroundImages();
        if (effectBurn) {
            BuildBurnBar();
        }
        BuildBar();

        if (effectTiled) {
            BuildEffectTilled();
        }

        BuildForegroundImages();

        UpdateSize();

        MoveLabelToTop();
    }

    private void BuildBurnBar() {
        imageBurn = CreateChild<Image2>("burn_bar");

        if (effectBurnSprite.sprite) {
            imageBurn.sprite = effectBurnSprite.sprite;
        } else {
            imageBurn.sprite = spriteBar;
        }

        imageBurn.material = effectBurnSprite.material;
        imageBurn.SetNativeSize();
        imageBurn.growDirection = growDirection;
        imageBurn.uvTiling = barImageRepeater;    // Cipsoft change
    }

    private void BuildBar() {
        imageBar = CreateChild<Image2>("bar");
        imageBar.sprite = spriteBar;
        imageBar.material = spriteBarMaterial;
        imageBar.SetNativeSize();
        imageBar.growDirection = growDirection;
        imageBar.readable = true;
        imageBar.uvTiling = barImageRepeater;   // Cipsoft change
    }

    private void BuildEffectTilled() {
        effectTilledImageBarMask = CreateChild<Image2>("bar_mask");
        effectTilledImageBarMask.sprite = spriteBar;
        effectTilledImageBarMask.SetNativeSize();
        effectTilledImageBarMask.growDirection = growDirection;
        effectTilledImageBarMask.readable = true;

        var mask = effectTilledImageBarMask.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        effectTilledImageTiles = CreateChild<Image2>("tiles", effectTilledImageBarMask.transform);
        effectTilledImageTiles.sprite = effectTiledSprite;
    }

    #endregion

    #region Inner Types

    public enum BlinkOperator {
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
    }

    #endregion
}

} // namespace
