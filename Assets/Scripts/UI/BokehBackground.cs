using UnityEngine;
using UnityEngine.UI;

public class BokehBackground : MonoBehaviour
{
    struct Blob
    {
        public RectTransform Rt;
        public Vector2 Origin;
        public float Amp;
        public float Speed;
        public float Phase;
    }

    Blob[] _blobs;

    public static BokehBackground Attach(Transform parent)
    {
        var bg = Ui.Img(parent, "Gradient", RuntimeSprites.Gradient, Color.white);
        Ui.Stretch(bg.rectTransform);
        bg.raycastTarget = false;
        var host = Ui.Rt(parent, "Bokeh");
        Ui.Stretch(host);
        var comp = host.gameObject.AddComponent<BokehBackground>();
        comp.Build(host);
        return comp;
    }

    void Awake() => EnsureBlobs();

    void EnsureBlobs()
    {
        if (_blobs != null && _blobs.Length > 0 && _blobs[0].Rt != null) return;
        int n = transform.childCount;
        if (n == 0)
        {
            Build((RectTransform)transform);
            return;
        }
        _blobs = new Blob[n];
        for (int i = 0; i < n; i++)
        {
            var rt = (RectTransform)transform.GetChild(i);
            _blobs[i] = new Blob
            {
                Rt = rt,
                Origin = rt.anchoredPosition,
                Amp = 18f + (i % 5) * 6f,
                Speed = 0.18f + (i % 4) * 0.07f,
                Phase = i * 1.7f
            };
        }
    }

    void Build(RectTransform host)
    {
        _blobs = new Blob[12];
        var colors = new[] { Palette.DustyPink, Palette.Champagne, Palette.Pearl, Palette.Rose, Palette.Gold };
        for (int i = 0; i < _blobs.Length; i++)
        {
            var img = Ui.Img(host, "b" + i, RuntimeSprites.Bokeh, colors[i % colors.Length]);
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.22f);
            float size = Random.Range(90f, 220f);
            Ui.Center(img.rectTransform, new Vector2(size, size));
            var origin = new Vector2(Random.Range(-420f, 420f), Random.Range(-700f, 700f));
            img.rectTransform.anchoredPosition = origin;
            _blobs[i] = new Blob
            {
                Rt = img.rectTransform,
                Origin = origin,
                Amp = Random.Range(18f, 48f),
                Speed = Random.Range(0.18f, 0.45f),
                Phase = Random.Range(0f, 30f)
            };
        }
    }

    void Update()
    {
        if (_blobs == null) return;
        float t = Time.unscaledTime;
        for (int i = 0; i < _blobs.Length; i++)
        {
            var b = _blobs[i];
            b.Rt.anchoredPosition = b.Origin + new Vector2(
                Mathf.Sin(t * b.Speed + b.Phase) * b.Amp,
                Mathf.Cos(t * b.Speed * 0.8f + b.Phase) * b.Amp * 0.7f);
        }
    }
}
