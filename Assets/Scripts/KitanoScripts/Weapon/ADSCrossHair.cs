using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ADSCrossHair : MonoBehaviour, IHUDPanel
{

    private PlayerAvatar weaponState;

    Image ADSCrossHairImage;
    // Start is called before the first frame update
    void Start()
    {
        ADSCrossHairImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(PlayerNetworkState _ , PlayerAvatar wState)
    {
        weaponState = wState;
        // イベント登録
        weaponState.OnADSChanged -= UpdateADSState;
        weaponState.OnADSChanged += UpdateADSState;

        // 初期値設定
        UpdateADSState(false);// 初期状態ではADSクロスヘアは非表示
    }
    public void Cleanup()
    {
        weaponState.OnADSChanged -= UpdateADSState;
    }

    void UpdateADSState(bool isADS)
    {
        // ADS状態に応じてクロスヘアの表示を切り替える
        ADSCrossHairImage.enabled = isADS;


    }
}
