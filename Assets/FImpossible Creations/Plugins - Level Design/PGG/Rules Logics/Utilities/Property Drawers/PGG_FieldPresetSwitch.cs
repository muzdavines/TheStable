using UnityEngine;

public class PGG_FieldPresetSwitch : PropertyAttribute
{
    public string PropName;
    public string PropTooltip;
    public int LabelWidth;

    public PGG_FieldPresetSwitch(string propName, string tooltip = "", int labelWidth = 0)
    {
        PropName = propName;
        PropTooltip = tooltip;
        LabelWidth = labelWidth;
    }
}


public class PGG_FieldPresetSwitchRange : PGG_FieldPresetSwitch
{
    public float From;
    public float To;

    public PGG_FieldPresetSwitchRange(string propName, string tooltip = "", int labelWidth = 0, float from = 0f, float to = 1f):base(propName, tooltip, labelWidth)
    {
        From = from;
        To = to;
    }
}