using System;
using System.Collections;
using UnityEngine;

public class WeaponIconTester : MonoBehaviour
{
    //�����̃e�X�g�p�X�N���v�g
    //WeaponIconSwitcher �̓���m�F�p
    [Header("����m�F�Ώ�")]
    [SerializeField] private WeaponIconSwitcher iconSwitcher;

    [Header("�����؂�ւ��C���^�[�o���i�b�j")]
    [SerializeField] private float interval = 1f;

    private void Awake()
    {
        if (iconSwitcher == null)
            Debug.LogError("[WeaponIconTester] Inspector �� WeaponIconSwitcher ���Z�b�g���Ă�������");
    }

    private void Update()
    {
        if (iconSwitcher == null) return;

        // �L�[�P�`�S�Ŏ蓮�؂�ւ�
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                var type = (WeaponType)i;
                Debug.Log($"[WeaponIconTester] Key {(i + 1)} �� {type}");
                iconSwitcher.SetWeaponIcon(type);
            }
        }

 
    }
}