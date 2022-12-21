using UnityEngine;
using UnityEngine.Rendering;

public class FReflectionProbePreset : ScriptableObject
{
    public ReflectionProbeMode Type = ReflectionProbeMode.Baked;
    public ReflectionProbeRefreshMode RefreshMode = ReflectionProbeRefreshMode.OnAwake;
    public ReflectionProbeTimeSlicingMode TimeSlicing = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;

    [Header("Runtime Settings")]
    public int Importance = 1;
    public float Intensity = 1f;
    [Space(4)]
    public bool BoxProjection = false;
    public float blendDistance = 1f;
    [Space(4)]
    public Vector3 BoxSize = new Vector3(10f, 10f, 10f);
    public Vector3 BoxOffset = Vector3.zero;

    [Header("Cubemap Capture Settings")]
    public int Resolution = 256;
    public bool HDR = true;

    [Space(4)]
    public float ShadowDistance = 100;
    public ReflectionProbeClearFlags ClearFlags = ReflectionProbeClearFlags.SolidColor;
    public Color BackgroundColor = Color.black;

    [Space(4)]
    public LayerMask CullingMask = ~(0 << 0);
    public float Far = 1000;
    public float Near = 0.1f;

    public void AssignSettingsTo(ReflectionProbe probe)
    {
        probe.mode = Type;
        probe.refreshMode = RefreshMode;
        probe.timeSlicingMode = TimeSlicing;
        probe.importance = Importance;
        probe.intensity = Intensity;
        probe.boxProjection = BoxProjection;
        probe.blendDistance = blendDistance;
        probe.size = BoxSize;
        probe.center = BoxOffset;
        probe.resolution = Resolution;
        probe.hdr = HDR;
        probe.shadowDistance = ShadowDistance;
        probe.clearFlags = ClearFlags;
        probe.backgroundColor = BackgroundColor;
        probe.cullingMask = CullingMask;
        probe.nearClipPlane = Near;
        probe.farClipPlane = Far;
    }

    public void CopySettingsFrom(ReflectionProbe probe)
    {
        Type = probe.mode;
        RefreshMode = probe.refreshMode;
        TimeSlicing = probe.timeSlicingMode;
        Importance = probe.importance;
        Intensity = probe.intensity;
        BoxProjection = probe.boxProjection;
        blendDistance = probe.blendDistance;
        BoxSize = probe.size;
        BoxOffset = probe.center;
        Resolution = probe.resolution;
        HDR = probe.hdr;
        ShadowDistance = probe.shadowDistance;
        ClearFlags = probe.clearFlags;
        BackgroundColor = probe.backgroundColor;
        CullingMask = probe.cullingMask;
        Near = probe.nearClipPlane;
        Far = probe.farClipPlane;
    }

}

