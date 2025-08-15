using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFollower : MonoBehaviour
{
    [SerializeField] private Transform target = null; // 追従するターゲットのTransform

    public void SetTarget(Transform newTarget)
    {
        target = newTarget; // ターゲットを設定
    }
    public void ClearTarget()
    {
        target = null; // ターゲットをクリア
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return; // ターゲットが設定されていない場合は何もしない
        this.transform.position = target.position; // ターゲットの位置に追従
        this.transform.rotation = target.rotation; // ターゲットの回転に追従   
    }
}
