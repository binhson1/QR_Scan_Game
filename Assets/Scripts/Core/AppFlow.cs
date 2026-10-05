using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class AppFlow : MonoBehaviour
{
    [Header("Cấu hình (chỉnh trên Inspector)")]
    [SerializeField] EventConfig config;
    [SerializeField] AppSettings settings;

    [Header("UI — kéo Canvas / màn hình / prefab vào đây")]
    [SerializeField] Canvas uiCanvas;
    [SerializeField] QrScanScreen qrScreen;
    [SerializeField] IntroScreen introScreen;
    [SerializeField] BalloonRoomScreen roomScreen;
    [SerializeField] CreateBalloonScreen createScreen;
    [SerializeField] WishSheet wishSheet;
    [SerializeField] Image flashOverlay;
    [SerializeField] BalloonView balloonPrefab;

    [Header("Hệ thống")]
    [SerializeField] ApiClient apiClient;

    public EventConfig Config => config;
    public AppSettings Settings => settings;
    public BalloonView BalloonPrefab => balloonPrefab;
    public IBalloonRepository Repo { get; private set; }
    public PhotoPicker Picker { get; private set; }
    public ApiClient Api => apiClient;

    SpringState _flashA = SpringState.Make(0, 20f, 0.9f);
    bool _flashing;
    bool _toIntro;

    void Awake()
    {
        ThemeFonts.Ensure();
        RuntimeSprites.Ensure();
        if (config == null) config = Resources.Load<EventConfig>("EventConfig");
        if (config == null) config = ScriptableObject.CreateInstance<EventConfig>();
        if (settings == null) settings = Resources.Load<AppSettings>("AppSettings");
        if (settings == null) settings = ScriptableObject.CreateInstance<AppSettings>();

        Application.targetFrameRate = settings.targetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.AutoRotation;

        ApplyCanvasSettings();
        Repo = new LocalBalloonRepository(settings);
        Repo.EnsureSeeded();
        if (GetComponent<MainThread>() == null) gameObject.AddComponent<MainThread>();
        if (GetComponent<JuiceAudio>() == null) gameObject.AddComponent<JuiceAudio>();
        Picker = GetComponent<PhotoPicker>() ?? gameObject.AddComponent<PhotoPicker>();
        apiClient = GetComponent<ApiClient>() ?? gameObject.AddComponent<ApiClient>();
        apiClient.Bind(settings);

        if (qrScreen == null)
        {
            Debug.LogError("AppFlow: chưa gán UI. Chọn menu Birthday → Bake Editable UI Prefabs, hoặc kéo các màn hình từ Canvas vào Inspector.");
            return;
        }

        qrScreen.Bind(this);
        introScreen.Bind(this);
        roomScreen.Bind(this);
        createScreen.Bind(this);
        wishSheet.Bind(this);
    }

    void ApplyCanvasSettings()
    {
        if (uiCanvas == null) return;
        var scaler = uiCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = settings.referenceResolution;
        scaler.matchWidthOrHeight = settings.canvasMatch;
    }

    void Start()
    {
        if (qrScreen == null) return;
        qrScreen.Motion.Present(true);
        qrScreen.OnShown();
        if (PlayerPrefs.GetInt("BBM_AutoDemo", 0) == 1)
        {
            PlayerPrefs.SetInt("BBM_AutoDemo", 0);
            PlayerPrefs.Save();
            Invoke(nameof(FlashThenIntro), 0.4f);
            Invoke(nameof(OpenFirstWish), 3.4f);
            Invoke(nameof(DebugAddSampleMemory), 4.6f);
        }
    }

    public void FlashThenIntro()
    {
        if (qrScreen == null) return;
        _flashing = true;
        _toIntro = true;
        _flashA.Value = 0.95f;
        _flashA.Target = 0f;
        qrScreen.OnHidden();
        qrScreen.Motion.Dismiss();
    }

    public void GoScan()
    {
        wishSheet.Close();
        createScreen.OnHidden();
        createScreen.Motion.Dismiss();
        introScreen.OnHidden();
        introScreen.Motion.Dismiss();
        roomScreen.Motion.Dismiss();
        qrScreen.Motion.Present();
        qrScreen.OnShown();
        JuiceAudio.I?.PlayWhoosh();
    }

    public void GoRoom()
    {
        introScreen.OnHidden();
        introScreen.Motion.Dismiss();
        roomScreen.Motion.Present();
        roomScreen.Reload();
    }

    public void OpenWish(BalloonRecord rec) => wishSheet.Open(rec);

    public void OpenCreate()
    {
        createScreen.OnShown();
        createScreen.Motion.Present();
    }

    public void CloseCreate()
    {
        createScreen.OnHidden();
        createScreen.Motion.Dismiss();
    }

    public void FinishCreate(BalloonRecord rec)
    {
        wishSheet.Close();
        createScreen.OnHidden();
        createScreen.Motion.Dismiss();
        roomScreen.Motion.Present();
        roomScreen.Reload(true, rec.id);
        apiClient?.UploadMemory(rec, Repo.ImageAbsPath(rec));
    }

    public void OpenFirstWish()
    {
        if (Repo.All.Count > 0) OpenWish(Repo.All[0]);
    }

    public void DebugAddSampleMemory()
    {
        var tex = ImageUtil.Placeholder("Demo");
        FinishCreate(Repo.AddMemory("Play Mode", "Khoảnh khắc thử nghiệm từ Editor.", tex));
    }

    public void OnGalleryPicked(string path)
    {
        Picker.OnGalleryPicked(path);
        createScreen?.ReceivePickedPhoto(path);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleBack();

        if (!_flashing) return;
        _flashA.Step(Time.unscaledDeltaTime);
        if (flashOverlay != null)
        {
            var c = Color.white;
            c.a = Mathf.Clamp01(_flashA.Value);
            flashOverlay.color = c;
        }
        if (_toIntro && _flashA.Value < 0.65f)
        {
            _toIntro = false;
            introScreen.Motion.Present();
            introScreen.Play();
        }
        if (_flashA.Value < 0.02f) _flashing = false;
    }

    void HandleBack()
    {
        if (createScreen != null && createScreen.HandleBack()) return;
        if (wishSheet != null && wishSheet.IsOpen)
        {
            wishSheet.Close();
            return;
        }
        if (introScreen != null && introScreen.Motion != null && introScreen.Motion.Shown)
        {
            GoRoom();
            return;
        }
        if (roomScreen != null && roomScreen.Motion != null && roomScreen.Motion.Shown)
            GoScan();
    }

    public static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

#if UNITY_EDITOR
    public void EditorAssign(
        EventConfig eventConfig,
        AppSettings appSettings,
        Canvas canvas,
        QrScanScreen qr,
        IntroScreen intro,
        BalloonRoomScreen room,
        CreateBalloonScreen create,
        WishSheet wish,
        Image flash,
        BalloonView balloon)
    {
        config = eventConfig;
        settings = appSettings;
        uiCanvas = canvas;
        qrScreen = qr;
        introScreen = intro;
        roomScreen = room;
        createScreen = create;
        wishSheet = wish;
        flashOverlay = flash;
        balloonPrefab = balloon;
    }
#endif
}
