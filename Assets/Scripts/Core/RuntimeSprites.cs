using UnityEngine;
using UnityEngine.UI;

public static class RuntimeSprites
{
    public static Sprite White { get; private set; }
    public static Sprite Circle { get; private set; }
    public static Sprite Rounded { get; private set; }
    public static Sprite Balloon { get; private set; }
    public static Sprite Bokeh { get; private set; }
    public static Sprite Gradient { get; private set; }

    static bool _ready;

    public static void BindAssets(Sprite white, Sprite circle, Sprite rounded, Sprite balloon, Sprite bokeh, Sprite gradient)
    {
        White = white;
        Circle = circle;
        Rounded = rounded;
        Balloon = balloon;
        Bokeh = bokeh;
        Gradient = gradient;
        _ready = White != null && Circle != null && Rounded != null && Balloon != null;
    }

    public static void Ensure()
    {
        if (_ready && White != null) return;
        White = Resources.Load<Sprite>("UI/ui_white");
        Circle = Resources.Load<Sprite>("UI/ui_circle");
        Rounded = Resources.Load<Sprite>("UI/ui_rounded");
        Balloon = Resources.Load<Sprite>("UI/ui_balloon");
        Bokeh = Resources.Load<Sprite>("UI/ui_bokeh");
        Gradient = Resources.Load<Sprite>("UI/ui_gradient");
        if (White != null && Circle != null && Rounded != null && Balloon != null)
        {
            if (Bokeh == null) Bokeh = Circle;
            if (Gradient == null) Gradient = White;
            _ready = true;
            return;
        }
        White = Sprite.Create(Solid(Color.white, 8, 8), new Rect(0, 0, 8, 8), Vector2.one * 0.5f, 8f);
        Circle = MakeSprite(Disk(128, 0.48f), 100f);
        Rounded = MakeSliced(RoundedRect(64, 18), 16f);
        Balloon = MakeSprite(BalloonTex(256), 100f);
        Bokeh = MakeSprite(Disk(64, 0.42f, 0.28f), 64f);
        Gradient = MakeSprite(VerticalGradient(16, 64, Palette.Blush, Palette.Champagne), 16f);
        _ready = true;
    }

    static Sprite MakeSprite(Texture2D tex, float ppu)
    {
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, ppu);
    }

    static Sprite MakeSliced(Texture2D tex, float border)
    {
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f, 0,
            SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    static Texture2D Solid(Color c, int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        t.SetPixels(px);
        return t;
    }

    static Texture2D Disk(int size, float radius, float feather = 1.4f)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        float cx = (size - 1) * 0.5f;
        float r = size * radius;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cx));
            float a = Mathf.Clamp01((r - d) / feather);
            px[y * size + x] = new Color(1f, 1f, 1f, a * a);
        }
        t.SetPixels(px);
        return t;
    }

    static Texture2D RoundedRect(int size, int radius)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float a = CornerAlpha(x, y, size, size, radius);
            px[y * size + x] = new Color(1f, 1f, 1f, a);
        }
        t.SetPixels(px);
        return t;
    }

    static float CornerAlpha(int x, int y, int w, int h, int r)
    {
        int cx = x < r ? r : (x >= w - r ? w - 1 - r : x);
        int cy = y < r ? r : (y >= h - r ? h - 1 - r : y);
        if (x >= r && x < w - r && y >= r && y < h - r) return 1f;
        float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        return Mathf.Clamp01(r - d + 0.5f);
    }

    static Texture2D VerticalGradient(int w, int h, Color top, Color bottom)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            var c = Color.Lerp(bottom, top, y / (h - 1f));
            for (int x = 0; x < w; x++) t.SetPixel(x, y, c);
        }
        return t;
    }

    static Texture2D BalloonTex(int size)
    {
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        float cx = size * 0.5f;
        float cy = size * 0.56f;
        float r = size * 0.38f;
        Vector2 shine = new Vector2(size * 0.38f, size * 0.72f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x - cx) / r;
            float dy = (y - cy) / (r * 1.08f);
            float d = dx * dx + dy * dy;
            float body = Mathf.Clamp01((1.05f - d) * 18f);
            float knot = 0f;
            Vector2 k = new Vector2(x - cx, y - size * 0.12f);
            if (y < size * 0.22f)
            {
                float tri = Mathf.Clamp01(1f - Mathf.Abs(k.x) / (size * 0.06f) - Mathf.Abs(k.y) / (size * 0.08f));
                knot = tri;
            }
            float a = Mathf.Max(body, knot);
            float hl = Mathf.Exp(-Vector2.SqrMagnitude(new Vector2(x, y) - shine) / (size * 18f)) * 0.55f;
            px[y * size + x] = new Color(1f, 1f, 1f, a) * new Color(1f, 1f, 1f, 1f);
            px[y * size + x].r = Mathf.Min(1f, 0.92f + hl);
            px[y * size + x].g = Mathf.Min(1f, 0.92f + hl);
            px[y * size + x].b = Mathf.Min(1f, 0.95f + hl);
            px[y * size + x].a = a;
        }
        t.SetPixels(px);
        return t;
    }

    public static void Slice(Image image)
    {
        image.sprite = Rounded;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 1.15f;
    }
}
