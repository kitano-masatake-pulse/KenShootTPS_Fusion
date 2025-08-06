
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
        bool isBlendTreeState = animator.GetCurrentAnimatorStateInfo(1).tagHash == BlendTreeTagHash;
        if (isBlendTreeState && !hasEnteredIdleState)
        {
            Debug.Log("上半身・下半身の再生位置をリセット");

            ResetLayerState(upperLayer);
            ResetLayerState(lowerLayer);
            hasEnteredIdleState=true;
        }
        if (!isBlendTreeState && hasEnteredIdleState)
        {
            hasEnteredIdleState = false;
        }
    }
    void ResetLayerState(int layerIndex)
    {
        // 現在のステート情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // 同じステートに再度遷移（normalizedTime = 0f）
        animator.Play(stateInfo.fullPathHash, layerIndex, 0f);
    }

    void SyncUpperWithLower()
    {
        //下半身のBlendTreeで、現在動いているアニメーションとnormalizedTimeを取得して
        Debug.Log("上半身がグレネードのBlendTreeに移動した → 下半身も同期させる");

        Vector2 lowerDir = new Vector2(
        animator.GetFloat("Horizontal"),
        animator.GetFloat("Vertical")
        );

        //下半身のBlendTreeの方向を取得
        string direction = GetCurrentBlendDirection(lowerDir);
        Debug.Log($"下半身のBlend方向: {direction}");

        //正規化された時間を取得
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(lowerLayer).normalizedTime % 1f;

        //上半身のBlendTreeを、下半身のBlendTreeと同じアニメーションに遷移させる
        int upperStateHash = direction switch
        {
            "Idle" => Animator.StringToHash("empty idle"),
            "Forward" => Animator.StringToHash("empty running"),
            "Back" => Animator.StringToHash("empty running back"),
            "Left" => Animator.StringToHash("empty left strafe"),
            "Right" => Animator.StringToHash("empty right strafe"),
            _ => Animator.StringToHash("empty idle")
        };
        animator.Play(Animator.StringToHash("empty running"), upperLayer, normalizedTime);
    }

    string GetCurrentBlendDirection(Vector2 dir)
    {
        if (dir.magnitude < 0.1f)
            return "Idle";

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? "Right" : "Left";
        }
        else
        {
            return dir.y > 0 ? "Forward" : "Back";
        }
    }
}
