
using UnityEngine;

public class WeaponIdleSync : MonoBehaviour
{
    public Animator animator;

    // レイヤーのインデックス
    private int upperLayer = 1; // 上半身
    private int lowerLayer = 0; // 下半身

    private int BlendTreeTagHash = Animator.StringToHash("BlendTree"); // 上半身BlendTree名（正確に）

    private bool hasEnteredIdleState = false;




    void Update()
    {
        //もし、リロードや武器切り替えなどの特定のアニメーションが終わったら、上下半身のステイトを0にする
        //やること。毎回このステイト遷移した1回目だけ実行する処理
        //アニメーター側でタグを変更。IKでタグを使えなくなるので、グレネードのように関数で分ける（アニメーションハンドラー）
        bool isBlendTreeState = (animator.GetCurrentAnimatorStateInfo(0).tagHash == animator.GetCurrentAnimatorStateInfo(1).tagHash) && animator.GetCurrentAnimatorStateInfo(1).tagHash != 0;
        bool isTest = (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f) - (animator.GetCurrentAnimatorStateInfo(1).normalizedTime % 1f) < 0.001;
        if (isBlendTreeState && !isTest)
        {
            ResetLayerState(upperLayer);
            ResetLayerState(lowerLayer);
        }
        //if (isBlendTreeState && !hasEnteredIdleState)
        //{
        //    Debug.Log("上半身・下半身の再生位置をリセット");

        //    ResetLayerState(upperLayer);
        //    ResetLayerState(lowerLayer);
        //    hasEnteredIdleState=true;
        //}
        //if (!isBlendTreeState && hasEnteredIdleState)
        //{
        //    hasEnteredIdleState = false;
        //}
    }
    void ResetLayerState(int layerIndex)
    {
        // 現在のステート情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // 同じステートに再度遷移（normalizedTime = 0f）
        animator.Play(stateInfo.fullPathHash, layerIndex, 0f);
    }


}
