using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

public enum SoundCategory {System, Action, Weapon, BGM }
public enum SoundType { OneShot, Loop }

public struct SoundHandle
{
    public int id;
    public string clipKey;
}

[RequireComponent(typeof(AudioMixer))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    struct CategoryInfo { public SoundCategory category; public AudioMixerGroup mixerGroup; }
    [SerializeField] CategoryInfo[] categories;
    Dictionary<SoundCategory, AudioMixerGroup> _groupMap;

    [Header("初期プールサイズ")]
    [SerializeField] int poolSize = 30;

    Queue<AudioSource> availableSources;
    HashSet<AudioSource> inUseSources;

    // 再生中の OneShot 用ソースを id で管理
    Dictionary<int, AudioSource> _playingSounds = new Dictionary<int, AudioSource>();
    int _nextSoundId = 1;

    // Clip cache
    Dictionary<string, AudioClip> _systemClips;
    Dictionary<string, AudioClip> _bgmClips;
    Dictionary<string, Dictionary<string, AudioClip>> _sceneClips;

    // BGM source
    AudioSource _bgmSource;

    //AudioMixer
    [SerializeField] 
    private AudioMixer audioMixer;


    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this;
            this.transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }

        // Category map
        _groupMap = new Dictionary<SoundCategory, AudioMixerGroup>();
        foreach (var info in categories) _groupMap[info.category] = info.mixerGroup;

        // Load UI and BGM at startup
        _systemClips = new Dictionary<string, AudioClip>();
        foreach (var clip in Resources.LoadAll<AudioClip>("Audio/System"))
        {
            _systemClips[clip.name] = clip;
        }
        _bgmClips = new Dictionary<string, AudioClip>();
        foreach (var clip in Resources.LoadAll<AudioClip>("Audio/BGM"))
        {
            _bgmClips[clip.name] = clip;
        }

        // One-shot pool
        availableSources = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SfxSrc_{i}");
            go.transform.parent = transform;
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            availableSources.Enqueue(src);
        }
        inUseSources = new HashSet<AudioSource>();
        _sceneClips = new Dictionary<string, Dictionary<string, AudioClip>>();

        // BGM streaming source
        var bgmGo = new GameObject("BgmSource");
        bgmGo.transform.parent = transform;
        _bgmSource = bgmGo.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f;
        _bgmSource.outputAudioMixerGroup = _groupMap[SoundCategory.BGM];
    }

    void OnEnable()
    {
        SceneTransitionManager.OnSceneLoad -= OnSceneLoaded;
        SceneTransitionManager.OnSceneUnload -= OnSceneUnloaded;
        SceneTransitionManager.OnSceneLoad += OnSceneLoaded;
        SceneTransitionManager.OnSceneUnload += OnSceneUnloaded;
        OptionsManager.OnApplied += ApplyOptions;
    }
    void OnDisable()
    {
        SceneTransitionManager.OnSceneLoad -= OnSceneLoaded;
        SceneTransitionManager.OnSceneUnload  -= OnSceneUnloaded;
        OptionsManager.OnApplied -= ApplyOptions;
    }
    void OnSceneLoaded(SceneType scene)
    {
        var sceneName = scene.ToSceneName();
        var clips = Resources.LoadAll<AudioClip>($"Audio/SE_{sceneName}");
        var dict = new Dictionary<string, AudioClip>();
        foreach (var clip in clips) dict[clip.name] = clip;
        _sceneClips[sceneName] = dict;
    }

    void OnSceneUnloaded(SceneType scene)
    {
        //フェードしながら消えるのが望ましいが、ここでは即座に停止
        StopAll();

        //プールサイズを元に戻す
        while(availableSources.Count > poolSize)
        {
            var src = availableSources.Dequeue();
            if (src != null)
            {
                Destroy(src.gameObject);
            }
        }

        if (_sceneClips.TryGetValue(scene.ToSceneName(), out var dict))
        {
            foreach (var clip in dict.Values) Resources.UnloadAsset(clip);
            _sceneClips.Remove(scene.ToSceneName());
        }

    }
    #region AudioSource Management
    AudioSource GetFreeSource()
    {
        if(availableSources.Count > 0)
        {

            var src = availableSources.Dequeue();
            inUseSources.Add(src);
            return src;
        }
        //プールが空ならWarningを出して新しいAudioSourceを作成
        Debug.LogWarning("AudioManager: No free AudioSource available, creating a new one.");
        var fallback = CreateNewSource();
        inUseSources.Add(fallback);
        return fallback;
    }

    AudioSource CreateNewSource()
    {
        var go = new GameObject($"SfxSrc_{_nextSoundId}");
        go.transform.parent = transform;
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }
    //AudioSourceを初期化
    void InitializeSource(AudioSource src)
    {
        src.clip = null;
        src.loop = false;
        src.playOnAwake = false;
        src.spatialBlend = 0f; 
        src.volume = 1f; 
        src.pitch = 1f; 
        src.panStereo = 0f; 
        src.dopplerLevel = 0f; 
        src.transform.position = Vector3.zero;
    }

    void ReturnSource(AudioSource src)
    {
        if (inUseSources.Remove(src))
        {
            availableSources.Enqueue(src);
        }
    }

    //再生し終わった音を再生中辞書から削除
    IEnumerator CleanupAfterPlay(int id, AudioSource src, float duration)
    {
        yield return new WaitForSeconds(duration);
        //再生が終わったらマップから除去、既に停止している場合は何もしない
        if (_playingSounds.ContainsKey(id)) { 
            _playingSounds.Remove(id);
            ReturnSource(src);
        }
    }

    #endregion

    #region AudioClip Management

    //読み込まれたAudioClipの中からkeyに対応するものを取得する
    AudioClip GetClip(string key)
    {
        if (_systemClips != null && _systemClips.TryGetValue(key, out var clip)) return clip;
        if (_bgmClips != null && _bgmClips.TryGetValue(key, out clip)) return clip;
        var current = SceneTransitionManager.Instance.CurrentSceneType.ToSceneName();
        if (_sceneClips.TryGetValue(current, out var dict) && dict.TryGetValue(key, out clip)) return clip;
        Debug.LogError($"AudioClip '{key}' not found.");
        return null;
    }
    #endregion


    #region Sound Play Methods
    public SoundHandle PlaySound(string clipKey, SoundCategory category, float startTime = 0f,float soundVolume = 1.0f, SoundType type = SoundType.OneShot,
                          Vector3? pos = null, Transform followTarget = null)
    {
        var clip = GetClip(clipKey);
        if (clip == null) return default;
        var group = _groupMap[category];
        var src = GetFreeSource();
        InitializeSource(src);
        if (type == SoundType.OneShot)
        {
            
            if (pos.HasValue)
            {
                src.transform.position = pos.Value;
                src.spatialBlend = 1f;
            }
            else if (followTarget != null)
            {
                src.transform.position = followTarget.position;
                src.spatialBlend = 1f;
            }
            else
            {
                src.spatialBlend = 0f;
            }
            src.clip = clip;
            src.outputAudioMixerGroup = group;
            src.time = startTime; // 再生開始位置を設定
            src.volume = soundVolume; // 音量を設定
            int id = _nextSoundId++;
            _playingSounds[id] = src;
            StartCoroutine(CleanupAfterPlay(id, src, clip.length));

            src.Play();
            return new SoundHandle { id = id, clipKey = clipKey };
        }
        else 
        {
            if (pos.HasValue)
            {
                src.transform.position = pos.Value;
                src.spatialBlend = 1f;
            }
            else if (followTarget != null)
            {
                src.transform.position = followTarget.position;
                src.spatialBlend = 1f;
            } 
            else
            {
                src.spatialBlend = 0f;
            }
            src.clip = clip;
            src.loop = true;
            src.time = startTime; // 再生開始位置を設定
            src.volume = soundVolume; // 音量を設定
            src.outputAudioMixerGroup = group;
            int id = _nextSoundId++;
            _playingSounds[id] = src;

            src.Play();
            return new SoundHandle { id = id, clipKey = clipKey };
        }  
    }



    // 指定されたハンドルのサウンドを停止
    public void StopSound(SoundHandle handle)
    {
        if(_playingSounds.ContainsKey(handle.id))
        {
            var playingSrc = _playingSounds[handle.id];
            if (playingSrc.isPlaying) playingSrc.Stop();
            _playingSounds.Remove(handle.id);
            ReturnSource(playingSrc);
        }

    }
    //指定された名前のサウンドを全て停止
    public void StopSound(string clipKey)
    {
        var keys = _playingSounds
            .Where(kvp => kvp.Value.clip != null && kvp.Value.clip.name == clipKey)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in keys)
        {
            var src = _playingSounds[key];
            if (src.isPlaying) src.Stop();
            _playingSounds.Remove(key);
            ReturnSource(src);
        }
    }

    /// <summary>
    /// BGM用ストリーミング再生
    /// </summary>
    public void PlayBgm(string clipKey,SoundCategory soundCategory = SoundCategory.BGM,SoundType soundType = SoundType.Loop)
    {
        var clip = GetClip(clipKey);
        if (clip == null) return;
        _bgmSource.clip = clip;
        _bgmSource.outputAudioMixerGroup = _groupMap[soundCategory];
        _bgmSource.loop = (soundType == SoundType.Loop);
        _bgmSource.Play();
    }

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBgm()
    {
        if (_bgmSource == null) return;
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void StopAll()
    {
        var handles = _playingSounds.Keys.ToList();
        foreach (var id in handles)
        {
            StopSound(new SoundHandle { id = id });
        }
        _playingSounds.Clear();
        StopBgm();
    }

    //再生中のサウンドの音量(AudioSource)を変更
    public void SetSoundVolume(SoundHandle handle, float volume)
    {
        if (_playingSounds.TryGetValue(handle.id, out var src))
        {
            src.volume = volume;
        }
    }

    //SetSoundVolumeを使って音量フェード
    public void FadeSound(SoundHandle handle, float targetVolume, float duration)
    {
        if (_playingSounds.TryGetValue(handle.id, out var src))
        {
            StartCoroutine(FadeCoroutine(src, targetVolume, duration));
        }
    }

    IEnumerator FadeCoroutine(AudioSource src, float targetVolume, float duration)
    {
        float startVolume = src.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        src.volume = targetVolume; // 最終的な音量を設定
    }
    #endregion
    #region AudioMixer Control

    private void ApplyOptions(OptionData data)
    {
        //音量をAudioMixerに適用
        SetMixerVolume("Master", data.masterVolume);
        SetMixerVolume("System", data.systemVolume);
        SetMixerVolume("BGM", data.bgmVolume);
        SetMixerVolume("Weapon", data.weaponVolume);
        SetMixerVolume("Action", data.actionVolume);
    }

    private void SetMixerVolume(string param,float volume )
    {
        float dB = (volume <= 0f) ? -80f
                   : Mathf.Log10(Mathf.Clamp(volume, 0f, 1f)) * 20f;
        audioMixer.SetFloat(param, dB);
    }
    
    #endregion
}

