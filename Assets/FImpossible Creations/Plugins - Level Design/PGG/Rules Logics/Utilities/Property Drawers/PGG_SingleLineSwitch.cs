using UnityEngine;

public class PGG_SingleLineSwitch : PropertyAttribute
{
    public string PropName;
    public int Width;
    public string PropTooltip;
    public int LabelWidth;
    public int UpPadding;

    public PGG_SingleLineSwitch(string propName, int width = 68, string tooltip = "", int labelWidth = 0, int upPadding = 0)
    {
        PropName = propName;
        Width = width;
        PropTooltip = tooltip;
        LabelWidth = labelWidth;
        UpPadding = upPadding;
    }

}
