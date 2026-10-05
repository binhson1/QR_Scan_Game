using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalloonView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs (kéo từ prefab)")]
    [SerializeField] RectTransform stringRt;
    [SerializeField] Image body;
    [SerializeField] Image avatar;
    [SerializeField] Text initials;

    public BalloonRecord Record;
    public Vector2 Home;
    public Action<BalloonView> Clicked;

    Spring2 _pos;
    SpringState _scale = SpringState.Make(1f, 18f, 0.5f);
    RectTransform _rt;
    float _phase;
    bool _entering;

    public static BalloonView Create(Transform parent, BalloonRecord rec, Action<BalloonView> clicked)
    {
        var root = Ui.Rt(parent, rec != null ? rec.displayName : "Balloon");
        root.sizeDelta = new Vector2(210, 260);
        var view = root.gameObject.AddComponent<BalloonView>();
        view._rt = root;
        view.BuildVisuals();
        view.Setup(rec, clicked);
        return view;
    }

    void Awake() => EnsureRefs();

    void EnsureRefs()
    {
        _rt = (RectTransform)transform;
        if (stringRt == null)
        {
            var t = transform.Find("String");
            if (t) stringRt = t as RectTransform;
        }
        if (body == null)
        {
            var t = transform.Find("Body");
            if (t) body = t.GetComponent<Image>();
        }
        if (avatar == null)
        {
            var t = transform.Find("Body/Mask/Avatar");
            if (t) avatar = t.GetComponent<Image>();
        }
        if (initials == null)
        {
            var t = transform.Find("Body/Ini");
            if (t) initials = t.GetComponent<Text>();
        }
    }

    void BuildVisuals()
    {
        var str = Ui.Img(_rt, "String", RuntimeSprites.White, Palette.DustyPink);
        stringRt = str.rectTransform;
        stringRt.pivot = new Vector2(0.5f, 1f);
        stringRt.anchorMin = stringRt.anchorMax = new Vector2(0.5f, 0.18f);
        stringRt.sizeDelta = new Vector2(4, 78);
        stringRt.anchoredPosition = Vector2.zero;

        body = Ui.Img(_rt, "Body", RuntimeSprites.Balloon, Palette.Balloons[0]);
        body.raycastTarget = true;
        Ui.Stretch(body.rectTransform, 8, 8, 8, 50);

        var maskRt = Ui.Rt(body.transform, "Mask");
        maskRt.anchorMin = new Vector2(0.22f, 0.32f);
        maskRt.anchorMax = new Vector2(0.78f, 0.78f);
        maskRt.offsetMin = maskRt.offsetMax = Vector2.zero;
        var maskImg = maskRt.gameObject.AddComponent<Image>();
        maskImg.sprite = RuntimeSprites.Circle;
        maskImg.color = new Color(1, 1, 1, 0.15f);
        maskRt.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        avatar = Ui.Img(maskRt, "Avatar", RuntimeSprites.Circle, Color.white);
        var art = avatar.rectTransform;
        art.anchorMin = art.anchorMax = art.pivot = new Vector2(0.5f, 0.5f);
        art.anchoredPosition = Vector2.zero;
        art.sizeDelta = new Vector2(120, 120);
        avatar.preserveAspect = true;
        avatar.gameObject.SetActive(false);

        initials = Ui.Txt(body.transform, "Ini", "", 42, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
        initials.rectTransform.anchorMin = new Vector2(0.2f, 0.34f);
        initials.rectTransform.anchorMax = new Vector2(0.8f, 0.76f);
        initials.rectTransform.offsetMin = initials.rectTransform.offsetMax = Vector2.zero;
    }

    public void Setup(BalloonRecord rec, Action<BalloonView> clicked)
    {
        EnsureRefs();
        Record = rec;
        Clicked = clicked;
        _phase = UnityEngine.Random.Range(0f, 30f);
        if (rec == null) return;
        name = rec.displayName;
        if (body != null)
            body.color = Palette.Balloons[rec.colorIndex % Palette.Balloons.Length];
        if (initials != null)
        {
            initials.text = ImageUtil.Initials(rec.displayName);
            initials.gameObject.SetActive(true);
        }
        if (avatar != null) avatar.gameObject.SetActive(false);
        if (_appSize.x > 1f) _rt.sizeDelta = _appSize;
    }

    Vector2 _appSize;

    public void ApplySize(Vector2 size)
    {
        _appSize = size;
        if (_rt == null) _rt = (RectTransform)transform;
        if (size.x > 1f) _rt.sizeDelta = size;
    }

    public void BindPhoto(Texture2D tex)
    {
        if (tex == null || avatar == null) return;
        ImageUtil.ShowContained(avatar, tex);
        avatar.gameObject.SetActive(true);
        if (initials != null) initials.gameObject.SetActive(false);
    }

    public void Place(Vector2 home, bool instant)
    {
        Home = home;
        _pos.Target = home;
        _pos.Omega = 11f;
        _pos.Zeta = 0.68f;
        if (instant)
        {
            _pos.Value = home;
            _rt.anchoredPosition = home;
        }
    }

    public void EnterFromBottom(Vector2 home)
    {
        Place(home, false);
        _entering = true;
        _pos.Value = new Vector2(0f, -900f);
        _pos.Velocity = new Vector2(0, 900f);
        _scale.Value = 0.2f;
        _scale.Velocity = 4f;
        _scale.Target = 1f;
        _rt.anchoredPosition = _pos.Value;
    }

    public void Nudge(Vector2 impulse) => _pos.Velocity += impulse;

    public void OnPointerDown(PointerEventData eventData)
    {
        _scale.Target = 0.9f;
        JuiceAudio.I?.PlayTick();
        Haptics.Tick();
    }

    public void OnPointerUp(PointerEventData eventData) => _scale.Target = 1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        _scale.Value = 1.1f;
        _scale.Velocity = 3f;
        _scale.Target = 1f;
        JuiceAudio.I?.PlayPop();
        Clicked?.Invoke(this);
    }

    void Update()
    {
        if (_rt == null) _rt = (RectTransform)transform;
        float dt = Time.unscaledDeltaTime;
        _pos.Target = Home;
        _pos.Step(dt);
        _scale.Step(dt);
        float breathe = 1f + 0.04f * Mathf.Sin(Time.unscaledTime * 1.35f + _phase);
        float sway = 6.5f * Mathf.Sin(Time.unscaledTime * 0.85f + _phase);
        _rt.anchoredPosition = _pos.Value + new Vector2(
            Mathf.Sin(Time.unscaledTime * 0.7f + _phase) * 6f,
            Mathf.Cos(Time.unscaledTime * 0.9f + _phase) * 5f);
        _rt.localScale = Vector3.one * _scale.Value * breathe;
        _rt.localRotation = Quaternion.Euler(0, 0, sway);
        if (stringRt != null)
            stringRt.localRotation = Quaternion.Euler(0, 0, -sway * 0.7f);
        if (_entering && Vector2.Distance(_pos.Value, Home) < 18f)
        {
            _entering = false;
            _scale.Value = 1.18f;
            _scale.Target = 1f;
        }
    }
}
