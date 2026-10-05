#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class BirthdayUiBaker
{
    const string ScenePath = "Assets/Scenes/Birthday.unity";
    const string ConfigPath = "Assets/Resources/EventConfig.asset";
    const string SettingsPath = "Assets/Resources/AppSettings.asset";
    const string PrefabFolder = "Assets/Prefabs/UI";

    [MenuItem("Birthday/Bake Editable UI Prefabs", false, 0)]
    public static void Bake()
    {
        ThemeFonts.Ensure();
        Directory.CreateDirectory("Assets/Resources");
        Directory.CreateDirectory("Assets/Resources/UI");
        Directory.CreateDirectory(PrefabFolder);
        EnsureUiSprites();
        RuntimeSprites.Ensure();

        var cfg = LoadOrCreate<EventConfig>(ConfigPath);
        var settings = LoadOrCreate<AppSettings>(SettingsPath);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        AppFlow.EnsureEventSystem();

        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Palette.Blush;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;
        }

        var flow = Object.FindFirstObjectByType<AppFlow>();
        if (flow == null)
        {
            var app = new GameObject("App");
            flow = app.AddComponent<AppFlow>();
        }

        if (flow.GetComponent<ApiClient>() == null)
            flow.gameObject.AddComponent<ApiClient>();
        if (flow.GetComponent<JuiceAudio>() == null)
            flow.gameObject.AddComponent<JuiceAudio>();
        if (flow.GetComponent<PhotoPicker>() == null)
            flow.gameObject.AddComponent<PhotoPicker>();

        flow.EditorAssign(cfg, settings, null, null, null, null, null, null, null, null);

        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas.GetComponentInChildren<QrScanScreen>(true) != null ||
                canvas.name == "Canvas" || canvas.name == "UICanvas")
                Object.DestroyImmediate(canvas.gameObject);
        }

        var canvasGo = new GameObject("UICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvasComp = canvasGo.GetComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 1;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = settings.referenceResolution;
        scaler.matchWidthOrHeight = settings.canvasMatch;
        var root = canvasGo.GetComponent<RectTransform>();

        var balloonTemp = BalloonView.Create(root, new BalloonRecord { displayName = "Balloon", colorIndex = 0 }, null);
        balloonTemp.ApplySize(settings.balloonPrefabSize);
        balloonTemp.name = "BalloonView";
        string balloonPath = PrefabFolder + "/BalloonView.prefab";
        PrefabUtility.SaveAsPrefabAsset(balloonTemp.gameObject, balloonPath);
        Object.DestroyImmediate(balloonTemp.gameObject);
        var balloonPrefab = AssetDatabase.LoadAssetAtPath<BalloonView>(balloonPath);

        var qr = QrScanScreen.Build(root, flow);
        var intro = IntroScreen.Build(root, flow);
        var room = BalloonRoomScreen.Build(root, flow);
        var create = CreateBalloonScreen.Build(root, flow);
        var wish = WishSheet.Build(root, flow);

        qr.gameObject.SetActive(true);
        if (qr.Group != null)
        {
            qr.Group.alpha = 1f;
            qr.Group.interactable = true;
            qr.Group.blocksRaycasts = true;
        }
        intro.Motion.Dismiss(true);
        room.Motion.Dismiss(true);
        create.Motion.Dismiss(true);
        wish.gameObject.SetActive(false);

        var flash = Ui.Img(root, "Flash", RuntimeSprites.White, new Color(1, 1, 1, 0));
        flash.raycastTarget = false;
        Ui.Stretch(flash.rectTransform);
        flash.rectTransform.SetAsLastSibling();

        SaveCopy(qr.gameObject, PrefabFolder + "/QrScanScreen.prefab");
        SaveCopy(intro.gameObject, PrefabFolder + "/IntroScreen.prefab");
        SaveCopy(room.gameObject, PrefabFolder + "/BalloonRoomScreen.prefab");
        SaveCopy(create.gameObject, PrefabFolder + "/CreateBalloonScreen.prefab");
        SaveCopy(wish.gameObject, PrefabFolder + "/WishSheet.prefab");

        flow.EditorAssign(cfg, settings, canvasComp, qr, intro, room, create, wish, flash, balloonPrefab);
        var so = new SerializedObject(flow);
        so.FindProperty("config").objectReferenceValue = cfg;
        so.FindProperty("settings").objectReferenceValue = settings;
        so.FindProperty("uiCanvas").objectReferenceValue = canvasComp;
        so.FindProperty("qrScreen").objectReferenceValue = qr;
        so.FindProperty("introScreen").objectReferenceValue = intro;
        so.FindProperty("roomScreen").objectReferenceValue = room;
        so.FindProperty("createScreen").objectReferenceValue = create;
        so.FindProperty("wishSheet").objectReferenceValue = wish;
        so.FindProperty("flashOverlay").objectReferenceValue = flash;
        so.FindProperty("balloonPrefab").objectReferenceValue = balloonPrefab;
        var api = flow.GetComponent<ApiClient>();
        so.FindProperty("apiClient").objectReferenceValue = api;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(flow);
        EditorUtility.SetDirty(canvasGo);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = canvasGo;
        Debug.Log("Đã bake Canvas + prefab UI. Chỉnh layout trên UICanvas hoặc Prefabs/UI. Camera/API/đường dẫn nằm trên App Settings.");
    }

    static void SaveCopy(GameObject go, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(go, path);
    }

    static void EnsureUiSprites()
    {
        RuntimeSprites.Ensure();
        WriteSprite("ui_white", RuntimeSprites.White, 8f, Vector4.zero);
        WriteSprite("ui_circle", RuntimeSprites.Circle, 100f, Vector4.zero);
        WriteSprite("ui_rounded", RuntimeSprites.Rounded, 100f, new Vector4(16, 16, 16, 16));
        WriteSprite("ui_balloon", RuntimeSprites.Balloon, 100f, Vector4.zero);
        WriteSprite("ui_bokeh", RuntimeSprites.Bokeh, 64f, Vector4.zero);
        WriteSprite("ui_gradient", RuntimeSprites.Gradient, 16f, Vector4.zero);
        AssetDatabase.Refresh();
        RuntimeSprites.BindAssets(
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_white.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_circle.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_rounded.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_balloon.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_bokeh.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/ui_gradient.png"));
    }

    static void WriteSprite(string name, Sprite src, float ppu, Vector4 border)
    {
        if (src == null || src.texture == null) return;
        string path = "Assets/Resources/UI/" + name + ".png";
        if (!File.Exists(path))
            File.WriteAllBytes(path, src.texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        if (imp == null) return;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled = false;
        imp.filterMode = FilterMode.Bilinear;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.spritePixelsPerUnit = ppu;
        imp.spriteBorder = border;
        EditorUtility.SetDirty(imp);
        imp.SaveAndReimport();
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}

[CustomEditor(typeof(AppFlow))]
public class AppFlowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "1) Chỉnh giao diện trên UICanvas trong Hierarchy (hoặc Prefabs/UI).\n" +
            "2) Camera, đường dẫn lưu, API: mở asset App Settings.\n" +
            "3) Mã sự kiện / copy: Event Config.\n" +
            "Nếu Canvas trống, bấm Bake bên dưới.",
            MessageType.Info);

        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Bake / Rebuild UI Prefabs", GUILayout.Height(36)))
            BirthdayUiBaker.Bake();
        if (GUILayout.Button("Ping App Settings"))
        {
            var s = AssetDatabase.LoadAssetAtPath<AppSettings>("Assets/Resources/AppSettings.asset");
            if (s != null) EditorGUIUtility.PingObject(s);
        }
        if (GUILayout.Button("Ping Event Config"))
        {
            var c = AssetDatabase.LoadAssetAtPath<EventConfig>("Assets/Resources/EventConfig.asset");
            if (c != null) EditorGUIUtility.PingObject(c);
        }
    }
}

[CustomEditor(typeof(AppSettings))]
public class AppSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "Camera: độ phân giải webcam & QR.\n" +
            "Lưu trữ: thư mục trong persistentDataPath.\n" +
            "API: bật useRemoteApi rồi điền base URL.",
            MessageType.None);
        DrawDefaultInspector();
        var s = (AppSettings)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Đường dẫn runtime", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(s.PersistentRoot);
        EditorGUILayout.SelectableLabel(s.StorePath);
        EditorGUILayout.SelectableLabel(s.ImagesPath);
        if (s.useRemoteApi)
        {
            EditorGUILayout.LabelField("Balloons URL", s.BalloonsUrl);
            EditorGUILayout.LabelField("Upload URL", s.UploadUrl);
        }
    }
}
#endif
