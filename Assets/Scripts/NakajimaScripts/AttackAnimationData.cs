using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AttackAnimationData", menuName = "Animation Speed Data")]
public class AttackAnimationData : ScriptableObject
{
    [Header("�A�j���[�V�����N���b�v")]
    public AnimationClip attackClip;

    [Header("���o�S�̂̒����i�b�j")]
    public float totalDuration = 1.0f;

    [Header("�q�b�g����̊J�n�����i�b�j���ߐڗp")]
    public float hitStartTime = 0.3f;

    [Header("�q�b�g����̏I�������i�b�j���ߐڗp")]
    public float hitEndTime = 0.5f;
}