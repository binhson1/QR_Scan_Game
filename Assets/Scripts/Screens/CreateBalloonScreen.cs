using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CreateBalloonScreen : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] CanvasGroup group;
    [SerializeField] ScreenMotion motion;
    [SerializeField] Image preview;
    [SerializeField] Text placeholder;
    [SerializeField] Text titleText;
    [SerializeField] InputField nameInput;
    [SerializeField] InputField captionInput;
    [SerializeField] GameObject camOverlay;
    [SerializeField] WebcamView webcam;

    [Header("Buttons")]
    [SerializeField] JuicyButton shootButton;
    [SerializeField] JuicyButton galleryButton;
    [SerializeField] JuicyButton releaseButton;
    [SerializeField] JuicyButton closeButton;
    [SerializeField] JuicyButton shutterButton;
    [SerializeField] JuicyButton flipButton;
    [SerializeField] JuicyButton cancelCamButton;

    AppFlow _app;
    Texture2D _photo;
    bool _preferBack;

    public CanvasGroup Group => group;
    public ScreenMotion Motion => motion;

    public static CreateBalloonScreen Build(Transform parent, AppFlow app)
    {
        var cg = Ui.Fullscreen(parent, "Create");
        var s = cg.gameObject.AddComponent<CreateBalloonScreen>();
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
        if (app?.Settings != null)
        {
            _preferBack = app.Settings.capturePreferBackCamera;
            webcam?.ApplySettings(app.Settings, _preferBack);
        }
        if (app?.Config != null && titleText != null)
            titleText.text = app.Config.createTitle;
        if (shootButton != null) shootButton.Clicked = OpenCamera;
        if (galleryButton != null) galleryButton.Clicked = PickGallery;
        if (releaseButton != null) releaseButton.Clicked = Release;
        if (closeButton != null) closeButton.Clicked = () => _app.CloseCreate();
        if (shutterButton != null) shutterButton.Clicked = Capture;
        if (flipButton != null) flipButton.Clicked = () => webcam.Flip();
        if (cancelCamButton != null) cancelCamButton.Clicked = CloseCamera;
    }

    void BuildUi(Transform root)
    {
        BokehBackground.Attach(root);
        var safe = Ui.Rt(root, "Safe");
        Ui.Stretch(safe, 28, 28, 24, 24);

        titleText = Ui.Txt(safe, "Title", "Tạo bong bóng mới", 40, Palette.DeepRose, TextAnchor.UpperCenter, FontStyle.Bold);
        titleText.rectTransform.anchorMin = new Vector2(0, 1);
        titleText.rectTransform.anchorMax = new Vector2(1, 1);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1);
        titleText.rectTransform.anchoredPosition = Vector2.zero;
        titleText.rectTransform.sizeDelta = new Vector2(0, 56);

        var previewBg = Ui.Img(safe, "PreviewBg", RuntimeSprites.Rounded, Palette.SoftWhite);
        RuntimeSprites.Slice(previewBg);
        previewBg.rectTransform.anchorMin = new Vector2(0.18f, 0.52f);
        previewBg.rectTransform.anchorMax = new Vector2(0.82f, 0.88f);
        previewBg.rectTransform.offsetMin = previewBg.rectTransform.offsetMax = Vector2.zero;
        var mask = Ui.Rt(previewBg.transform, "Mask");
        Ui.Stretch(mask, 12, 12, 12, 12);
        var mi = mask.gameObject.AddComponent<Image>();
        mi.sprite = RuntimeSprites.Rounded;
        mi.type = Image.Type.Sliced;
        mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        preview = Ui.Img(mask, "Photo", RuntimeSprites.White, Palette.Pearl);
        var prt = preview.rectTransform;
        prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = Vector2.zero;
        prt.sizeDelta = new Vector2(200, 200);
        preview.preserveAspect = true;
        var fitter = preview.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1f;
        placeholder = Ui.Txt(previewBg.transform, "Ph", "Ảnh của bạn sẽ bay cùng bóng", 24, Palette.Ink, TextAnchor.MiddleCenter);
        Ui.Stretch(placeholder.rectTransform, 20, 20, 20, 20);

        shootButton = Ui.Button(safe, "Chụp ảnh", Palette.Rose, Color.white, null, new Vector2(0, 80));
        var srt = (RectTransform)shootButton.transform;
        srt.anchorMin = new Vector2(0, 0.42f);
        srt.anchorMax = new Vector2(0.48f, 0.49f);
        srt.offsetMin = srt.offsetMax = Vector2.zero;
        srt.sizeDelta = Vector2.zero;

        galleryButton = Ui.Button(safe, "Thư viện", Palette.DeepRose, Color.white, null, new Vector2(0, 80));
        var grt = (RectTransform)galleryButton.transform;
        grt.anchorMin = new Vector2(0.52f, 0.42f);
        grt.anchorMax = new Vector2(1f, 0.49f);
        grt.offsetMin = grt.offsetMax = Vector2.zero;
        grt.sizeDelta = Vector2.zero;

        nameInput = Ui.Input(safe, "Tên của bạn", 24);
        var nrt = nameInput.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0, 0.33f);
        nrt.anchorMax = new Vector2(1, 0.40f);
        nrt.offsetMin = nrt.offsetMax = Vector2.zero;

        captionInput = Ui.Input(safe, "Lời chúc / khoảnh khắc", 120);
        var crt = captionInput.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 0.22f);
        crt.anchorMax = new Vector2(1, 0.32f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;
        captionInput.lineType = InputField.LineType.MultiLineNewline;

        releaseButton = Ui.Button(safe, "Thả bóng bay", Palette.Rose, Color.white, null, new Vector2(0, 92));
        var rrt = (RectTransform)releaseButton.transform;
        rrt.anchorMin = new Vector2(0, 0.08f);
        rrt.anchorMax = new Vector2(1, 0.16f);
        rrt.offsetMin = rrt.offsetMax = Vector2.zero;
        rrt.sizeDelta = Vector2.zero;

        closeButton = Ui.Button(safe, "Đóng", Palette.DustyPink, Palette.Ink, null, new Vector2(220, 72));
        var brt = (RectTransform)closeButton.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0, 1);
        brt.pivot = new Vector2(0, 1);
        brt.anchoredPosition = new Vector2(0, -4);

        BuildCamOverlay(root);
        SetHasPhoto(false);
    }

    void BuildCamOverlay(Transform root)
    {
        camOverlay = Ui.Rt(root, "Cam").gameObject;
        Ui.Stretch(camOverlay.GetComponent<RectTransform>());
        var bg = Ui.Img(camOverlay.transform, "Bg", RuntimeSprites.White, Color.black);
        Ui.Stretch(bg.rectTransform);
        bg.raycastTarget = true;
        webcam = WebcamView.Create(camOverlay.transform);
        shutterButton = Ui.Button(camOverlay.transform, "●", Palette.Rose, Color.white, null, new Vector2(120, 120));
        var srt = (RectTransform)shutterButton.transform;
        srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.08f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = Vector2.zero;
        flipButton = Ui.Button(camOverlay.transform, "⇄", Palette.SoftWhite, Palette.Ink, null, new Vector2(88, 88));
        var frt = (RectTransform)flipButton.transform;
        frt.anchorMin = frt.anchorMax = new Vector2(0.82f, 0.08f);
        cancelCamButton = Ui.Button(camOverlay.transform, "Huỷ", Palette.Champagne, Palette.Ink, null, new Vector2(160, 72));
        var crt = (RectTransform)cancelCamButton.transform;
        crt.anchorMin = crt.anchorMax = new Vector2(0.18f, 0.08f);
        camOverlay.SetActive(false);
    }

    public void OnShown()
    {
        if (nameInput != null) nameInput.text = "";
        if (captionInput != null) captionInput.text = "";
        ClearPhoto();
    }

    public void OnHidden() => CloseCamera();

    void OpenCamera() => StartCoroutine(OpenCameraCo());

    IEnumerator OpenCameraCo()
    {
        if (!CameraPermission.Granted)
        {
            CameraPermission.Request();
            float t = 0f;
            while (t < 8f && !CameraPermission.Granted)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        if (!CameraPermission.Granted)
        {
            UsePlaceholder();
            yield break;
        }
        camOverlay.SetActive(true);
        webcam.Play(_preferBack);
        yield return null;
        if (webcam.Texture == null)
            UsePlaceholder();
    }

    void CloseCamera()
    {
        webcam?.Stop();
        camOverlay?.SetActive(false);
    }

    void Capture()
    {
        var tex = webcam.CaptureStill();
        CloseCamera();
        if (tex == null) UsePlaceholder();
        else SetPhoto(tex);
        JuiceAudio.I?.PlayPop();
        Haptics.Tick();
    }

    void PickGallery()
    {
        _app.Picker.Pick(path =>
        {
            if (string.IsNullOrEmpty(path)) return;
            var tex = ImageUtil.Load(path);
            if (tex != null) SetPhoto(tex);
        });
    }

    public void ReceivePickedPhoto(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var tex = ImageUtil.Load(path);
        if (tex != null) SetPhoto(tex);
    }

    void UsePlaceholder()
    {
        SetPhoto(ImageUtil.Placeholder("Khoảnh khắc"));
        JuiceAudio.I?.PlayPop();
    }

    void SetPhoto(Texture2D tex)
    {
        _photo = tex;
        ImageUtil.ShowContained(preview, tex);
        SetHasPhoto(true);
    }

    public bool HandleBack()
    {
        if (camOverlay != null && camOverlay.activeSelf)
        {
            CloseCamera();
            return true;
        }
        if (group != null && group.interactable && gameObject.activeInHierarchy)
        {
            _app.CloseCreate();
            return true;
        }
        return false;
    }

    void ClearPhoto()
    {
        _photo = null;
        if (preview != null)
        {
            preview.sprite = RuntimeSprites.White;
            preview.color = Palette.Pearl;
        }
        SetHasPhoto(false);
    }

    void SetHasPhoto(bool on)
    {
        if (placeholder != null) placeholder.gameObject.SetActive(!on);
        releaseButton?.SetInteractable(on);
    }

    void Release()
    {
        if (_photo == null) return;
        var rec = _app.Repo.AddMemory(nameInput != null ? nameInput.text : "", captionInput != null ? captionInput.text : "", _photo);
        JuiceAudio.I?.PlayChime();
        _app.FinishCreate(rec);
    }
}
