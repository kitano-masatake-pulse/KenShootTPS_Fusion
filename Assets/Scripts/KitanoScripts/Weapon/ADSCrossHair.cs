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
        // �C�x���g�o�^
        weaponState.OnADSChanged -= UpdateADSState;
        weaponState.OnADSChanged += UpdateADSState;

        // �����l�ݒ�
        UpdateADSState(false);// ������Ԃł�ADS�N���X�w�A�͔�\��
    }
    public void Cleanup()
    {
        weaponState.OnADSChanged -= UpdateADSState;
    }

    void UpdateADSState(bool isADS)
    {
        // ADS��Ԃɉ����ăN���X�w�A�̕\����؂�ւ���
        ADSCrossHairImage.enabled = isADS;


    }
}
