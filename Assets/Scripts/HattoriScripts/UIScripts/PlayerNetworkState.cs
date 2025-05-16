using Fusion;

public class PlayerNetworkState : NetworkBehaviour
{
    [Networked] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    [Networked] public int Ammo { get; set; }
    [Networked] public int Score { get; set; }

    // HUDManager�ɓn���p�v���p�e�B(����HP�́���Ԃ�)
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
}
