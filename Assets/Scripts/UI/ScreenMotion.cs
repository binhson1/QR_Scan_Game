using UnityEngine;

public class ScreenMotion : MonoBehaviour
{
    public CanvasGroup Group;
    public RectTransform Rt;
    public bool Shown;

    SpringState _alpha = SpringState.Make(0f, 22f, 0.88f);
    Spring2 _pos = Spring2.Make(Vector2.zero, 18f, 0.8f);
    bool _wantsOff;

    public void Bind(CanvasGroup group)
    {
        Group = group;
        Rt = (RectTransform)group.transform;
    }

    public void Present(bool instant = false)
    {
        Shown = true;
        _wantsOff = false;
        gameObject.SetActive(true);
        Group.gameObject.SetActive(true);
        Group.interactable = true;
        Group.blocksRaycasts = true;
        _pos.Target = Vector2.zero;
        _alpha.Target = 1f;
        if (instant)
        {
            _alpha.Value = 1f;
            _pos.Value = Vector2.zero;
            Apply();
        }
        else
        {
            _pos.Value = new Vector2(48f, 0f);
            _alpha.Value = 0.15f;
            JuiceAudio.I?.PlayWhoosh();
        }
    }

    public void Dismiss(bool instant = false)
    {
        Shown = false;
        Group.interactable = false;
        Group.blocksRaycasts = false;
        _alpha.Target = 0f;
        _pos.Target = new Vector2(-70f, 0f);
        _wantsOff = true;
        if (instant)
        {
            _alpha.Value = 0f;
            Apply();
            Group.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        _alpha.Step(dt);
        _pos.Step(dt);
        Apply();
        if (_wantsOff && _alpha.Value < 0.02f && !Shown)
            Group.gameObject.SetActive(false);
    }

    void Apply()
    {
        Group.alpha = Mathf.Clamp01(_alpha.Value);
        Rt.anchoredPosition = _pos.Value;
    }
}
