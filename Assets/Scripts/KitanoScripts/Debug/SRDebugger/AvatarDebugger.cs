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
    [Category("�_�~�[����")]// �� �ϐ����J�e�S���ŃO���[�v��

    [DisplayName("������")]
    [NumberRange(0, 10)]
    public int DummyCount { get; set; } = 1;

    private GameObject spawner;

    BattleSceneSpawner Bspawner;


    [Category("�_�~�[����")]
    public void SpawnDummies()
    {

        Bspawner = GameObject.FindObjectOfType<BattleSceneSpawner>();
        GameLauncher.Instance.CreateDummyAvatars(DummyCount);
        
    }





    [Category("�����蔻��̉���")]
    [DisplayName("Hitbox")]
    public bool ShowHitbox { get; set; } = false;

    [Category("�����蔻��̉���")]
    [DisplayName("1�t���[���̍U������")]
    public bool ShowCollisionOneFrame { get; set; } = false;

    [Category("�����蔻��̉���")]
    [DisplayName("��������U������")]
    public bool ShowCollisionSustain { get; set; } = false;



    void FindSpawner()
    {
        GameLauncher.Instance.CreateDummyAvatars(DummyCount);

    }
}
