using Fusion;
using System;
using UnityEngine;

//ゲームの制限時間を管理する
//TimerStart(),TimerStop()でタイマー開始・停止
//プレイヤーのHUD(HUDManager)に現在時間を渡すことも
public class GameTimeManager : NetworkBehaviour
{
    // 残り時間を秒で管理（秒単位で同期する例）
    [Networked(OnChanged = nameof(TimeChangedCallback))]
    public int RemainingSeconds { get; private set; }
    // 時間変更時のイベント
    public static event Action<int> OnTimeChanged;
    // 初期値（3分 = 180 秒）
    public　static int initialTimeSec = 180;
    // 内部用タイマー
    private float _accumDelta;
    //タイマーが動くかどうかのフラグ
    [Networked] public bool IsTimerRunning { get; private set; } = false;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            // ホストだけが初期値をセット
            RemainingSeconds = initialTimeSec;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsTimerRunning) return;

        // ネットワーク Tick ごとの delta 時間を加算
        _accumDelta += Runner.DeltaTime;

        // 1 秒以上たまったら
        if (_accumDelta >= 1f)
        {
            _accumDelta -= 1f;
            RemainingSeconds = Mathf.Max(0, RemainingSeconds - 1);
        }

        // 残り時間が 0 になったらタイマーをストップ
        if (RemainingSeconds == 0) TimerStop();
    }

    //タイマーをスタート
    public void TimerStart()
    {
        if (Object.HasStateAuthority)
            IsTimerRunning = true;
    }

    //タイマーをストップ
    public void TimerStop()
    {
        if (Object.HasStateAuthority)
            IsTimerRunning = false;
    }

    //タイマーをリセット
    public void TimerReset()
    {
        if (Object.HasStateAuthority)
        {
            RemainingSeconds = initialTimeSec;
            IsTimerRunning = false;
        }
    }

    // Networked プロパティ変更検知用 static コールバック
    static void TimeChangedCallback(Changed<GameTimeManager> c)
    {
        c.Behaviour.RaiseTimeChanged();
    }

    // インスタンス側でイベントを発火
    private void RaiseTimeChanged()
    {
        OnTimeChanged?.Invoke(RemainingSeconds);
    }
}
