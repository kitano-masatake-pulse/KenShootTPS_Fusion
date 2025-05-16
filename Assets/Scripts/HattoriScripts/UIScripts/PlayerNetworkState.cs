using Fusion;

public class PlayerNetworkState : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    [Networked] public int Ammo { get; set; }
    [Networked] public int Score { get; set; }

    // HUDManagerに渡す用プロパティ(現在HPの％を返す)
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    
    // ChangeイベントでUI更新を呼び出す
    static void OnHPChanged(Changed<PlayerNetworkState> changed)
    {
        changed.Behaviour.OnNetworkedHPChanged();
    }
    // インスタンスメソッドで、登録されたUIに通知
    void OnNetworkedHPChanged()
    {
        var hud = GetComponentInChildren<PlayerHUDController>();
        if (hud != null)
            hud.UpdateHPBar(HpNormalized);
    }
}
