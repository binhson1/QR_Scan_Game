using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonRoomScreen : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] CanvasGroup group;
    [SerializeField] ScreenMotion motion;
    [SerializeField] RectTransform balloonLayer;
    [SerializeField] ConfettiBurst confetti;
    [SerializeField] Text titleText;
    [SerializeField] Text subtitleText;
    [SerializeField] JuicyButton createButton;
    [SerializeField] JuicyButton muteButton;
    [SerializeField] JuicyButton backButton;
    [SerializeField] BalloonView balloonPrefabOverride;

    AppFlow _app;
    readonly List<BalloonView> _views = new List<BalloonView>();

    static readonly Vector2[] SlotsN =
    {
        new Vector2(0.20f, 0.74f), new Vector2(0.50f, 0.80f), new Vector2(0.80f, 0.72f),
        new Vector2(0.14f, 0.50f), new Vector2(0.38f, 0.56f), new Vector2(0.62f, 0.48f), new Vector2(0.86f, 0.52f),
        new Vector2(0.30f, 0.22f), new Vector2(0.70f, 0.20f)
    };

    public CanvasGroup Group => group;
    public ScreenMotion Motion => motion;

    public static BalloonRoomScreen Build(Transform parent, AppFlow app)
    {
        var cg = Ui.Fullscreen(parent, "Room");
        var s = cg.gameObject.AddComponent<BalloonRoomScreen>();
        s.group = cg;
        s.motion = cg.gameObject.AddComponent<ScreenMotion>();
        s.motion.Bind(cg);
        s.BuildUi(cg.transform);
        s.Bind(app);
        return s;
    }

    public void Bind(AppFlow app)
    {
        _app = app;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (motion == null) motion = GetComponent<ScreenMotion>();
        if (motion != null && group != null) motion.Bind(group);
        if (app?.Config != null)
        {
            if (titleText != null) titleText.text = app.Config.roomTitle;
            if (subtitleText != null) subtitleText.text = app.Config.roomSubtitle;
        }
        if (createButton != null) createButton.Clicked = () => _app.OpenCreate();
        if (muteButton != null) muteButton.Clicked = () => JuiceAudio.I?.ToggleMute();
        if (backButton != null) backButton.Clicked = () => _app.GoScan();
    }

    void BuildUi(Transform root)
    {
        BokehBackground.Attach(root);
        var safe = Ui.Rt(root, "Safe");
        Ui.Stretch(safe, 20, 20, 16, 16);

        var header = Ui.Img(safe, "Header", RuntimeSprites.White, new Color(Palette.Blush.r, Palette.Blush.g, Palette.Blush.b, 0.92f));
        header.rectTransform.anchorMin = new Vector2(0, 1);
        header.rectTransform.anchorMax = new Vector2(1, 1);
        header.rectTransform.pivot = new Vector2(0.5f, 1);
        header.rectTransform.anchoredPosition = Vector2.zero;
        header.rectTransform.sizeDelta = new Vector2(0, 118);

        titleText = Ui.Txt(safe, "Title", "Không gian bong bóng", 40, Palette.DeepRose, TextAnchor.UpperCenter, FontStyle.Bold);
        titleText.rectTransform.anchorMin = new Vector2(0, 1);
        titleText.rectTransform.anchorMax = new Vector2(1, 1);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1);
        titleText.rectTransform.anchoredPosition = new Vector2(0, -8);
        titleText.rectTransform.sizeDelta = new Vector2(0, 56);

        subtitleText = Ui.Txt(safe, "Sub", "Chạm vào bóng để xem ảnh + tên + lời chúc", 22, Palette.Ink, TextAnchor.UpperCenter);
        subtitleText.rectTransform.anchorMin = new Vector2(0, 1);
        subtitleText.rectTransform.anchorMax = new Vector2(1, 1);
        subtitleText.rectTransform.pivot = new Vector2(0.5f, 1);
        subtitleText.rectTransform.anchoredPosition = new Vector2(0, -64);
        subtitleText.rectTransform.sizeDelta = new Vector2(0, 40);

        balloonLayer = Ui.Rt(safe, "Balloons");
        Ui.Stretch(balloonLayer, 0, 0, 128, 150);

        confetti = ConfettiBurst.Create(safe);

        createButton = Ui.Button(safe, "+  Tạo khoảnh khắc", Palette.Rose, Color.white, null, new Vector2(0, 96));
        var fabRt = (RectTransform)createButton.transform;
        fabRt.anchorMin = new Vector2(0.08f, 0.02f);
        fabRt.anchorMax = new Vector2(0.92f, 0.09f);
        fabRt.offsetMin = fabRt.offsetMax = Vector2.zero;
        fabRt.sizeDelta = Vector2.zero;

        muteButton = Ui.Button(safe, "♪", Palette.SoftWhite, Palette.Rose, null, new Vector2(72, 72));
        var muteRt = (RectTransform)muteButton.transform;
        muteRt.anchorMin = muteRt.anchorMax = new Vector2(1, 1);
        muteRt.pivot = new Vector2(1, 1);
        muteRt.anchoredPosition = new Vector2(0, -8);

        backButton = Ui.Button(safe, "← QR", Palette.SoftWhite, Palette.DeepRose, null, new Vector2(160, 72));
        var backRt = (RectTransform)backButton.transform;
        backRt.anchorMin = backRt.anchorMax = new Vector2(0, 1);
        backRt.pivot = new Vector2(0, 1);
        backRt.anchoredPosition = new Vector2(0, -8);
    }

    public void Reload(bool animateNewId = false, string newId = null)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ReloadCo(animateNewId, newId));
    }

    IEnumerator ReloadCo(bool animateNewId, string newId)
    {
        yield return null;
        for (int i = 0; i < _views.Count; i++)
            if (_views[i] != null) Destroy(_views[i].gameObject);
        _views.Clear();
        var rect = balloonLayer.rect;
        float w = Mathf.Max(400f, rect.width);
        float h = Mathf.Max(500f, rect.height);
        var all = _app.Repo.All;
        Vector2 newest = Vector2.zero;
        var prefab = balloonPrefabOverride != null ? balloonPrefabOverride : _app.BalloonPrefab;
        Vector2 size = _app.Settings != null ? _app.Settings.balloonPrefabSize : new Vector2(210, 260);
        for (int i = 0; i < all.Count; i++)
        {
            var rec = all[i];
            BalloonView view;
            if (prefab != null)
            {
                view = Instantiate(prefab, balloonLayer);
                view.gameObject.SetActive(true);
                view.Setup(rec, v => _app.OpenWish(v.Record));
            }
            else
            {
                view = BalloonView.Create(balloonLayer, rec, v => _app.OpenWish(v.Record));
            }
            view.ApplySize(size);
            var home = Slot(i, w, h);
            bool isNew = animateNewId && rec.id == newId;
            if (isNew) view.EnterFromBottom(home);
            else view.Place(home, true);
            var tex = _app.Repo.LoadImage(rec);
            if (tex != null) view.BindPhoto(tex);
            _views.Add(view);
            newest = home;
        }
        if (animateNewId)
        {
            Punch(newest);
            confetti?.Play(newest, 48);
            JuiceAudio.I?.PlaySuccess();
            Haptics.Success();
        }
    }

    void Punch(Vector2 from)
    {
        for (int i = 0; i < _views.Count - 1; i++)
        {
            Vector2 d = _views[i].Home - from;
            if (d.sqrMagnitude < 1f) d = Vector2.up;
            _views[i].Nudge(d.normalized * 280f);
        }
    }

    public Vector2 Slot(int i, float w, float h)
    {
        Vector2 n;
        if (i < SlotsN.Length) n = SlotsN[i];
        else
        {
            float a = i * 2.399963f;
            float r = 0.12f * Mathf.Sqrt(i);
            n = new Vector2(0.5f + Mathf.Cos(a) * r, 0.48f + Mathf.Sin(a) * r * 0.7f);
        }
        return new Vector2((n.x - 0.5f) * w * 0.92f, (n.y - 0.5f) * h * 0.86f);
    }
}
