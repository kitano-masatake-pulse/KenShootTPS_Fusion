using UnityEngine;

/// <summary>
/// 2D Freeform DirectionalのBlendTreeステートにアタッチして、
/// 入力(Horizontal, Vertical)ベクトルの大きさが閾値以上のときだけ足音を鳴らす。
/// 再生はAudioManager経由（クリップキー指定）。
/// </summary>
public class FootstepOnBlend2D_UsingAudioManager : StateMachineBehaviour
{
    [Header("Animator Parameters")]
    public string horizontalParam = "Horizontal";
    public string verticalParam   = "Vertical";

    [Header("Footstep Triggering")]
    [Tooltip("||(Horizontal, Vertical)|| がこの値以上で発音開始")]
    public float playThreshold = 0.15f;
    [Tooltip("ヒステリシス：この値未満まで下がると停止（playThresholdより小さめ推奨）")]
    public float stopThreshold = 0.10f;

    [Tooltip("速度→足音間隔（秒）マッピングの最大側（ゆっくり時）")]
    public float maxInterval = 0.55f;
    [Tooltip("速度→足音間隔（秒）マッピングの最小側（速い時）")]
    public float minInterval = 0.25f;
    [Tooltip("速度の正規化上限（magをこの値でClamp01して間隔に反映）")]
    public float normalizeMaxMagnitude = 1.0f;

    [Header("AudioManager Settings")]
    [Tooltip("再生する効果音のクリップキー（AudioManagerが読み込むResources名）からランダム選択")]
    public string[] footstepClipKeys;
    public SoundCategory category = SoundCategory.Action;

    [Tooltip("足音をキャラクター位置から鳴らす（true: AnimatorのTransformをfollow、false: 2D=非3D音）")]
    public bool spatialFollowCharacter = true;


    [Tooltip("ベース音量（OneShot直後にSetSoundVolumeで適用）")]
    [Range(0f, 1f)] public float baseVolume = 1f;
    [Tooltip("音量のランダム幅（±）")]
    [Range(0f, 1f)] public float volumeJitter = 0.1f;

    // 内部
    int _hId, _vId;
    bool _active;
    float _timer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hId = Animator.StringToHash(horizontalParam);
        _vId = Animator.StringToHash(verticalParam);
        _active = false;
        _timer = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.Instance == null) return; // なくても落ちないように

        float x = animator.GetFloat(_hId);
        float y = animator.GetFloat(_vId);
        float mag = new Vector2(x, y).magnitude;

        // ヒステリシス
        if (!_active && mag >= playThreshold) _active = true;
        else if (_active && mag <= stopThreshold) _active = false;

        if (!_active)
        {
            _timer = 0f;
            return;
        }

        // 速度→間隔
        float m = Mathf.Clamp01(normalizeMaxMagnitude <= 0f ? mag : (mag / normalizeMaxMagnitude));
        float interval = Mathf.Lerp(maxInterval, minInterval, m);

        _timer += Time.deltaTime * animator.speed * stateInfo.speed;
        if (_timer >= interval)
        {
            _timer = 0f;
            PlayFootstep(animator);
        }
    }

    void PlayFootstep(Animator animator)
    {
        if (footstepClipKeys == null || footstepClipKeys.Length == 0) return;

        // ランダムにクリップキー選択
        string key = footstepClipKeys[Random.Range(0, footstepClipKeys.Length)];

        // 位置/追従指定：キャラ位置から3Dで鳴らすか、非3D(0)で鳴らすか
        SoundHandle handle;
        if (spatialFollowCharacter)
        {
            handle = AudioManager.Instance.PlaySound(
                clipKey: key,
                category: category,
                startTime: 0f,
                soundVolume: baseVolume,
                type: SoundType.OneShot,
                pos: animator.transform.position,
                followTarget: animator.transform // キャラに追従
            );
        }
        else
        {
            handle = AudioManager.Instance.PlaySound(
                clipKey: key,
                category: category,
                startTime: 0f,
                soundVolume: baseVolume,
                type: SoundType.OneShot,
                pos: null,
                followTarget: null // 2D音
            );
        }

        //// ワンショット直後に音量だけ調整（ピッチはAudioManager APIにないため未対応）
        //if (handle.id != 0)
        //{
        //    float vol = Mathf.Clamp01(baseVolume * (1f + Random.Range(-volumeJitter, volumeJitter)));
        //    AudioManager.Instance.SetSoundVolume(handle, vol);
        //}
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _active = false;
        _timer = 0f;
    }
}
