using System;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public static class CameraPermission
{
    public static bool Granted
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
            return true;
#endif
        }
    }

    public static void Request()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Granted) Permission.RequestUserPermission(Permission.Camera);
#endif
    }

    public static void OpenSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");
            using var uriClass = new AndroidJavaClass("android.net.Uri");
            using var uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null);
            intent.Call<AndroidJavaObject>("setData", uri);
            activity.Call("startActivity", intent);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
#endif
    }
}
