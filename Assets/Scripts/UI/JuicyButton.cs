using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class JuicyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Tooltip("Gán sự kiện trên Inspector (Button thường).")]
    public UnityEvent OnClicked = new UnityEvent();

    public Action Clicked;
    public bool Interactable = true;

    SpringState _scale = SpringState.Make(1f, 22f, 0.46f);
    RectTransform _rt;
    CanvasGroup _cg;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        if (_rt == null) _rt = (RectTransform)transform;
        _scale.Step(Time.unscaledDeltaTime);
        _rt.localScale = Vector3.one * _scale.Value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable) return;
        _scale.Target = 0.92f;
        JuiceAudio.I?.PlayTick();
        Haptics.Tick();
    }

    public void OnPointerUp(PointerEventData eventData) => _scale.Target = 1f;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable) return;
        _scale.Value = 1.07f;
        _scale.Velocity = 2.5f;
        _scale.Target = 1f;
        Clicked?.Invoke();
        OnClicked?.Invoke();
    }

    public void SetInteractable(bool on)
    {
        Interactable = on;
        if (_cg == null) _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = on ? 1f : 0.45f;
        _cg.blocksRaycasts = on;
    }
}
