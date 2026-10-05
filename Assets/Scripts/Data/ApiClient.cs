using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [SerializeField] AppSettings settings;

    public void Bind(AppSettings appSettings) => settings = appSettings;

    public bool Enabled =>
        settings != null && settings.useRemoteApi && !string.IsNullOrWhiteSpace(settings.apiBaseUrl);

    public void UploadMemory(BalloonRecord rec, string imagePath)
    {
        if (!Enabled || rec == null) return;
        StartCoroutine(UploadCo(rec, imagePath));
    }

    IEnumerator UploadCo(BalloonRecord rec, string imagePath)
    {
        string url = settings.UploadUrl;
        if (string.IsNullOrEmpty(url) || url.StartsWith("/"))
        {
            Debug.LogWarning("API upload URL chưa đúng. Điền apiBaseUrl trong App Settings.");
            yield break;
        }

        var form = new WWWForm();
        form.AddField("id", rec.id ?? "");
        form.AddField("name", rec.displayName ?? "");
        form.AddField("message", rec.message ?? "");
        form.AddField("kind", rec.kind ?? "Memory");
        if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
            form.AddBinaryData("image", System.IO.File.ReadAllBytes(imagePath), rec.imageFile ?? "photo.jpg", "image/jpeg");

        using var req = UnityWebRequest.Post(url, form);
        req.timeout = Mathf.CeilToInt(settings.apiTimeoutSeconds);
        if (!string.IsNullOrEmpty(settings.apiAuthToken))
            req.SetRequestHeader(settings.apiAuthHeader, settings.apiAuthToken);

        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning("API upload thất bại: " + req.error + " " + req.downloadHandler?.text);
        else
            Debug.Log("API upload OK: " + req.downloadHandler?.text);
    }
}
