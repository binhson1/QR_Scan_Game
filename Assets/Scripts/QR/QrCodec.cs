using System;
using UnityEngine;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Collections.Generic;

public static class QrCodec
{
    static int _busy;

    public static Texture2D Encode(string payload, int size = 512)
    {
        var writer = new QRCodeWriter();
        var hints = new Dictionary<EncodeHintType, object>
        {
            { EncodeHintType.MARGIN, 1 },
            { EncodeHintType.CHARACTER_SET, "UTF-8" }
        };
        BitMatrix matrix = writer.encode(payload, BarcodeFormat.QR_CODE, size, size, hints);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var black = Palette.Ink;
        var white = Palette.SoftWhite;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            tex.SetPixel(x, size - 1 - y, matrix[x, y] ? black : white);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return tex;
    }

    public static void DecodeAsync(Color32[] pixels, int width, int height, int rotation, Action<string> done, int maxEdge = 480)
    {
        if (pixels == null || width < 8 || height < 8) return;
        if (System.Threading.Interlocked.Exchange(ref _busy, 1) == 1) return;
        int edge = Mathf.Clamp(maxEdge, 160, 1920);

        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            string text = null;
            try
            {
                var small = Downscale(pixels, width, height, edge, out int w, out int h);
                small = Rotate(small, w, h, rotation, out w, out h);
                var bytes = new byte[small.Length * 4];
                for (int i = 0; i < small.Length; i++)
                {
                    int o = i * 4;
                    bytes[o] = small[i].r;
                    bytes[o + 1] = small[i].g;
                    bytes[o + 2] = small[i].b;
                    bytes[o + 3] = small[i].a;
                }
                var source = new RGBLuminanceSource(bytes, w, h, RGBLuminanceSource.BitmapFormat.RGBA32);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                var result = new QRCodeReader().decode(bitmap);
                if (result != null) text = result.Text;
            }
            catch
            {
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref _busy, 0);
            }

            MainThread.Run(() => done?.Invoke(text));
        });
    }

    static Color32[] Downscale(Color32[] src, int w, int h, int max, out int nw, out int nh)
    {
        float scale = max / (float)Mathf.Max(w, h);
        if (scale >= 1f)
        {
            nw = w;
            nh = h;
            return src;
        }
        nw = Mathf.Max(1, Mathf.RoundToInt(w * scale));
        nh = Mathf.Max(1, Mathf.RoundToInt(h * scale));
        var dst = new Color32[nw * nh];
        for (int y = 0; y < nh; y++)
        for (int x = 0; x < nw; x++)
            dst[y * nw + x] = src[(y * h / nh) * w + (x * w / nw)];
        return dst;
    }

    static Color32[] Rotate(Color32[] src, int w, int h, int angle, out int nw, out int nh)
    {
        angle = ((angle % 360) + 360) % 360;
        nw = w;
        nh = h;
        if (angle == 0) return src;
        if (angle == 180)
        {
            var d = new Color32[src.Length];
            for (int i = 0; i < src.Length; i++) d[i] = src[src.Length - 1 - i];
            return d;
        }
        nw = h;
        nh = w;
        var dst = new Color32[nw * nh];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int nx = angle == 90 ? h - 1 - y : y;
            int ny = angle == 90 ? x : w - 1 - x;
            dst[ny * nw + nx] = src[y * w + x];
        }
        return dst;
    }
}
