using System;

[Serializable]
public class BalloonRecord
{
    public string id;
    public string displayName;
    public string message;
    public string imageFile;
    public long createdUnix;
    public bool isSeed;
    public int colorIndex;
    public string kind;

    public bool HasImage => !string.IsNullOrEmpty(imageFile);
}
