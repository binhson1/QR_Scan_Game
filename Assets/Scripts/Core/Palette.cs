using UnityEngine;

public static class Palette
{
    public static readonly Color Blush = Hex("F6D5D8");
    public static readonly Color DustyPink = Hex("E7A8B0");
    public static readonly Color Rose = Hex("C46B7A");
    public static readonly Color Champagne = Hex("F7E7CE");
    public static readonly Color Ink = Hex("5C3A42");
    public static readonly Color DeepRose = Hex("9B4455");
    public static readonly Color SoftWhite = Hex("FFF8F6");
    public static readonly Color Pearl = Hex("F4E6EA");
    public static readonly Color Gold = Hex("E8C992");
    public static readonly Color White = Color.white;
    public static readonly Color Overlay = new Color(0.36f, 0.18f, 0.22f, 0.48f);

    public static readonly Color[] Balloons =
    {
        Hex("E7A8B0"), Hex("F6C9CE"), Hex("C46B7A"), Hex("F7E7CE"),
        Hex("F4E6EA"), Hex("E8C992"), Hex("D98A98"), Hex("F2C9CE"), Hex("C9A0A8"),
        Hex("F3B8C0"), Hex("E6C7A8")
    };

    public static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString("#" + h, out var c);
        return c;
    }
}
