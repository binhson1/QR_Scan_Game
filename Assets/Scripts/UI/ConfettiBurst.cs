using UnityEngine;
using UnityEngine.UI;

public class ConfettiBurst : MonoBehaviour
{
    struct Piece
    {
        public RectTransform Rt;
        public Image Img;
        public Vector2 Vel;
        public float Rot;
        public float Spin;
        public float Life;
        public float Max;
    }

    Piece[] _pool;
    int _alive;

    public static ConfettiBurst Create(Transform parent)
    {
        var rt = Ui.Rt(parent, "Confetti");
        Ui.Stretch(rt);
        rt.SetAsLastSibling();
        return rt.gameObject.AddComponent<ConfettiBurst>();
    }

    void Awake() => EnsurePool();

    void EnsurePool()
    {
        if (_pool != null && _pool.Length > 0 && _pool[0].Rt != null) return;
        var host = (RectTransform)transform;
        if (host.childCount == 0)
        {
            Build(host);
            return;
        }
        _pool = new Piece[host.childCount];
        for (int i = 0; i < host.childCount; i++)
        {
            var child = host.GetChild(i);
            _pool[i] = new Piece
            {
                Rt = child as RectTransform,
                Img = child.GetComponent<Image>()
            };
        }
    }

    void Build(RectTransform host)
    {
        _pool = new Piece[56];
        for (int i = 0; i < _pool.Length; i++)
        {
            var img = Ui.Img(host, "p" + i, i % 2 == 0 ? RuntimeSprites.Circle : RuntimeSprites.White, Color.white);
            img.rectTransform.sizeDelta = new Vector2(14, 18);
            img.gameObject.SetActive(false);
            _pool[i] = new Piece { Rt = img.rectTransform, Img = img };
        }
    }

    public void Play(Vector2 localOrigin, int count = 36)
    {
        EnsurePool();
        if (_pool == null || _pool.Length == 0) return;
        var colors = Palette.Balloons;
        count = Mathf.Min(count, _pool.Length);
        for (int i = 0; i < count; i++)
        {
            var p = _pool[i];
            p.Rt.gameObject.SetActive(true);
            p.Rt.anchoredPosition = localOrigin;
            p.Rt.localScale = Vector3.one * Random.Range(0.5f, 1.15f);
            p.Vel = Random.insideUnitCircle * 620f;
            p.Vel.y = Mathf.Abs(p.Vel.y) + 240f;
            p.Rot = Random.Range(0f, 360f);
            p.Spin = Random.Range(-420f, 420f);
            p.Max = Random.Range(0.9f, 1.45f);
            p.Life = p.Max;
            p.Img.color = colors[i % colors.Length];
            _pool[i] = p;
        }
        _alive = count;
    }

    void Update()
    {
        if (_alive <= 0 || _pool == null) return;
        float dt = Time.unscaledDeltaTime;
        int live = 0;
        for (int i = 0; i < _pool.Length; i++)
        {
            var p = _pool[i];
            if (p.Life <= 0f) continue;
            p.Life -= dt;
            p.Vel.y -= 980f * dt;
            p.Rt.anchoredPosition += p.Vel * dt;
            p.Rot += p.Spin * dt;
            p.Rt.localRotation = Quaternion.Euler(0, 0, p.Rot);
            var c = p.Img.color;
            c.a = Mathf.Clamp01(p.Life / 0.35f);
            p.Img.color = c;
            if (p.Life <= 0f) p.Rt.gameObject.SetActive(false);
            else live++;
            _pool[i] = p;
        }
        _alive = live;
    }
}
