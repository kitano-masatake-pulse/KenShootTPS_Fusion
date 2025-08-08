using UnityEngine;

/// <summary>
/// Animator �̃X�e�[�g�ɓ������� AudioManager �ŉ���炵�A
/// ���[�v�^�C�v�Ȃ�X�e�[�g�𔲂���Ƃ��ɒ�~����
/// </summary>
public class PlaySoundOnStateEnter : StateMachineBehaviour
{
    [Header("�K�{")]
    [Tooltip("�炵���������N���b�v�̃L�[�i�g���q�Ȃ��j")]
    public string clipKey;

    [Tooltip("���̃J�e�S���iSystem/Action/Weapon/BGM�j")]
    public SoundCategory category = SoundCategory.Action;

    [Header("�C��")]
    [Tooltip("�Đ��J�n�ʒu�i�b�j�B�f�t�H���g 0s")]
    public float startTime = 0f;

    [Tooltip("����(%)�B�f�t�H���g 1(100%)")]
    public float volume = 1f;   

    [Tooltip("�Đ��^�C�v�B��x�����炷�����[�v�����邩")]
    public SoundType type = SoundType.OneShot;

    [Tooltip("3D ��ԂŖ炷�ꍇ�̓`�F�b�N")]
    public bool useSpatial = false;

    [Tooltip("���[�v�����I�u�W�F�N�g�ɒǏ]������ꍇ�̓`�F�b�N")]
    public bool useFollow = false;

    [Tooltip("����(0�`1)")]
    public float normalizedVolume = 1f;

    // ���[�v�Đ����ɕێ�����n���h��
    private SoundHandle _loopHandle;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �ʒu or �Ǐ]�^�[�Q�b�g��ݒ�
        Vector3? pos = useSpatial ? (Vector3?)animator.transform.position : null;
        Transform follow = useFollow ? animator.transform : null;

        // PlaySound ���Ăяo��
        _loopHandle = AudioManager.Instance.PlaySound(
            clipKey,
            category,
            startTime,
            volume,
            type,
            pos,
            follow
        );

        AudioManager.Instance.SetSoundVolume(_loopHandle, normalizedVolume);




    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ���[�v�Đ����������~
        if (type == SoundType.Loop)
        {
            AudioManager.Instance.StopSound(_loopHandle);
        }
    }
}
