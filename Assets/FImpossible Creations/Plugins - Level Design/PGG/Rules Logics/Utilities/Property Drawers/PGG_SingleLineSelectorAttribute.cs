using UnityEngine;

public class PGG_SingleLineSelector : PropertyAttribute
{
    public string[] PropNames;
    public int Width;
    public string PropTooltip;
    public int LabelWidth;
    public int UpPadding;

    public PGG_SingleLineSelector(string[] propNames, int width = 68, string tooltip = "", int labelWidth = 0, int upPadding = 0)
    {
        PropNames = propNames;
        Width = width;
        PropTooltip = tooltip;
        LabelWidth = labelWidth;
        UpPadding = upPadding;
    }

}
