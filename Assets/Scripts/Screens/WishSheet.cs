using UnityEngine;
using UnityEngine.UI;

public class WishSheet : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] CanvasGroup group;
    [SerializeField] Image photo;
    [SerializeField] Text initials;
    [SerializeField] Text nameText;
    [SerializeField] Text messageText;
    [SerializeField] RectTransform sheet;
    [SerializeField] Image dimImg;
    [SerializeField] JuicyButton closeButton;
    [SerializeField] JuicyButton dimButton;

    AppFlow _app;
    Spring2 _sheetPos;
    SpringState _dim = SpringState.Make(0, 16f, 0.9f);
    bool _open;
    bool _posReady;

    public CanvasGroup Group => group;
    public bool IsOpen => _open;

    public static WishSheet Build(Transform parent, AppFlow app)
    {
        var cg = Ui.Fullscreen(parent, "Wish");
        var s = cg.gameObject.AddComponent<WishSheet>();
        s.group = cg;
        s.BuildUi(cg.transform);
        s.Bind(app);
        return s;
    }

    public void Bind(AppFlow app)
    {
        _app = app;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (closeButton != null) closeButton.Clicked = Close;
        if (dimButton != null) dimButton.Clicked = Close;
        EnsureSpring();
    }

    void EnsureSpring()
    {
        if (_posReady || sheet == null) return;
        _sheetPos = Spring2.Make(sheet.anchoredPosition, 14f, 0.72f);
        _posReady = true;
    }

    void BuildUi(Transform root)
    {
        dimImg = Ui.Img(root, "Dim", RuntimeSprites.White, Palette.Overlay);
        dimImg.raycastTarget = true;
        Ui.Stretch(dimImg.rectTransform);
        dimButton = dimImg.gameObject.AddComponent<JuicyButton>();

        var card = Ui.Img(root, "Sheet", RuntimeSprites.Rounded, Palette.SoftWhite);
        RuntimeSprites.Slice(card);
        sheet = card.rectTransform;
        sheet.anchorMin = new Vector2(0.04f, 0f);
        sheet.anchorMax = new Vector2(0.96f, 0f);
        sheet.pivot = new Vector2(0.5f, 0f);
        sheet.sizeDelta = new Vector2(0, 1120);
        sheet.anchoredPosition = new Vector2(0, -1200);

        var handle = Ui.Img(sheet, "Handle", RuntimeSprites.Rounded, Palette.DustyPink);
        handle.rectTransform.anchorMin = handle.rectTransform.anchorMax = new Vector2(0.5f, 1);
        handle.rectTransform.pivot = new Vector2(0.5f, 1);
        handle.rectTransform.sizeDelta = new Vector2(90, 10);
        handle.rectTransform.anchoredPosition = new Vector2(0, -18);

        var mask = Ui.Rt(sheet, "PhotoMask");
        mask.anchorMin = mask.anchorMax = new Vector2(0.5f, 1);
        mask.pivot = new Vector2(0.5f, 1);
        mask.sizeDelta = new Vector2(280, 280);
        mask.anchoredPosition = new Vector2(0, -70);
        var mimg = mask.gameObject.AddComponent<Image>();
        mimg.sprite = RuntimeSprites.Circle;
        mimg.color = Palette.Pearl;
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        photo = Ui.Img(mask, "Photo", RuntimeSprites.Circle, Color.white);
        Ui.Stretch(photo.rectTransform);
        photo.preserveAspect = true;

        initials = Ui.Txt(mask, "Ini", "", 64, Palette.DeepRose, TextAnchor.MiddleCenter, FontStyle.Bold);
        Ui.Stretch(initials.rectTransform);

        nameText = Ui.Txt(sheet, "Name", "", 40, Palette.DeepRose, TextAnchor.MiddleCenter, FontStyle.Bold);
        nameText.rectTransform.anchorMin = new Vector2(0.08f, 1);
        nameText.rectTransform.anchorMax = new Vector2(0.92f, 1);
        nameText.rectTransform.pivot = new Vector2(0.5f, 1);
        nameText.rectTransform.anchoredPosition = new Vector2(0, -380);
        nameText.rectTransform.sizeDelta = new Vector2(0, 56);

        messageText = Ui.Txt(sheet, "Msg", "", 30, Palette.Ink, TextAnchor.UpperCenter);
        messageText.rectTransform.anchorMin = new Vector2(0.1f, 0.12f);
        messageText.rectTransform.anchorMax = new Vector2(0.9f, 1);
        messageText.rectTransform.offsetMin = new Vector2(0, 0);
        messageText.rectTransform.offsetMax = new Vector2(0, -450);

        closeButton = Ui.Button(sheet, "Đóng", Palette.Rose, Color.white, null, new Vector2(420, 84));
        var crt = (RectTransform)closeButton.transform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0);
        crt.pivot = new Vector2(0.5f, 0);
        crt.anchoredPosition = new Vector2(0, 36);
    }

    public void Open(BalloonRecord rec)
    {
        EnsureSpring();
        _open = true;
        group.gameObject.SetActive(true);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        nameText.text = rec.displayName;
        messageText.text = rec.message;
        initials.text = ImageUtil.Initials(rec.displayName);
        var tex = _app.Repo.LoadImage(rec);
        if (tex != null)
        {
            ImageUtil.ShowContained(photo, tex);
            initials.gameObject.SetActive(false);
        }
        else
        {
            photo.sprite = RuntimeSprites.Circle;
            photo.color = Palette.Balloons[rec.colorIndex % Palette.Balloons.Length];
            initials.gameObject.SetActive(true);
        }
        _sheetPos.Value = new Vector2(0, -1100);
        _sheetPos.Target = new Vector2(0, 24);
        _dim.Value = 0;
        _dim.Target = 0.48f;
        JuiceAudio.I?.PlayWhoosh();
    }

    public void Close()
    {
        if (!_open) return;
        EnsureSpring();
        _open = false;
        _sheetPos.Target = new Vector2(0, -1100);
        _dim.Target = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        JuiceAudio.I?.PlayTick();
    }

    void Update()
    {
        if (sheet == null || dimImg == null) return;
        EnsureSpring();
        float dt = Time.unscaledDeltaTime;
        _sheetPos.Step(dt);
        _dim.Step(dt);
        sheet.anchoredPosition = _sheetPos.Value;
        var c = Palette.Overlay;
        c.a = Mathf.Clamp01(_dim.Value);
        dimImg.color = c;
        if (!_open && _dim.Value < 0.02f)
            group.gameObject.SetActive(false);
    }
}
