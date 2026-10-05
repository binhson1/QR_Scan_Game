using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class QrScanScreen : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] CanvasGroup group;
    [SerializeField] ScreenMotion motion;
    [SerializeField] WebcamView webcam;
    [SerializeField] InputField codeInput;
    [SerializeField] Text statusText;
    [SerializeField] Text titleText;
    [SerializeField] Text subtitleText;
    [SerializeField] Text hintText;
    [SerializeField] RectTransform frame;
    [SerializeField] Image laser;
    [SerializeField] GameObject permCard;
    [SerializeField] GameObject qrModal;
    [SerializeField] Image qrPreview;
    [SerializeField] Text qrPathText;

    [Header("Buttons")]
    [SerializeField] JuicyButton muteButton;
    [SerializeField] JuicyButton enterButton;
    [SerializeField] JuicyButton exportButton;
    [SerializeField] JuicyButton allowCameraButton;
    [SerializeField] JuicyButton openSettingsButton;
    [SerializeField] JuicyButton closeQrModalButton;
    [SerializeField] JuicyButton dimQrModalButton;

    AppFlow _app;
    SpringState _shake = SpringState.Make(0, 28f, 0.45f);
    float _scanTimer;
    float _cooldown;
    bool _locked;
    bool _camOn;
    float _laserT;
    float _scanInterval = 0.14f;
    int _qrExportSize = 512;
    bool _preferBack = true;

    public CanvasGroup Group => group;
    public ScreenMotion Motion => motion;

    public static QrScanScreen Build(Transform parent, AppFlow app)
    {
        var cg = Ui.Fullscreen(parent, "QrScan");
        var s = cg.gameObject.AddComponent<QrScanScreen>();
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
        if (app != null && app.Settings != null)
        {
            _scanInterval = app.Settings.qrScanInterval;
            _qrExportSize = app.Settings.qrExportSize;
            _preferBack = app.Settings.qrPreferBackCamera;
            webcam?.ApplySettings(app.Settings, _preferBack);
        }
        ApplyCopy();
        Wire();
    }

    void ApplyCopy()
    {
        if (_app?.Config == null) return;
        if (titleText != null) titleText.text = _app.Config.AppTitle;
        if (subtitleText != null) subtitleText.text = _app.Config.AppSubtitle;
        if (hintText != null) hintText.text = _app.Config.scanHint;
        if (codeInput != null && codeInput.placeholder is Text ph)
            ph.text = _app.Config.codePlaceholder;
        if (enterButton != null)
        {
            var label = enterButton.GetComponentInChildren<Text>();
            if (label != null) label.text = _app.Config.enterPartyLabel;
        }
    }

    void Wire()
    {
        if (muteButton != null) muteButton.Clicked = () => JuiceAudio.I?.ToggleMute();
        if (enterButton != null) enterButton.Clicked = SubmitCode;
        if (exportButton != null) exportButton.Clicked = ShowTestQr;
        if (allowCameraButton != null) allowCameraButton.Clicked = () => StartCoroutine(GrantAndStart());
        if (openSettingsButton != null) openSettingsButton.Clicked = CameraPermission.OpenSettings;
        if (closeQrModalButton != null) closeQrModalButton.Clicked = () => qrModal.SetActive(false);
        if (dimQrModalButton != null) dimQrModalButton.Clicked = () => qrModal.SetActive(false);
    }

    void BuildUi(Transform root)
    {
        BokehBackground.Attach(root);
        var safe = Ui.Rt(root, "Safe");
        Ui.Stretch(safe, 28, 28, 24, 28);

        muteButton = Ui.Button(safe, "♪", Palette.SoftWhite, Palette.Rose, null, new Vector2(72, 72));
        var muteRt = (RectTransform)muteButton.transform;
        muteRt.anchorMin = muteRt.anchorMax = new Vector2(1, 1);
        muteRt.pivot = new Vector2(1, 1);
        muteRt.anchoredPosition = new Vector2(0, -8);

        titleText = Ui.Txt(safe, "Title", "Birthday\nBalloon Memories", 48, Palette.DeepRose, TextAnchor.UpperLeft, FontStyle.Bold);
        titleText.rectTransform.anchorMin = new Vector2(0, 1);
        titleText.rectTransform.anchorMax = new Vector2(1, 1);
        titleText.rectTransform.pivot = new Vector2(0.5f, 1);
        titleText.rectTransform.anchoredPosition = new Vector2(0, -8);
        titleText.rectTransform.sizeDelta = new Vector2(0, 130);

        subtitleText = Ui.Txt(safe, "Sub", "Những lời chúc nhỏ — tạo nên một ngày thật lớn", 22, Palette.Ink, TextAnchor.UpperLeft);
        subtitleText.rectTransform.anchorMin = new Vector2(0, 1);
        subtitleText.rectTransform.anchorMax = new Vector2(1, 1);
        subtitleText.rectTransform.pivot = new Vector2(0.5f, 1);
        subtitleText.rectTransform.anchoredPosition = new Vector2(0, -140);
        subtitleText.rectTransform.sizeDelta = new Vector2(0, 50);

        var frameImg = Ui.Img(safe, "Frame", RuntimeSprites.Rounded, Palette.SoftWhite);
        RuntimeSprites.Slice(frameImg);
        frameImg.raycastTarget = true;
        frame = frameImg.rectTransform;
        frame.anchorMin = new Vector2(0.08f, 0.34f);
        frame.anchorMax = new Vector2(0.92f, 0.72f);
        frame.offsetMin = Vector2.zero;
        frame.offsetMax = Vector2.zero;

        var inner = Ui.Rt(frame, "Inner");
        Ui.Stretch(inner, 14, 14, 14, 14);
        var maskImg = inner.gameObject.AddComponent<Image>();
        maskImg.sprite = RuntimeSprites.Rounded;
        maskImg.color = Color.white;
        maskImg.type = Image.Type.Sliced;
        inner.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        webcam = WebcamView.Create(inner);

        laser = Ui.Img(inner, "Laser", RuntimeSprites.White, new Color(Palette.Rose.r, Palette.Rose.g, Palette.Rose.b, 0.65f));
        laser.rectTransform.anchorMin = new Vector2(0.08f, 0.5f);
        laser.rectTransform.anchorMax = new Vector2(0.92f, 0.5f);
        laser.rectTransform.sizeDelta = new Vector2(0, 4);

        hintText = Ui.Txt(safe, "Hint", "Quét QR để mở món quà đặc biệt", 24, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
        hintText.rectTransform.anchorMin = new Vector2(0, 0.28f);
        hintText.rectTransform.anchorMax = new Vector2(1, 0.34f);
        hintText.rectTransform.offsetMin = hintText.rectTransform.offsetMax = Vector2.zero;

        statusText = Ui.Txt(safe, "Status", "", 20, Palette.DeepRose, TextAnchor.MiddleCenter);
        statusText.rectTransform.anchorMin = new Vector2(0, 0.24f);
        statusText.rectTransform.anchorMax = new Vector2(1, 0.28f);
        statusText.rectTransform.offsetMin = statusText.rectTransform.offsetMax = Vector2.zero;

        codeInput = Ui.Input(safe, "Nhập mã sự kiện", 40);
        var codeRt = codeInput.GetComponent<RectTransform>();
        codeRt.anchorMin = new Vector2(0, 0.155f);
        codeRt.anchorMax = new Vector2(1, 0.215f);
        codeRt.offsetMin = codeRt.offsetMax = Vector2.zero;

        enterButton = Ui.Button(safe, "Vào tiệc", Palette.Rose, Color.white, null, new Vector2(0, 88));
        var enterRt = (RectTransform)enterButton.transform;
        enterRt.anchorMin = new Vector2(0, 0.07f);
        enterRt.anchorMax = new Vector2(1, 0.14f);
        enterRt.offsetMin = enterRt.offsetMax = Vector2.zero;
        enterRt.sizeDelta = Vector2.zero;

        var export = Ui.Txt(safe, "Export", "Xuất QR thử", 22, Palette.DeepRose, TextAnchor.MiddleCenter, FontStyle.Bold);
        export.raycastTarget = true;
        exportButton = export.gameObject.AddComponent<JuicyButton>();
        export.rectTransform.anchorMin = new Vector2(0, 0.01f);
        export.rectTransform.anchorMax = new Vector2(1, 0.06f);
        export.rectTransform.offsetMin = export.rectTransform.offsetMax = Vector2.zero;

        BuildPermCard(root);
        BuildQrModal(root);
    }

    void BuildPermCard(Transform root)
    {
        permCard = Ui.Rt(root, "Perm").gameObject;
        Ui.Stretch(permCard.GetComponent<RectTransform>());
        var dim = Ui.Img(permCard.transform, "Dim", RuntimeSprites.White, Palette.Overlay);
        dim.raycastTarget = true;
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Img(permCard.transform, "Card", RuntimeSprites.Rounded, Palette.SoftWhite);
        RuntimeSprites.Slice(card);
        Ui.Center(card.rectTransform, new Vector2(860, 520));
        var t = Ui.Txt(card.transform, "T", "Cần camera để quét QR và chụp khoảnh khắc sinh nhật.", 30, Palette.Ink, TextAnchor.UpperCenter);
        Ui.Stretch(t.rectTransform, 40, 40, 40, 180);
        allowCameraButton = Ui.Button(card.transform, "Cho phép camera", Palette.Rose, Color.white, null, new Vector2(640, 88));
        ((RectTransform)allowCameraButton.transform).anchoredPosition = new Vector2(0, -40);
        openSettingsButton = Ui.Button(card.transform, "Mở cài đặt", Palette.Champagne, Palette.Ink, null, new Vector2(640, 80));
        ((RectTransform)openSettingsButton.transform).anchoredPosition = new Vector2(0, -140);
        permCard.SetActive(false);
    }

    void BuildQrModal(Transform root)
    {
        qrModal = Ui.Rt(root, "QrModal").gameObject;
        Ui.Stretch(qrModal.GetComponent<RectTransform>());
        var dim = Ui.Img(qrModal.transform, "Dim", RuntimeSprites.White, Palette.Overlay);
        dim.raycastTarget = true;
        Ui.Stretch(dim.rectTransform);
        dimQrModalButton = dim.gameObject.AddComponent<JuicyButton>();
        var card = Ui.Img(qrModal.transform, "Card", RuntimeSprites.Rounded, Palette.SoftWhite);
        RuntimeSprites.Slice(card);
        Ui.Center(card.rectTransform, new Vector2(820, 980));
        var title = Ui.Txt(card.transform, "T", "QR sự kiện", 34, Palette.DeepRose, TextAnchor.UpperCenter, FontStyle.Bold);
        title.rectTransform.anchoredPosition = new Vector2(0, 430);
        title.rectTransform.sizeDelta = new Vector2(700, 50);
        qrPreview = Ui.Img(card.transform, "QR", RuntimeSprites.White, Color.white);
        Ui.Center(qrPreview.rectTransform, new Vector2(520, 520));
        qrPreview.rectTransform.anchoredPosition = new Vector2(0, 80);
        qrPathText = Ui.Txt(card.transform, "P", "", 20, Palette.Ink, TextAnchor.MiddleCenter);
        qrPathText.rectTransform.anchoredPosition = new Vector2(0, -250);
        qrPathText.rectTransform.sizeDelta = new Vector2(740, 90);
        closeQrModalButton = Ui.Button(card.transform, "Đóng", Palette.Rose, Color.white, null, new Vector2(420, 80));
        ((RectTransform)closeQrModalButton.transform).anchoredPosition = new Vector2(0, -380);
        qrModal.SetActive(false);
    }

    IEnumerator GrantAndStart()
    {
        permCard.SetActive(false);
        CameraPermission.Request();
        float t = 0f;
        while (t < 8f && !CameraPermission.Granted)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (CameraPermission.Granted) TryStartCamera();
        else permCard.SetActive(true);
    }

    public void OnShown()
    {
        _locked = false;
        if (statusText != null) statusText.text = "";
        _scanTimer = 0;
        if (!CameraPermission.Granted)
            permCard.SetActive(true);
        else
            TryStartCamera();
    }

    public void OnHidden()
    {
        _camOn = false;
        webcam?.Stop();
        permCard?.SetActive(false);
        qrModal?.SetActive(false);
    }

    void TryStartCamera()
    {
        if (!CameraPermission.Granted)
        {
            statusText.text = "Không có quyền camera. Nhập mã sự kiện để vào.";
            return;
        }
        webcam.Play(_preferBack);
        _camOn = webcam.Texture != null;
        if (!_camOn)
            statusText.text = "Không có camera. Nhập mã " + (_app != null ? _app.Config.EventCode : "") + " để vào tiệc.";
    }

    void SubmitCode() => Accept(codeInput != null ? codeInput.text : "");

    void ShowTestQr()
    {
        string payload = _app.Config.QrPrefix + _app.Config.EventCode;
        var tex = QrCodec.Encode(payload, _qrExportSize);
        var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
        qrPreview.sprite = spr;
        qrPreview.preserveAspect = true;
        string path = _app.Settings != null ? _app.Settings.QrExportPath : Path.Combine(Application.persistentDataPath, "bbm", "test-qr.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, tex.EncodeToPNG());
        qrPathText.text = "Đã lưu:\n" + path + "\nMã: " + _app.Config.EventCode;
        qrModal.SetActive(true);
        JuiceAudio.I?.PlayPop();
    }

    void Accept(string payload)
    {
        if (_locked) return;
        if (_app.Config.IsValidPayload(payload))
        {
            _locked = true;
            statusText.text = "Mở quà thành công!";
            JuiceAudio.I?.PlaySuccess();
            Haptics.Success();
            _app.FlashThenIntro();
        }
        else
        {
            statusText.text = "Mã chưa đúng, thử lại nhé.";
            JuiceAudio.I?.PlayError();
            Haptics.Error();
            _shake.Velocity = 720f;
            _cooldown = 1.2f;
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        _shake.Target = 0f;
        _shake.Step(dt);
        if (frame != null)
            frame.anchoredPosition = new Vector2(_shake.Value, frame.anchoredPosition.y);

        _laserT += dt;
        if (laser != null)
        {
            float y = Mathf.PingPong(_laserT * 0.35f, 1f);
            laser.rectTransform.anchorMin = new Vector2(0.08f, 0.08f + y * 0.84f);
            laser.rectTransform.anchorMax = new Vector2(0.92f, 0.08f + y * 0.84f);
        }

        if (_cooldown > 0f) _cooldown -= dt;
        if (_locked || !_camOn || _cooldown > 0f || webcam == null) return;
        var tex = webcam.Texture;
        if (tex == null || !tex.isPlaying || tex.width < 16) return;
        _scanTimer += dt;
        if (_scanTimer < _scanInterval) return;
        _scanTimer = 0f;
        var pixels = tex.GetPixels32();
        int rot = tex.videoRotationAngle;
        int maxEdge = _app != null && _app.Settings != null ? _app.Settings.qrDecodeMaxEdge : 480;
        QrCodec.DecodeAsync(pixels, tex.width, tex.height, rot, text =>
        {
            if (_locked || string.IsNullOrEmpty(text)) return;
            Accept(text);
        }, maxEdge);
    }
}
