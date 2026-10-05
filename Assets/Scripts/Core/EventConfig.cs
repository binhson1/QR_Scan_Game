using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Birthday/Event Config", fileName = "EventConfig")]
public class EventConfig : ScriptableObject
{
    [Header("Sự kiện")]
    public string EventCode = "BIRTHDAY-SEP-0110";
    public string QrPrefix = "birthday:";
    public string Honoree = "Sếp";
    public string EventTitle = "Happy Birthday";
    public string AppTitle = "Birthday\nBalloon Memories";
    public string AppSubtitle = "Những lời chúc nhỏ — tạo nên một ngày thật lớn";

    [Header("Copy UI")]
    public string scanHint = "Quét QR để mở món quà đặc biệt";
    public string enterPartyLabel = "Vào tiệc";
    public string codePlaceholder = "Nhập mã sự kiện";
    public string createTitle = "Tạo bong bóng mới";
    public string roomTitle = "Không gian bong bóng";
    public string roomSubtitle = "Chạm vào bóng để xem ảnh + tên + lời chúc";

    public bool IsValidPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return false;
        payload = payload.Trim();
        if (payload.Equals(EventCode, StringComparison.OrdinalIgnoreCase)) return true;
        if (payload.StartsWith(QrPrefix, StringComparison.OrdinalIgnoreCase)) return true;
        return payload.IndexOf(EventCode, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void OnEnable()
    {
        if (string.IsNullOrEmpty(EventCode)) EventCode = "BIRTHDAY-SEP-0110";
        if (string.IsNullOrEmpty(QrPrefix)) QrPrefix = "birthday:";
        if (string.IsNullOrEmpty(Honoree)) Honoree = "Sếp";
        if (string.IsNullOrEmpty(AppTitle)) AppTitle = "Birthday\nBalloon Memories";
        if (string.IsNullOrEmpty(AppSubtitle)) AppSubtitle = "Những lời chúc nhỏ — tạo nên một ngày thật lớn";
        if (string.IsNullOrEmpty(scanHint)) scanHint = "Quét QR để mở món quà đặc biệt";
        if (string.IsNullOrEmpty(enterPartyLabel)) enterPartyLabel = "Vào tiệc";
        if (string.IsNullOrEmpty(codePlaceholder)) codePlaceholder = "Nhập mã sự kiện";
        if (string.IsNullOrEmpty(createTitle)) createTitle = "Tạo bong bóng mới";
        if (string.IsNullOrEmpty(roomTitle)) roomTitle = "Không gian bong bóng";
        if (string.IsNullOrEmpty(roomSubtitle)) roomSubtitle = "Chạm vào bóng để xem ảnh + tên + lời chúc";
    }
}
