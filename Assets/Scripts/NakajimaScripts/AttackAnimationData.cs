using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AttackAnimationData", menuName = "Animation Speed Data")]
public class AttackAnimationData : ScriptableObject
{
    [Header("アニメーションクリップ")]
    public AnimationClip attackClip;

    [Header("演出全体の長さ（秒）")]
    public float totalDuration = 1.0f;

    [Header("ヒット判定の開始時刻（秒）※近接用")]
    public float hitStartTime = 0.3f;

    [Header("ヒット判定の終了時刻（秒）※近接用")]
    public float hitEndTime = 0.5f;
}