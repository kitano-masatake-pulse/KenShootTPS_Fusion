using SRDebugger;
using SRF;
using System.ComponentModel;
using UnityEngine;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;


public partial class SROptions 
{
    [Category("ダミー生成")]// ← 変数をカテゴリでグループ化

    [DisplayName("生成数")]
    [NumberRange(0, 10)]
    public int DummyCount { get; set; } = 1;

    private GameObject spawner;

    BattleSceneSpawner Bspawner;


    [Category("ダミー生成")]
    public void SpawnDummies()
    {

        Bspawner = GameObject.FindObjectOfType<BattleSceneSpawner>();
        GameLauncher.Instance.CreateDummyAvatars(DummyCount);
        
    }





    [Category("当たり判定の可視化")]
    [DisplayName("Hitbox")]
    public bool isShowingHitbox { get; set; } =true;

    [Category("当たり判定の可視化")]
    [DisplayName("攻撃側OverlapSphere")]
    public bool isShowingAttackOverlapSphere { get; set; } = true;

    [Category("当たり判定の可視化")]
    [DisplayName("攻撃側Raycast")]
    public bool isShowingAttackRaycast { get; set; } = true;



    void FindSpawner()
    {
        GameLauncher.Instance.CreateDummyAvatars(DummyCount);

    }
}
