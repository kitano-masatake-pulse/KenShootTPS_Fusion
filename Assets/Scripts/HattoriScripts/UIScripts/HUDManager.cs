using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text scoreText;

    private PlayerNetworkState localPlayer;

    void Start()
    {
        // 自分のプレイヤーを取得（Spawned側でセットしても良い）
        localPlayer = FindObjectsOfType<PlayerNetworkState>().FirstOrDefault(p => p.HasInputAuthority);
    }

    void Update()
    {
        if (localPlayer != null)
        {
            hpSlider.value = localPlayer.HpNormalized;
            ammoText.text = $"Ammo: {localPlayer.Ammo}";
            scoreText.text = $"Score: {localPlayer.Score}";
        }
    }
}
