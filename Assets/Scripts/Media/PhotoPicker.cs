using System;
using UnityEngine;

public class PhotoPicker : MonoBehaviour
{
    Action<string> _onPicked;

    public void Pick(Action<string> onPicked)
    {
        _onPicked = onPicked;
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Chọn ảnh", "", "png,jpg,jpeg");
        onPicked?.Invoke(path ?? "");
#elif UNITY_ANDROID
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var jc = new AndroidJavaClass("com.birthday.balloon.GalleryActivity");
            jc.CallStatic("pick", activity, gameObject.name);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
            onPicked?.Invoke("");
        }
#else
        onPicked?.Invoke("");
#endif
    }

    public void OnGalleryPicked(string path)
    {
        _onPicked?.Invoke(path ?? "");
        _onPicked = null;
    }
}
