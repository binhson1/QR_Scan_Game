using UnityEngine;

public static class ThemeFonts
{
    public static Font Regular { get; private set; }

    public static void Ensure()
    {
        if (Regular != null) return;
        Regular = Resources.Load<Font>("Fonts/Nunito");
        if (Regular == null)
            Regular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (Regular == null)
            Regular = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
