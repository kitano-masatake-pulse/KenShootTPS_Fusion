using UnityEngine;

/// <summary>
/// 2D Freeform Directional��BlendTree�X�e�[�g�ɃA�^�b�`���āA
/// ����(Horizontal, Vertical)�x�N�g���̑傫����臒l�ȏ�̂Ƃ�����������炷�B
/// �Đ���AudioManager�o�R�i�N���b�v�L�[�w��j�B
/// </summary>
public class FootstepOnBlend2D_UsingAudioManager : StateMachineBehaviour
{
    [Header("Animator Parameters")]
    public string horizontalParam = "Horizontal";
    public string verticalParam   = "Vertical";

    [Header("Footstep Triggering")]
    [Tooltip("||(Horizontal, Vertical)|| �����̒l�ȏ�Ŕ����J�n")]
    public float playThreshold = 0.15f;
    [Tooltip("�q�X�e���V�X�F���̒l�����܂ŉ�����ƒ�~�iplayThreshold��菬���ߐ����j")]
    public float stopThreshold = 0.10f;

    [Tooltip("���x�������Ԋu�i�b�j�}�b�s���O�̍ő呤�i������莞�j")]
    public float maxInterval = 0.55f;
    [Tooltip("���x�������Ԋu�i�b�j�}�b�s���O�̍ŏ����i�������j")]
    public float minInterval = 0.25f;
    [Tooltip("���x�̐��K������imag�����̒l��Clamp01���ĊԊu�ɔ��f�j")]
    public float normalizeMaxMagnitude = 1.0f;

    [Header("AudioManager Settings")]
    [Tooltip("�Đ�������ʉ��̃N���b�v�L�[�iAudioManager���ǂݍ���Resources���j���烉���_���I��")]
    public string[] footstepClipKeys;
    public SoundCategory category = SoundCategory.Action;

    [Tooltip("�������L�����N�^�[�ʒu����炷�itrue: Animator��Transform��follow�Afalse: 2D=��3D���j")]
    public bool spatialFollowCharacter = true;


    [Tooltip("�x�[�X���ʁiOneShot�����SetSoundVolume�œK�p�j")]
    [Range(0f, 1f)] public float baseVolume = 1f;
    [Tooltip("���ʂ̃����_�����i�}�j")]
    [Range(0f, 1f)] public float volumeJitter = 0.1f;

    // ����
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
        if (AudioManager.Instance == null) return; // �Ȃ��Ă������Ȃ��悤��

        float x = animator.GetFloat(_hId);
        float y = animator.GetFloat(_vId);
        float mag = new Vector2(x, y).magnitude;

        // �q�X�e���V�X
        if (!_active && mag >= playThreshold) _active = true;
        else if (_active && mag <= stopThreshold) _active = false;

        if (!_active)
        {
            _timer = 0f;
            return;
        }

        // ���x���Ԋu
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

        // �����_���ɃN���b�v�L�[�I��
        string key = footstepClipKeys[Random.Range(0, footstepClipKeys.Length)];

        // �ʒu/�Ǐ]�w��F�L�����ʒu����3D�Ŗ炷���A��3D(0)�Ŗ炷��
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
                followTarget: animator.transform // �L�����ɒǏ]
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
                followTarget: null // 2D��
            );
        }

        //// �����V���b�g����ɉ��ʂ��������i�s�b�`��AudioManager API�ɂȂ����ߖ��Ή��j
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
