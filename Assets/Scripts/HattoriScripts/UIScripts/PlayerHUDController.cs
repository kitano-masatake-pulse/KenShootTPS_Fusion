using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private PlayerNetworkState player;

    // Update is called once per frame
    void Update()
    {
        hpSlider.value = player.HpNormalized;
    }
}
