//HUD�̊e�p�l����HUDManager���ꗥ�Ɉ������߂̃C���^�[�t�F�[�X
public interface IHUDPanel
{
    void Initialize(PlayerNetworkState pState, WeaponLocalState wState);
    void Cleanup();
}
