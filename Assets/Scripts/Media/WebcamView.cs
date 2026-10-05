using UnityEngine;
using UnityEngine.UI;

public class WebcamView : MonoBehaviour
{
    [Header("Camera (có thể override từ App Settings)")]
    [SerializeField] int requestedWidth = 1280;
    [SerializeField] int requestedHeight = 720;
    [SerializeField] int requestedFps = 30;
    [SerializeField] bool preferBackCamera = true;
    [SerializeField] RawImage preview;

    public WebCamTexture Texture { get; private set; }
    public int Rotation { get; private set; }
    public bool VerticallyMirrored { get; private set; }

    RectTransform _rawRt;
    int _index;

    public static WebcamView Create(Transform parent)
    {
        var raw = new GameObject("Webcam", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        raw.transform.SetParent(parent, false);
        raw.color = Color.white;
        raw.raycastTarget = false;
        raw.uvRect = new Rect(0, 0, 1, 1);
        var rt = raw.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(8, 8);
        var view = raw.gameObject.AddComponent<WebcamView>();
        view.preview = raw;
        view._rawRt = rt;
        return view;
    }

    public void ApplySettings(AppSettings settings, bool preferBack)
    {
        if (settings == null) return;
        requestedWidth = Mathf.Max(160, settings.webcamWidth);
        requestedHeight = Mathf.Max(120, settings.webcamHeight);
        requestedFps = Mathf.Max(8, settings.webcamFps);
        preferBackCamera = preferBack;
    }

    void Awake()
    {
        if (preview == null) preview = GetComponent<RawImage>();
        _rawRt = (RectTransform)transform;
    }

    public void Play(bool preferBack)
    {
        preferBackCamera = preferBack;
        Play();
    }

    public void Play()
    {
        Stop();
        var devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0) return;
        _index = 0;
        for (int i = 0; i < devices.Length; i++)
        {
            bool front = devices[i].isFrontFacing;
            if (preferBackCamera && !front) { _index = i; break; }
            if (!preferBackCamera && front) { _index = i; break; }
        }
        Texture = new WebCamTexture(devices[_index].name, requestedWidth, requestedHeight, requestedFps);
        if (preview == null) preview = GetComponent<RawImage>();
        if (preview != null) preview.texture = Texture;
        Texture.Play();
    }

    public void Flip()
    {
        preferBackCamera = !preferBackCamera;
        Play();
    }

    public Texture2D CaptureStill() => ImageUtil.FromWebcam(Texture);

    public void Stop()
    {
        if (Texture == null) return;
        Texture.Stop();
        Destroy(Texture);
        Texture = null;
        if (preview != null) preview.texture = null;
    }

    void LateUpdate()
    {
        if (Texture == null || Texture.width < 16) return;
        Rotation = Texture.videoRotationAngle;
        VerticallyMirrored = Texture.videoVerticallyMirrored;
        FitPreview();
    }

    void FitPreview()
    {
        if (_rawRt == null) _rawRt = (RectTransform)transform;
        int rot = ((Rotation % 360) + 360) % 360;
        _rawRt.localEulerAngles = new Vector3(0f, 0f, -rot);
        _rawRt.localScale = new Vector3(1f, VerticallyMirrored ? -1f : 1f, 1f);

        var parent = _rawRt.parent as RectTransform;
        if (parent == null) return;
        float cw = parent.rect.width;
        float ch = parent.rect.height;
        if (cw < 2f || ch < 2f) return;

        float tw = Texture.width;
        float th = Texture.height;
        bool swapped = rot == 90 || rot == 270;
        float orientedAspect = swapped ? th / tw : tw / th;

        float displayW;
        float displayH;
        if (cw / ch > orientedAspect)
        {
            displayH = ch;
            displayW = ch * orientedAspect;
        }
        else
        {
            displayW = cw;
            displayH = cw / orientedAspect;
        }

        _rawRt.sizeDelta = swapped ? new Vector2(displayH, displayW) : new Vector2(displayW, displayH);
        _rawRt.anchoredPosition = Vector2.zero;
    }

    void OnDestroy() => Stop();
}
