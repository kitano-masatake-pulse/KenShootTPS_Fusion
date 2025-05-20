using System;
using System.Collections;
using UnityEngine;

public class WeaponIconTester : MonoBehaviour
{
    //ただのテスト用スクリプト
    //WeaponIconSwitcher の動作確認用
    [Header("動作確認対象")]
    [SerializeField] private WeaponIconSwitcher iconSwitcher;

    [Header("自動切り替えインターバル（秒）")]
    [SerializeField] private float interval = 1f;

    private void Awake()
    {
        if (iconSwitcher == null)
            Debug.LogError("[WeaponIconTester] Inspector に WeaponIconSwitcher をセットしてください");
    }

    private void Update()
    {
        if (iconSwitcher == null) return;

        // キー１～４で手動切り替え
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                var type = (WeaponType)i;
                Debug.Log($"[WeaponIconTester] Key {(i + 1)} → {type}");
                iconSwitcher.SetWeaponIcon(type);
            }
        }

 
    }
}