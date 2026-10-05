using UnityEngine;

[CreateAssetMenu(menuName = "Birthday/App Settings", fileName = "AppSettings")]
public class AppSettings : ScriptableObject
{
    [Header("Camera")]
    [Tooltip("Độ phân giải ngang webcam")]
    public int webcamWidth = 1280;
    [Tooltip("Độ phân giải dọc webcam")]
    public int webcamHeight = 720;
    public int webcamFps = 30;
    [Tooltip("Mặc định dùng camera sau khi quét QR")]
    public bool qrPreferBackCamera = true;
    [Tooltip("Mặc định dùng camera trước khi chụp khoảnh khắc")]
    public bool capturePreferBackCamera = false;
    [Tooltip("Cạnh dài tối đa khi decode QR (px)")]
    public int qrDecodeMaxEdge = 480;
    [Tooltip("Khoảng cách giữa 2 lần quét QR (giây)")]
    public float qrScanInterval = 0.14f;
    [Tooltip("Kích thước ảnh QR xuất ra (px)")]
    public int qrExportSize = 512;

    [Header("Lưu trữ local")]
    public string dataFolderName = "bbm";
    public string storeFileName = "store.json";
    public string imagesFolderName = "images";
    public string qrExportFileName = "test-qr.png";
    [Range(128, 2048)] public int maxImageEdge = 512;
    [Range(40, 100)] public int jpegQuality = 88;

    [Header("API / Server")]
    [Tooltip("Bật để đồng bộ bóng lên server")]
    public bool useRemoteApi;
    [Tooltip("Ví dụ https://api.example.com")]
    public string apiBaseUrl = "";
    public string apiBalloonsPath = "/api/balloons";
    public string apiUploadPath = "/api/balloons/upload";
    public string apiAuthHeader = "Authorization";
    public string apiAuthToken = "";
    public float apiTimeoutSeconds = 15f;

    [Header("Hiển thị")]
    public Vector2 referenceResolution = new Vector2(1080, 1920);
    [Range(0f, 1f)] public float canvasMatch = 0.5f;
    public int targetFrameRate = 60;
    public float introDuration = 2.6f;
    public float introSkipAfter = 0.6f;

    [Header("Bong bóng")]
    public Vector2 balloonPrefabSize = new Vector2(210, 260);

    public string PersistentRoot =>
        System.IO.Path.Combine(Application.persistentDataPath, string.IsNullOrEmpty(dataFolderName) ? "bbm" : dataFolderName);

    public string StorePath => System.IO.Path.Combine(PersistentRoot, storeFileName);
    public string ImagesPath => System.IO.Path.Combine(PersistentRoot, imagesFolderName);
    public string QrExportPath => System.IO.Path.Combine(PersistentRoot, qrExportFileName);

    public string BalloonsUrl
    {
        get
        {
            string root = (apiBaseUrl ?? "").TrimEnd('/');
            string path = apiBalloonsPath ?? "";
            if (!path.StartsWith("/")) path = "/" + path;
            return root + path;
        }
    }

    public string UploadUrl
    {
        get
        {
            string root = (apiBaseUrl ?? "").TrimEnd('/');
            string path = apiUploadPath ?? "";
            if (!path.StartsWith("/")) path = "/" + path;
            return root + path;
        }
    }
}
