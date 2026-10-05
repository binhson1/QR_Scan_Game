using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class ImageUtil
{
    public static Texture2D Load(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        var data = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!tex.LoadImage(data))
        {
            UnityEngine.Object.Destroy(tex);
            return null;
        }
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    public static Texture2D ScaleMax(Texture2D src, int max)
    {
        if (src == null) return null;
        int w = src.width;
        int h = src.height;
        float m = Mathf.Max(w, h);
        if (m <= max) return src;
        float s = max / m;
        int nw = Mathf.Max(2, Mathf.RoundToInt(w * s));
        int nh = Mathf.Max(2, Mathf.RoundToInt(h * s));
        var rt = RenderTexture.GetTemporary(nw, nh, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var dst = new Texture2D(nw, nh, TextureFormat.RGBA32, false);
        dst.ReadPixels(new Rect(0, 0, nw, nh), 0, 0);
        dst.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }

    public static Texture2D FromWebcam(WebCamTexture cam)
    {
        if (cam == null || !cam.isPlaying || cam.width < 16) return null;
        var pixels = cam.GetPixels32();
        var oriented = Orient(pixels, cam.width, cam.height, cam.videoRotationAngle, cam.videoVerticallyMirrored, out int ow, out int oh);
        var tex = new Texture2D(ow, oh, TextureFormat.RGBA32, false);
        tex.SetPixels32(oriented);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply();
        return tex;
    }

    public static Color32[] Orient(Color32[] src, int w, int h, int rotation, bool verticalMirror, out int ow, out int oh)
    {
        ow = w;
        oh = h;
        var buf = src;
        if (verticalMirror)
            buf = FlipVertical(buf, w, h);

        int rot = ((rotation % 360) + 360) % 360;
        if (rot == 0) return buf;
        if (rot == 180)
        {
            var d = new Color32[buf.Length];
            for (int i = 0; i < buf.Length; i++) d[i] = buf[buf.Length - 1 - i];
            return d;
        }

        ow = h;
        oh = w;
        var dst = new Color32[ow * oh];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int nx;
            int ny;
            if (rot == 90)
            {
                nx = h - 1 - y;
                ny = x;
            }
            else
            {
                nx = y;
                ny = w - 1 - x;
            }
            dst[ny * ow + nx] = buf[y * w + x];
        }
        return dst;
    }

    static Color32[] FlipVertical(Color32[] src, int w, int h)
    {
        var dst = new Color32[src.Length];
        for (int y = 0; y < h; y++)
            System.Array.Copy(src, y * w, dst, (h - 1 - y) * w, w);
        return dst;
    }

    public static void ShowContained(Image image, Texture2D tex)
    {
        if (image == null || tex == null) return;
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
        image.color = Color.white;
        image.preserveAspect = true;
        image.type = Image.Type.Simple;
        var fitter = image.GetComponent<AspectRatioFitter>();
        if (fitter == null) fitter = image.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = tex.height < 1 ? 1f : tex.width / (float)tex.height;
    }

    public static Texture2D Placeholder(string caption)
    {
        int s = 512;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            float u = x / (float)s;
            float v = y / (float)s;
            tex.SetPixel(x, y, Color.Lerp(Palette.DustyPink, Palette.Champagne, (u + v) * 0.5f));
        }
        tex.Apply();
        return tex;
    }

    public static string Initials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }
}
