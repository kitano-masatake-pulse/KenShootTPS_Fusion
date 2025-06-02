using Fusion;
using System;
using UnityEngine;

//�Q�[���̐������Ԃ��Ǘ�����
//TimerStart(),TimerStop()�Ń^�C�}�[�J�n�E��~
//�v���C���[��HUD(HUDManager)�Ɍ��ݎ��Ԃ�n�����Ƃ�
public class GameTimeManager : NetworkBehaviour
{
    // �c�莞�Ԃ�b�ŊǗ��i�b�P�ʂœ��������j
    [Networked(OnChanged = nameof(TimeChangedCallback))]
    public int RemainingSeconds { get; private set; }
    // ���ԕύX���̃C�x���g
    public static event Action<int> OnTimeChanged;
    // �����l�i3�� = 180 �b�j
    public�@static int initialTimeSec = 180;
    // �����p�^�C�}�[
    private float _accumDelta;
    //�^�C�}�[���������ǂ����̃t���O
    [Networked] public bool IsTimerRunning { get; private set; } = false;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // �z�X�g�����������l���Z�b�g
            RemainingSeconds = initialTimeSec;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsTimerRunning) return;

        // �l�b�g���[�N Tick ���Ƃ� delta ���Ԃ����Z
        _accumDelta += Runner.DeltaTime;

        // 1 �b�ȏソ�܂�����
        if (_accumDelta >= 1f)
        {
            _accumDelta -= 1f;
            RemainingSeconds = Mathf.Max(0, RemainingSeconds - 1);
        }

        // �c�莞�Ԃ� 0 �ɂȂ�����^�C�}�[���X�g�b�v
        if (RemainingSeconds == 0) TimerStop();
    }

    //�^�C�}�[���X�^�[�g
    public void TimerStart()
    {
        if (Object.HasStateAuthority)
            IsTimerRunning = true;
    }

    //�^�C�}�[���X�g�b�v
    public void TimerStop()
    {
        if (Object.HasStateAuthority)
            IsTimerRunning = false;
    }

    //�^�C�}�[�����Z�b�g
    public void TimerReset()
    {
        if (Object.HasStateAuthority)
        {
            RemainingSeconds = initialTimeSec;
            IsTimerRunning = false;
        }
    }

    // Networked �v���p�e�B�ύX���m�p static �R�[���o�b�N
    static void TimeChangedCallback(Changed<GameTimeManager> c)
    {
        c.Behaviour.RaiseTimeChanged();
    }

    // �C���X�^���X���ŃC�x���g�𔭉�
    private void RaiseTimeChanged()
    {
        OnTimeChanged?.Invoke(RemainingSeconds);
    }
}
