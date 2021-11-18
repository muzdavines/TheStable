/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using EnergyBarToolkit;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class EnergyBarBase : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    [SerializeField]
    protected int version = 169;  // EBT version number to help updating properties
    
    // Label
    public bool labelEnabled;
    public Vector2 labelPosition;
    public bool labelPositionNormalized = true;
    
    public string labelFormat = "{cur}/{max}";
    public Color labelColor = Color.white;
    
    // smooth effect
    public bool effectSmoothChange = false;          // smooth change value display over time
    public float effectSmoothChangeSpeed = 0.5f;    // value bar width percentage per second
    public SmoothDirection effectSmoothChangeDirection = SmoothDirection.Both;
    public Notify effectSmoothChangeFinishedNotify = new Notify();

    private bool effectSmoothChangeWorking = false;

    // burn effect
    public bool effectBurn = false;                 // bar draining will display 'burn' effect
    public Texture2D effectBurnTextureBar;
    public string atlasEffectBurnTextureBarGUID = "";
    public Color effectBurnTextureBarColor = Color.red;
    public Notify effectBurnFinishedNotify = new Notify();
    public BurnDirection effectBurnDirection = BurnDirection.OnlyWhenDecreasing;

    private bool effectBurnWorking = false;

    // reference to actual bar component    
    protected EnergyBar energyBar {
        get {
            if (_energyBar == null) {
                _energyBar = GetComponent<EnergyBar>();
                MadDebug.Assert(_energyBar != null, "Cannot access energy bar?!");
            }

            return _energyBar;
        }
    }

    private EnergyBar _energyBar;

    protected float ValueFBurn;
    protected float ValueF2;
    
    // ===========================================================
    // Getters / Setters
    // ===========================================================

    public abstract Rect DrawAreaRect { get; }

    public bool isBurning {
        get { return effectBurn && Math.Abs(ValueF2 - ValueFBurn) > 0.001f; }
    }
    
    protected float ValueF {
        get {
            return energyBar.ValueF;
        }
    }
    
    protected Vector2 LabelPositionPixels {
        get {
            var rect = DrawAreaRect;
            Vector2 v;
            if (labelPositionNormalized) {
                v = new Vector2(rect.x + labelPosition.x * rect.width, rect.y + labelPosition.y * rect.height);
            } else {
                v = new Vector2(rect.x + labelPosition.x, rect.y + labelPosition.y);
            }
            
            return v;
        }
    }

    /// <summary>
    /// Currently displayed value by the renderer (after applying effects)
    /// </summary>
    public float displayValue {
        get {
            return ValueF2;
        }
    }
    
    /// <summary>
    /// Global opacity value.
    /// </summary>
    public float opacity {
        get {
            return _tint.a;
        }
        set {
            _tint.a = Mathf.Clamp01(value);
        }
    }
    
    /// <summary>
    /// Global tint value
    /// </summary>
    public Color tint {
        get {
            return _tint;
        }
        set {
            _tint = value;
        }
    }
    [SerializeField]
    Color _tint = Color.white;

    /// <summary>
    /// If the bar is currently burning
    /// </summary>
    public bool burning {
        get {
            return effectBurn && ValueF2 != ValueFBurn;
        }
    }

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    // ===========================================================
    // Methods
    // ===========================================================

    /// <summary>
    /// Resets animations state, so bar value can be changed without any animation.
    /// It's useful for objects pooling when you want to reuse bars. Should be executed
    /// after setting the value, but before displaying.
    /// </summary>
    public virtual void ResetAnimations() {
        ValueF2 = ValueF;
        ValueFBurn = ValueF;
    }

    protected virtual void OnEnable() {
        ValueF2 = ValueF;
    }
    
    protected virtual void OnDisable() {
        // do nothing
    }
    
    protected virtual void Start() {
        // do nothing
    }

    protected virtual void Update() {
        UpdateAnimations();
    }

    void UpdateAnimations() {
        UpdateBarValue();
        UpdateBurnValue();
    }

    void UpdateBurnValue() {
        EnergyBarCommons.SmoothDisplayValue(
                       ref ValueFBurn, ValueF2, effectSmoothChangeSpeed);
        ValueFBurn = Mathf.Max(ValueFBurn, ValueF2);
        switch (effectBurnDirection) {
            case BurnDirection.Both:
                if (ValueF > ValueF2) {
                    ValueFBurn = ValueF;
                } else if (ValueF < ValueF2) {
                    EnergyBarCommons.SmoothDisplayValue(
                        ref ValueFBurn, ValueF, effectSmoothChangeSpeed);
                } else {
                    ValueFBurn = Mathf.Max(ValueFBurn, ValueF2);
                }
                
                break;
            case BurnDirection.OnlyWhenDecreasing:
                if (ValueF < ValueF2) {
                    EnergyBarCommons.SmoothDisplayValue(
                        ref ValueFBurn, ValueF, effectSmoothChangeSpeed);
                } else {
                    ValueFBurn = Mathf.Max(ValueFBurn, ValueF2);
                }

                break;
            case BurnDirection.OnlyWhenIncreasing:
                if (ValueF > ValueF2) {
                    ValueFBurn = ValueF;
                } else {
                    ValueFBurn = ValueF2;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!Mathf.Approximately(ValueFBurn, ValueF2)) {
            effectBurnWorking = true;
        } else if (effectBurnWorking) {
            effectBurnFinishedNotify.Execute(this);
            effectBurnWorking = false;
        }
        
    }

    void UpdateBarValue() {
        if (effectBurn) {
            if (effectSmoothChange) {
                // in burn mode smooth primary bar only when it's increasing
                bool canGoUp = effectSmoothChangeDirection == SmoothDirection.Both
                    || effectSmoothChangeDirection == SmoothDirection.OnlyWhenIncreasing;

                if (ValueF > ValueF2 && canGoUp) {
                    EnergyBarCommons.SmoothDisplayValue(ref ValueF2, ValueF, effectSmoothChangeSpeed);
                } else {
                    ValueF2 = energyBar.ValueF;
                }

                if (!Mathf.Approximately(ValueF, ValueF2)) {
                    effectSmoothChangeWorking = true;
                } else if (effectSmoothChangeWorking) {
                    effectSmoothChangeFinishedNotify.Execute(this);
                    effectSmoothChangeWorking = false;
                }

            } else {
                ValueF2 = energyBar.ValueF;
            }

        } else {
            if (effectSmoothChange) {
                bool canGoUp = effectSmoothChangeDirection == SmoothDirection.Both
                    || effectSmoothChangeDirection == SmoothDirection.OnlyWhenIncreasing;
                bool canGoDown = effectSmoothChangeDirection == SmoothDirection.Both
                    || effectSmoothChangeDirection == SmoothDirection.OnlyWhenDecreasing;

                if ((ValueF > ValueF2 && canGoUp) || (ValueF < ValueF2 && canGoDown)) {
                    EnergyBarCommons.SmoothDisplayValue(ref ValueF2, ValueF, effectSmoothChangeSpeed);
                } else {
                    ValueF2 = energyBar.ValueF;
                }

                if (!Mathf.Approximately(ValueF, ValueF2)) {
                    effectSmoothChangeWorking = true;
                } else if (effectSmoothChangeWorking) {
                    effectSmoothChangeFinishedNotify.Execute(this);
                    effectSmoothChangeWorking = false;
                }
            } else {
                ValueF2 = energyBar.ValueF;
            }
        }
    }
    
    protected bool RepaintPhase() {
        return Event.current.type == EventType.Repaint;
    }

    private string labelCache;
    private int labelCacheParamsHash;
    
    protected string LabelFormatResolve(string format) {
        int hash = MadHashCode.FirstPrime;
        hash = MadHashCode.Add(hash, format);
        hash = MadHashCode.Add(hash, energyBar.valueCurrent);
        hash = MadHashCode.Add(hash, energyBar.valueMin);
        hash = MadHashCode.Add(hash, energyBar.valueMax);
        hash = MadHashCode.Add(hash, energyBar.ValueF);

        if (labelCacheParamsHash == hash) {
            return labelCache;
        }

        format = format.Replace("{cur}", "" + energyBar.valueCurrent);
        format = format.Replace("{min}", "" + energyBar.valueMin);
        format = format.Replace("{max}", "" + energyBar.valueMax);
        format = format.Replace("{cur%}", string.Format("{0:00}", energyBar.ValueF * 100));
        format = format.Replace("{cur2%}", string.Format("{0:00.0}", energyBar.ValueF * 100));
        try {
            format = string.Format(format, energyBar.valueCurrent, energyBar.valueMin,
                energyBar.valueMax, energyBar.ValueF, energyBar.ValueF * 100);
        } catch (IndexOutOfRangeException) {
            // ignore
        } catch (FormatException) {
            // ignore
        }

        labelCache = format;
        labelCacheParamsHash = hash;

        return format;
    }
    
    protected Vector4 ToVector4(Rect r) {
        return new Vector4(r.xMin, r.yMax, r.xMax, r.yMin);
    }
    
    protected Vector2 Round(Vector2 v) {
        return new Vector2(Mathf.Round(v.x), Mathf.Round (v.y));
    }
    
    protected virtual bool IsVisible() {
        if (opacity == 0) {
            return false;
        }

        return true;
    }
    
    protected Color PremultiplyAlpha(Color c) {
        return new Color(c.r * c.a, c.g * c.a, c.b * c.a, c.a);
    }
    
    protected virtual Color ComputeColor(Color localColor) {
        Color outColor = Multiply(localColor, tint);
        return outColor;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    protected static Color Multiply(Color c1, Color c2) {
        return new Color(c1.r * c2.r, c1.g * c2.g, c1.b * c2.b, c1.a * c2.a);
    }

    protected static int HashAdd(int current, bool obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, int obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, float obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, UnityEngine.Object obj) {
        if (obj != null) {
            return MadHashCode.Add(current, obj.GetInstanceID());
        } else {
            return MadHashCode.Add(current, null);
        }
    }

    protected static int HashAdd(int current, object obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAddTexture(int current, Texture texture) {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(texture);
        string guid = AssetDatabase.AssetPathToGUID(path);
        return MadHashCode.Add(current, guid);
#else
        return MadHashCode.Add(current, texture);
#endif
    }

    protected static int HashAddArray(int current, object[] arr) {
        return MadHashCode.AddArray(current, arr);
    }

    protected static int HashAddTextureArray(int current, Texture[] arr, string name = "") {
#if UNITY_EDITOR

        for (int i = 0; i < arr.Length; ++i) {
            string path = AssetDatabase.GetAssetPath(arr[i]);
            string guid = AssetDatabase.AssetPathToGUID(path);
            current =  MadHashCode.Add(current, guid);
        }

        return current;
#else
        return MadHashCode.AddArray(current, arr);
#endif
    }

    protected Rect FindBounds(Texture2D texture) {
        
        int left = -1, top = -1, right = -1, bottom = -1;
        bool expanded = false;
        Color32[] pixels;
        try {
            pixels = texture.GetPixels32();
        } catch (UnityException) { // catch not readable
            return new Rect();
        }
            
        int w = texture.width;
        int h = texture.height;
        int x = 0, y = h - 1;
        for (int i = 0; i < pixels.Length; ++i) {
            var c = pixels[i];
            if (c.a != 0) {
                Expand(x, y, ref left, ref top, ref right, ref bottom);
                expanded = true;
            }
            
            if (++x == w) {
                y--;
                x = 0;
            }
        }
        
        
        MadDebug.Assert(expanded, "bar texture has no visible pixels");
        
        var pixelsRect = new Rect(left, top, right - left + 1, bottom - top + 1);
        var normalizedRect = new Rect(
            pixelsRect.xMin / texture.width,
            1 - pixelsRect.yMax / texture.height,
            pixelsRect.xMax / texture.width - pixelsRect.xMin / texture.width,
            1 - pixelsRect.yMin / texture.height - (1 - pixelsRect.yMax / texture.height));
            
        return normalizedRect;
    }
    
    protected void Expand(int x, int y, ref int left, ref int top, ref int right, ref int bottom) {
        if (left == -1) {
            left = right = x;
            top = bottom = y;
        } else {
            if (left > x) {
                left = x;
            } else if (right < x) {
                right = x;
            }
            
            if (top > y) {
                top = y;
            } else if (bottom == -1 || bottom < y) {
                bottom = y;
            }    
        }
    }

    // ===========================================================
    // Static Methods
    // ===========================================================
    
    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    public enum Pivot {
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        Center,
    }
    
    [System.Serializable]
    public class Tex : AbstractTex {
        public virtual int width { get { return texture.width; } }
        public virtual int height { get { return texture.height; } }
        
        public virtual bool Valid {
            get {
                return texture != null;
            }
        }
    
        public Texture2D texture;
        
        public override int GetHashCode() {
            int hash = MadHashCode.FirstPrime;
            hash = HashAddTexture(hash, texture);
            //hash = HashAdd(hash, color);
            
            return hash;
        }
    }
    
    public class AbstractTex {
        public Color color = Color.white;
    }
    
    public enum GrowDirection {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
        RadialCW,
        RadialCCW,
        ExpandHorizontal,
        ExpandVertical,
        ColorChange,
    }
          
    public enum ColorType {
        Solid,
        Gradient,
    }

    public enum SmoothDirection {
        Both,
        OnlyWhenDecreasing,
        OnlyWhenIncreasing,
    }

    public enum BurnDirection {
        Both,
        OnlyWhenDecreasing,
        OnlyWhenIncreasing,
    }

    public abstract class TransformFunction {
        public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    }

    [System.Serializable]
    public class TranslateFunction : TransformFunction {
        public Vector2 startPosition;
        public Vector2 endPosition;

        public Vector2 Value(float progress) {
            progress = Mathf.Clamp01(progress);
            progress = animationCurve.Evaluate(progress);

            var result = new Vector2(
                    startPosition.x + (endPosition.x - startPosition.x) * progress,
                    startPosition.y + (endPosition.y - startPosition.y) * progress
                );
            return result;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            
            if (!(obj is TranslateFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startPosition.GetHashCode();
            hash = hash * 23 + endPosition.GetHashCode();
            return hash;
        }

    }

    [System.Serializable]
    public class ScaleFunction : TransformFunction {
        public Vector2 startScale = Vector3.one;
        public Vector2 endScale = Vector3.one;

        public Vector3 Value(float progress) {
            progress = Mathf.Clamp01(progress);
            progress = animationCurve.Evaluate(progress);

            var result = new Vector2(
                    startScale.x + (endScale.x - startScale.x) * progress,
                    startScale.y + (endScale.y - startScale.y) * progress
                );

            return new Vector3(result.x, result.y, 1);
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            if (!(obj is ScaleFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startScale.GetHashCode();
            hash = hash * 23 + endScale.GetHashCode();
            return hash;
        }
    }

    [System.Serializable]
    public class RotateFunction : TransformFunction {
        public float startAngle;
        public float endAngle;

        public Quaternion Value(float progress) {
            progress = Mathf.Clamp01(progress);
            progress = animationCurve.Evaluate(progress);

            float angle = startAngle + (endAngle - startAngle) * progress;

            var result = Quaternion.Euler(0, 0, angle);
            return result;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            if (!(obj is RotateFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startAngle.GetHashCode();
            hash = hash * 23 + endAngle.GetHashCode();
            return hash;
        }
    }

    [Serializable]
    public class Notify {
        public MonoBehaviour receiver;
        public string methodName;
        public event Action<EnergyBarBase> eventReceiver;

        public void Execute(EnergyBarBase sender) {
            if (receiver != null && !string.IsNullOrEmpty(methodName)) {
                receiver.SendMessage(methodName, sender);
            }

            if (eventReceiver != null) {
                eventReceiver(sender);
            }
        }
    }
    
}
