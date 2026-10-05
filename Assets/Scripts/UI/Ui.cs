using System;
using UnityEngine;
using UnityEngine.UI;

public static class Ui
{
    public static RectTransform Rt(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
        rt.localScale = Vector3.one;
    }

    public static void Center(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
    }

    public static Image Img(Transform parent, string name, Sprite sprite, Color color)
    {
        var rt = Rt(parent, name);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    public static Text Txt(Transform parent, string name, string content, int size, Color color,
        TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
    {
        var rt = Rt(parent, name);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = ThemeFonts.Regular;
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.supportRichText = false;
        return t;
    }

    public static JuicyButton Button(Transform parent, string label, Color bg, Color fg, Action onClick, Vector2 size)
    {
        var img = Img(parent, "Btn_" + label, RuntimeSprites.Rounded, bg);
        img.raycastTarget = true;
        RuntimeSprites.Slice(img);
        Center(img.rectTransform, size);
        var text = Txt(img.transform, "Label", label, 30, fg, TextAnchor.MiddleCenter, FontStyle.Bold);
        Stretch(text.rectTransform, 16, 16, 8, 8);
        var juicy = img.gameObject.AddComponent<JuicyButton>();
        juicy.Clicked = onClick;
        if (img.gameObject.GetComponent<CanvasGroup>() == null)
            img.gameObject.AddComponent<CanvasGroup>();
        return juicy;
    }

    public static InputField Input(Transform parent, string placeholder, int limit = 80)
    {
        var img = Img(parent, "Input", RuntimeSprites.Rounded, Palette.SoftWhite);
        img.raycastTarget = true;
        RuntimeSprites.Slice(img);
        var field = img.gameObject.AddComponent<InputField>();
        var ph = Txt(img.transform, "Placeholder", placeholder, 26, new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, 0.4f),
            TextAnchor.MiddleLeft);
        Stretch(ph.rectTransform, 22, 22, 6, 6);
        var tx = Txt(img.transform, "Text", "", 26, Palette.Ink, TextAnchor.MiddleLeft);
        Stretch(tx.rectTransform, 22, 22, 6, 6);
        tx.supportRichText = false;
        field.textComponent = tx;
        field.placeholder = ph;
        field.characterLimit = limit;
        field.transition = Selectable.Transition.None;
        field.caretColor = Palette.Rose;
        return field;
    }

    public static CanvasGroup Fullscreen(Transform parent, string name)
    {
        var rt = Rt(parent, name);
        Stretch(rt);
        var cg = rt.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        rt.gameObject.SetActive(false);
        return cg;
    }

    public static void Show(CanvasGroup cg, bool on)
    {
        cg.gameObject.SetActive(true);
        cg.alpha = on ? 1f : 0f;
        cg.interactable = on;
        cg.blocksRaycasts = on;
        if (!on) cg.gameObject.SetActive(false);
    }
}
