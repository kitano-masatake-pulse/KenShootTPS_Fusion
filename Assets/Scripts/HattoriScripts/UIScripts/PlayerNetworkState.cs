using System;
using Fusion;

//�e�v���C���[�̃X�e�[�^�X���Ǘ�����N���X
public class PlayerNetworkState : NetworkBehaviour
{
    // �C�x���g���s����
    public event Action<float> OnHPChanged;
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int> OnAmmoChanged;   // (totalAmmo, magazineAmmo)
    public event Action<int, int> OnScoreChanged;
    // ���[�J���v���C���[�������ɒʒm����ÓI�C�x���g
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;

    // �������� HP(����/�ő�)����������������������������������������
    [Networked(OnChanged = nameof(HPChangedCallback))] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    static void HPChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseHPChanged();
    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);

    // �������� �������̕��� ������������������������������������������
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;
    static void WeaponChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseWeaponChanged();
    void RaiseWeaponChanged() => OnWeaponChanged?.Invoke(CurrentWeapon);

    // �������� �e��i���e���^�}�K�W���e���j������
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int TotalAmmo { get; set; }
    static void AmmoChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseAmmoChanged(); 
    void RaiseAmmoChanged() => OnAmmoChanged?.Invoke(TotalAmmo, MagazineAmmo);

    // �������� �X�R�A(�L�����ƃf�X��) ����������������������������������������������������
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; set; }
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; set; }
    static void ScoreChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseScoreChanged();
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(KillScore, DeathScore);

    //�������ɁA�����̂��̂ł���Ή��HUD�ɓo�^����
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            // �����̃L�������������ꂽ�甭��
            if (HasInputAuthority)
            {
                OnLocalPlayerSpawned?.Invoke(this);
            }
        }
    }


}
