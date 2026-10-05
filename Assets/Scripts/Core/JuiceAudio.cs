using UnityEngine;

public class JuiceAudio : MonoBehaviour
{
    public static JuiceAudio I { get; private set; }

    [Header("Âm lượng")]
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.05f;
    [Range(0f, 1f)] [SerializeField] float sfxVolume = 1f;

    [Header("Clip tùy chọn (để trống = procedural)")]
    [SerializeField] AudioClip tickClip;
    [SerializeField] AudioClip popClip;
    [SerializeField] AudioClip whooshClip;
    [SerializeField] AudioClip chimeClip;
    [SerializeField] AudioClip errorClip;
    [SerializeField] AudioClip successClip;
    [SerializeField] AudioClip padClip;

    AudioSource _sfx;
    AudioSource _music;
    AudioClip _tick, _pop, _whoosh, _chime, _error, _success, _pad;
    bool _muted;

    public bool Muted => _muted;

    void Awake()
    {
        I = this;
        _muted = PlayerPrefs.GetInt("bbm_muted", 0) == 1;
        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;
        _sfx.spatialBlend = 0f;
        _music = gameObject.AddComponent<AudioSource>();
        _music.playOnAwake = false;
        _music.loop = true;
        _music.spatialBlend = 0f;
        BuildClips();
        _music.clip = _pad;
        _music.volume = _muted ? 0f : musicVolume;
        _music.Play();
    }

    public void ToggleMute()
    {
        _muted = !_muted;
        PlayerPrefs.SetInt("bbm_muted", _muted ? 1 : 0);
        _music.volume = _muted ? 0f : musicVolume;
        if (!_muted) PlayTick();
    }

    public void PlayTick() => OneShot(_tick, 0.42f, 0.96f, 1.08f);
    public void PlayPop() => OneShot(_pop, 0.55f, 0.94f, 1.06f);
    public void PlayWhoosh() => OneShot(_whoosh, 0.38f, 0.9f, 1.1f);
    public void PlayChime() => OneShot(_chime, 0.5f, 0.98f, 1.03f);
    public void PlayError() => OneShot(_error, 0.45f, 0.95f, 1.02f);
    public void PlaySuccess() => OneShot(_success, 0.58f, 0.98f, 1.04f);

    void OneShot(AudioClip clip, float volume, float pitchMin, float pitchMax)
    {
        if (_muted || clip == null) return;
        _sfx.pitch = Random.Range(pitchMin, pitchMax);
        _sfx.PlayOneShot(clip, volume * sfxVolume);
    }

    void BuildClips()
    {
        _tick = tickClip != null ? tickClip : Tone(0.045f, 880f, 42f, 0.5f);
        _pop = popClip != null ? popClip : Pop();
        _whoosh = whooshClip != null ? whooshClip : Whoosh();
        _chime = chimeClip != null ? chimeClip : Chord(new[] { 523.25f, 659.25f, 783.99f }, 0.55f);
        _error = errorClip != null ? errorClip : Tone(0.16f, 196f, 14f, 0.45f, 0.35f);
        _success = successClip != null ? successClip : Chord(new[] { 392f, 523.25f, 659.25f, 1046.5f }, 0.7f);
        _pad = padClip != null ? padClip : Pad();
    }

    static AudioClip Tone(float seconds, float hz, float decay, float amp, float noise = 0f)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * seconds);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Exp(-t * decay);
            float s = Mathf.Sin(2f * Mathf.PI * hz * t);
            if (noise > 0f) s = s * (1f - noise) + (Random.value * 2f - 1f) * noise;
            data[i] = s * env * amp;
        }
        var clip = AudioClip.Create("tone", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip Chord(float[] hz, float seconds)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * seconds);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Pow(Mathf.Clamp01(1f - t / seconds), 1.4f) * Mathf.Clamp01(t * 80f);
            float s = 0f;
            for (int k = 0; k < hz.Length; k++)
                s += Mathf.Sin(2f * Mathf.PI * hz[k] * t) / hz.Length;
            data[i] = s * env * 0.55f;
        }
        var clip = AudioClip.Create("chord", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip Pop()
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * 0.12f);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Exp(-t * 28f);
            float s = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(420f, 180f, t * 8f) * t);
            s += (Random.value * 2f - 1f) * 0.35f * Mathf.Exp(-t * 60f);
            data[i] = s * env * 0.5f;
        }
        var clip = AudioClip.Create("pop", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip Whoosh()
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * 0.28f);
        var data = new float[n];
        float lp = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Sin(Mathf.PI * t / 0.28f);
            float nse = Random.value * 2f - 1f;
            lp = Mathf.Lerp(lp, nse, 0.18f);
            data[i] = lp * env * 0.42f;
        }
        var clip = AudioClip.Create("whoosh", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip Pad()
    {
        int sr = 44100;
        int n = sr * 4;
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float trem = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * 0.22f * t);
            float s = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.45f
                      + Mathf.Sin(2f * Mathf.PI * 329.63f * t) * 0.35f
                      + Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.12f;
            data[i] = s * trem * 0.22f;
        }
        var clip = AudioClip.Create("pad", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
