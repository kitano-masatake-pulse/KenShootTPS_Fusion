using UnityEngine;
using System.Collections;

/// <summary>
/// Animator のステートに入ったら AudioManager で音を鳴らし、
/// ループタイプならステートを抜けるときに停止する
/// </summary>
public class PlaySoundOnStateEnter : StateMachineBehaviour
{
    [Header("必須")]
    [Tooltip("鳴らしたい音声クリップのキー（拡張子なし）")]
    public string clipKey;

    [Tooltip("音のカテゴリ（System/Action/Weapon/BGM）")]
    public SoundCategory category = SoundCategory.Action;

    [Header("任意")]

    [Header("再生設定")]
    [Tooltip("ステートに入ってから何秒後に再生するか")]
    public float delay = 0f;

    [Tooltip("再生開始位置（秒）。デフォルト 0s")]
    public float startTime = 0f;

    [Tooltip("再生タイプ。一度だけ鳴らすかループさせるか")]
    public SoundType type = SoundType.OneShot;

    [Tooltip("3D 空間で鳴らす場合はチェック")]
    public bool useSpatial = false;

    [Tooltip("ループ音をオブジェクトに追従させる場合はチェック")]
    public bool useFollow = false;

    [Tooltip("音量(0～1)")]
    public float normalizedVolume = 1f;

    // ループ再生時に保持するハンドル
    private SoundHandle _loopHandle;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


        AudioManager.Instance.StartCoroutine(DelayedPlay(animator));


        

    }



    private IEnumerator DelayedPlay(Animator animator)
    {
        // 指定秒だけ待機
        if (delay > 0f)
        { yield return new WaitForSeconds(delay); }

        // 位置 or 追従ターゲットを設定
        Vector3? pos = useSpatial ? (Vector3?)animator.transform.position : null;
        Transform follow = useFollow ? animator.transform : null;

        // PlaySound を呼び出し
        _loopHandle = AudioManager.Instance.PlaySound(
            clipKey,
            category,
            startTime,
            type,
            pos,
            follow
        );

        AudioManager.Instance.SetSoundVolume(_loopHandle, normalizedVolume);



    }




    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ループ再生だったら停止
        if (type == SoundType.Loop)
        {
            AudioManager.Instance.StopSound(_loopHandle);
        }
    }
}
