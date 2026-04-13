using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public enum BGMState
{
    None,
    Chill,
    Exploration,
    Brewing
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer mixer;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;
    private bool useA = true;

    [Header("Playlists")]
    public List<AudioClip> chillPlaylist;
    public List<AudioClip> explorationPlaylist;
    public List<AudioClip> brewingPlaylist;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip defaultButtonSFX;

    [Header("UI Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    private Coroutine playlistRoutine;
    private Coroutine crossfadeRoutine;
    private Coroutine debounceRoutine;

    private BGMState currentState = BGMState.None;
    private BGMState pendingState = BGMState.None;



    // ======================================================
    // INITIALIZE
    // ======================================================
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        bgmSlider.value = bgm;
        sfxSlider.value = sfx;

        SetBGMVolume();
        SetSFXVolume();

        RequestState(BGMState.Chill);
    }


    // ======================================================
    // REQUEST BGM STATE (Debounce)
    // ======================================================

    public void RequestExplore(bool isExplore)
    {
        if (isExplore)
            RequestState(BGMState.Exploration);
        else
            RequestState(BGMState.Chill);
    }
    public void RequestBrewing(bool isBrewing)
    {
        if (isBrewing)
            RequestState(BGMState.Brewing);
        else
            RequestState(BGMState.Chill);
    }

    public void RequestState(BGMState newState)
    {
        if (newState == currentState)
            return;

        pendingState = newState;
        ApplyState(pendingState);
    }

    // ======================================================
    // APPLY STATE
    // ======================================================
    private void ApplyState(BGMState state)
    {
        currentState = state;

        List<AudioClip> playlist = null;

        switch (state)
        {
            case BGMState.Chill:
                playlist = chillPlaylist;
                break;

            case BGMState.Exploration:
                playlist = explorationPlaylist;
                break;

            case BGMState.Brewing:
                playlist = brewingPlaylist;
                break;
        }

        StartPlaylist(playlist);

    }


    // ======================================================
    // PLAYLIST SYSTEM
    // ======================================================
    private void StartPlaylist(List<AudioClip> playlist)
    {
        if (playlist == null || playlist.Count == 0)
            return;

        if (playlistRoutine != null)
            StopCoroutine(playlistRoutine);

        playlistRoutine = StartCoroutine(PlaylistRoutine(playlist));
    }

    private IEnumerator PlaylistRoutine(List<AudioClip> playlist)
    {
        int index = Random.Range(0, playlist.Count);

        while (true)
        {
            AudioClip clip = playlist[index];

            yield return CrossfadeToClip(clip, 1f);

            // Bảo đảm không cắt mất fade-out → fade-in
            yield return new WaitForSeconds(clip.length - 1f);

            index = (index + 1) % playlist.Count;
        }
    }


    // ======================================================
    // CROSSFADE
    // ======================================================
    private Coroutine CrossfadeToClip(AudioClip clip, float duration)
    {
        if (crossfadeRoutine != null)
            StopCoroutine(crossfadeRoutine);

        crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip, duration));
        return crossfadeRoutine;
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float fadeTime)
    {
        AudioSource active = useA ? sourceA : sourceB;
        AudioSource next = useA ? sourceB : sourceA;

        // --------------------------------------------------
        // 1. Fade OUT đoạn nhạc hiện tại
        // --------------------------------------------------
        float t = 0f;
        float startVolume = active.volume;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float p = t / fadeTime;
            active.volume = Mathf.Lerp(startVolume, 0f, p);

            // Nếu trong lúc fade-out mà Crossfade bị gọi lại → stop
            if (pendingState != currentState)
                yield break;

            yield return null;
        }

        active.volume = 0f;
        active.Stop();

        // --------------------------------------------------
        // 2. Đợi 2 giây TRƯỚC KHI PHÁT BÀI MỚI
        // Nếu trong lúc chờ có yêu cầu đổi nhạc → reset
        // --------------------------------------------------
        float waitTime = 0f;
        const float WAIT_DURATION = 3f;

        while (waitTime < WAIT_DURATION)
        {
            waitTime += Time.deltaTime;

            if (pendingState != currentState)
                yield break;

            yield return null;
        }

        // --------------------------------------------------
        // 3. Phát bài mới và fade IN
        // --------------------------------------------------
        next.clip = newClip;
        next.volume = 0f;
        next.Play();

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float p = t / fadeTime;

            next.volume = Mathf.Lerp(0f, 1f, p);

            // Nếu bị đổi nhạc trong lúc fade-in → stop
            if (pendingState != currentState)
                yield break;

            yield return null;
        }

        next.volume = 1f;

        // Hoán đổi active source
        useA = !useA;
    }


    // ======================================================
    // SFX
    // ======================================================
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayButtonSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip != null ? clip : defaultButtonSFX);
    }


    // ======================================================
    // MIXER VOLUME
    // ======================================================
    public void SetBGMVolume()
    {
        float v = Mathf.Clamp(bgmSlider.value, 0.001f, 1f);
        mixer.SetFloat("BGMVolume", Mathf.Log10(v) * 20f);
        PlayerPrefs.SetFloat("BGMVolume", v);
    }

    public void SetSFXVolume()
    {
        float v = Mathf.Clamp(sfxSlider.value, 0.001f, 1f);
        mixer.SetFloat("SFXVolume", Mathf.Log10(v) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", v);
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused)
        {
            StartCoroutine(RecoverAudioAfterResume());
        }
    }

    private IEnumerator RecoverAudioAfterResume()
    {
        // Đợi 0.1s để Unity khởi động lại audio engine
        yield return new WaitForSeconds(0.1f);

        // Force resume tất cả AudioSource
        sourceA.UnPause();
        sourceB.UnPause();
        sfxSource.UnPause();

        // Khôi phục volume mixer
        float bgm = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        mixer.SetFloat("BGMVolume", Mathf.Log10(bgm) * 20f);
        mixer.SetFloat("SFXVolume", Mathf.Log10(sfx) * 20f);

        // Nếu như active AudioSource đã stop → bật lại playlist hiện tại
        if (!sourceA.isPlaying && !sourceB.isPlaying)
        {
            ApplyState(currentState);
        }
    }

}
