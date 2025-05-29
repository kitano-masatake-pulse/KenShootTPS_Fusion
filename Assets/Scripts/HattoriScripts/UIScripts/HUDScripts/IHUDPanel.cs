//HUDの各パネルをHUDManagerが一律に扱うためのインターフェース
public interface IHUDPanel
{
    void Initialize(PlayerNetworkState pState, WeaponLocalState wState);
    void Cleanup();
}
