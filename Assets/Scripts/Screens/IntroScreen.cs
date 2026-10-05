using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntroScreen : MonoBehaviour, IPointerClickHandler
{
    [Header("Core")]
    [SerializeField] CanvasGroup group;
    [SerializeField] ScreenMotion motion;
    [SerializeField] Text happyText;
    [SerializeField] Text birthdayText;
    [SerializeField] Text honoreeText;
    [SerializeField] RectTransform[] balloons;
    [SerializeField] ConfettiBurst confetti;

    AppFlow _app;
    float _t;
    bool _running;
    bool _canSkip;
    float _duration = 2.6f;
    float _skipAfter = 0.6f;

    public CanvasGroup Group => group;
    public ScreenMotion Motion => motion;

    public static IntroScreen Build(Transform parent, AppFlow app)
    {
        var cg = Ui.Fullscreen(parent, "Intro");
        var s = cg.gameObject.AddComponent<IntroScreen>();
        s.group = cg;
        s.motion = cg.gameObject.AddComponent<ScreenMotion>();
        s.motion.Bind(cg);
        s.BuildUi(cg.transform);
        var catcher = cg.gameObject.GetComponent<Image>();
        if (catcher == null)
        {
            catcher = cg.gameObject.AddComponent<Image>();
            catcher.color = Color.clear;
            catcher.raycastTarget = true;
        }
        s.Bind(app);
        return s;
    }

    public void Bind(AppFlow app)
    {
        _app = app;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (motion == null) motion = GetComponent<ScreenMotion>();
        if (motion != null && group != null) motion.Bind(group);
        if (app != null && app.Settings != null)
        {
            _duration = app.Settings.introDuration;
            _skipAfter = app.Settings.introSkipAfter;
        }
    }

    void BuildUi(Transform root)
    {
        BokehBackground.Attach(root);
        happyText = Ui.Txt(root, "Happy", "Happy", 86, Palette.DeepRose, TextAnchor.MiddleCenter, FontStyle.Bold);
        birthdayText = Ui.Txt(root, "Birthday", "Birthday", 78, Palette.Rose, TextAnchor.MiddleCenter, FontStyle.Bold);
        honoreeText = Ui.Txt(root, "Sep", "", 64, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
        CenterY(happyText.rectTransform, 220, 120);
        CenterY(birthdayText.rectTransform, 40, 110);
        CenterY(honoreeText.rectTransform, -160, 100);
        balloons = new RectTransform[3];
        for (int i = 0; i < 3; i++)
        {
            var img = Ui.Img(root, "ib" + i, RuntimeSprites.Balloon, Palette.Balloons[i]);
            img.rectTransform.sizeDelta = new Vector2(160, 190);
            balloons[i] = img.rectTransform;
        }
        confetti = ConfettiBurst.Create(root);
    }

    static void CenterY(RectTransform rt, float y, float h)
    {
        rt.anchorMin = new Vector2(0.08f, 0.5f);
        rt.anchorMax = new Vector2(0.92f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(0, h);
    }

    public void Play()
    {
        if (honoreeText != null && _app != null)
            honoreeText.text = _app.Config.Honoree;
        _t = 0f;
        _running = true;
        _canSkip = false;
        if (happyText != null) happyText.color = Fade(happyText.color, 0);
        if (birthdayText != null) birthdayText.color = Fade(birthdayText.color, 0);
        if (honoreeText != null) honoreeText.color = Fade(honoreeText.color, 0);
        if (balloons != null)
        {
            for (int i = 0; i < balloons.Length; i++)
                if (balloons[i] != null)
                    balloons[i].anchoredPosition = new Vector2((i - 1) * 220f, -900f);
        }
        JuiceAudio.I?.PlayChime();
        confetti?.Play(Vector2.zero, 42);
        Haptics.Success();
    }

    public void OnHidden() => _running = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_canSkip) _app.GoRoom();
    }

    void Update()
    {
        if (!_running) return;
        _t += Time.unscaledDeltaTime;
        if (_t >= _skipAfter) _canSkip = true;
        Stamp(happyText, 0.12f, 1f);
        Stamp(birthdayText, 0.38f, 1.08f);
        Stamp(honoreeText, 0.62f, 1.16f);
        if (balloons != null)
        {
            for (int i = 0; i < balloons.Length; i++)
            {
                if (balloons[i] == null) continue;
                float u = Mathf.Clamp01((_t - (0.2f + i * 0.12f)) / 0.7f);
                u = 1f - Mathf.Pow(1f - u, 3f);
                float x = (i - 1) * 240f;
                float y = Mathf.Lerp(-820f, 420f + Mathf.Sin(Time.unscaledTime * 1.4f + i) * 18f, u);
                balloons[i].anchoredPosition = new Vector2(x, y);
                balloons[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 1.6f + i) * 8f);
            }
        }
        if (_t >= _duration) _app.GoRoom();
    }

    void Stamp(Text t, float start, float peak)
    {
        if (t == null) return;
        float u = Mathf.Clamp01((_t - start) / 0.35f);
        t.color = Fade(t.color, u);
        float s = Mathf.Lerp(0.72f, peak, 1f - Mathf.Pow(1f - u, 3f));
        if (u > 0.75f) s = Mathf.Lerp(peak, 1f, (u - 0.75f) / 0.25f);
        t.rectTransform.localScale = Vector3.one * s;
    }

    static Color Fade(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
