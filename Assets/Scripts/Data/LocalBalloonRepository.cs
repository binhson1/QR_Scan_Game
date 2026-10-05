using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IBalloonRepository
{
    IReadOnlyList<BalloonRecord> All { get; }
    void EnsureSeeded();
    BalloonRecord AddMemory(string displayName, string message, Texture2D photo);
    Texture2D LoadImage(BalloonRecord record);
    string ImageAbsPath(BalloonRecord record);
}

[Serializable]
class BalloonStoreFile
{
    public BalloonRecord[] items;
}

public class LocalBalloonRepository : IBalloonRepository
{
    readonly string _root;
    readonly string _jsonPath;
    readonly string _images;
    readonly int _maxEdge;
    readonly int _jpegQuality;
    readonly List<BalloonRecord> _items = new List<BalloonRecord>();

    public IReadOnlyList<BalloonRecord> All => _items;

    static readonly (string name, string msg)[] Seeds =
    {
        ("Minh Anh", "Chúc sếp tuổi mới thật rạng rỡ, luôn mạnh khỏe và dẫn dắt team bay cao!"),
        ("Hoàng Long", "Cảm ơn sếp đã luôn tin tưởng. Chúc sếp một ngày sinh nhật thật ấm áp!"),
        ("Thu Hà", "Chúc sếp luôn vui vẻ, giữ nụ cười tươi và thật nhiều năng lượng tích cực!"),
        ("Quốc Bảo", "Sinh nhật vui vẻ sếp ơi! Team luôn đứng sau sếp trên mọi chặng đường."),
        ("Lan Chi", "Chúc sếp một năm mới đầy ý tưởng, sức khỏe dồi dào và bình an bên gia đình."),
        ("Đức Anh", "Happy Birthday sếp! Cảm ơn những lần sếp chỉ bảo rất tận tâm."),
        ("Phương Thảo", "Chúc sếp tuổi mới ngọt ngào như bánh kem, nhẹ nhàng như bóng bay hôm nay."),
        ("Khánh Vy", "Sếp là điểm tựa của tụi mình. Chúc sếp luôn hạnh phúc và rạng ngời!"),
        ("Tuấn Kiệt", "Chúc sếp sinh nhật vui, làm việc hăng say, nghỉ ngơi đúng điệu và luôn được yêu mến.")
    };

    public LocalBalloonRepository(AppSettings settings)
    {
        if (settings == null)
        {
            _root = Path.Combine(Application.persistentDataPath, "bbm");
            _jsonPath = Path.Combine(_root, "store.json");
            _images = Path.Combine(_root, "images");
            _maxEdge = 512;
            _jpegQuality = 88;
        }
        else
        {
            _root = settings.PersistentRoot;
            _jsonPath = settings.StorePath;
            _images = settings.ImagesPath;
            _maxEdge = Mathf.Clamp(settings.maxImageEdge, 128, 2048);
            _jpegQuality = Mathf.Clamp(settings.jpegQuality, 40, 100);
        }
        Directory.CreateDirectory(_images);
    }

    public void EnsureSeeded()
    {
        if (File.Exists(_jsonPath))
        {
            try
            {
                var file = JsonUtility.FromJson<BalloonStoreFile>(File.ReadAllText(_jsonPath));
                _items.Clear();
                if (file?.items != null) _items.AddRange(file.items);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Balloon store read failed: " + ex.Message);
            }
        }

        if (_items.Count == 0)
        {
            for (int i = 0; i < Seeds.Length; i++)
            {
                _items.Add(new BalloonRecord
                {
                    id = "seed-" + i,
                    displayName = Seeds[i].name,
                    message = Seeds[i].msg,
                    imageFile = "",
                    createdUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (Seeds.Length - i) * 60,
                    isSeed = true,
                    colorIndex = i,
                    kind = "Wish"
                });
            }
            Save();
        }
    }

    public BalloonRecord AddMemory(string displayName, string message, Texture2D photo)
    {
        string id = Guid.NewGuid().ToString("N");
        string file = id + ".jpg";
        if (photo != null)
        {
            var scaled = ImageUtil.ScaleMax(photo, _maxEdge);
            File.WriteAllBytes(Path.Combine(_images, file), scaled.EncodeToJPG(_jpegQuality));
            if (scaled != photo) UnityEngine.Object.Destroy(scaled);
        }
        else file = "";

        var rec = new BalloonRecord
        {
            id = id,
            displayName = string.IsNullOrWhiteSpace(displayName) ? "Bạn" : displayName.Trim(),
            message = string.IsNullOrWhiteSpace(message) ? "Một khoảnh khắc đáng nhớ." : message.Trim(),
            imageFile = file,
            createdUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            isSeed = false,
            colorIndex = _items.Count % Palette.Balloons.Length,
            kind = "Memory"
        };
        _items.Add(rec);
        Save();
        return rec;
    }

    public Texture2D LoadImage(BalloonRecord record)
    {
        string path = ImageAbsPath(record);
        return string.IsNullOrEmpty(path) ? null : ImageUtil.Load(path);
    }

    public string ImageAbsPath(BalloonRecord record)
    {
        if (record == null || string.IsNullOrEmpty(record.imageFile)) return "";
        return Path.Combine(_images, record.imageFile);
    }

    void Save()
    {
        var file = new BalloonStoreFile { items = _items.ToArray() };
        File.WriteAllText(_jsonPath, JsonUtility.ToJson(file, true));
    }
}
