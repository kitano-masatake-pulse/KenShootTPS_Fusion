using UnityEngine;
using System.Collections.Generic;
using Fusion;

//軌跡描画スプリクト
public class TrajectoryDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    //軌跡の描画点の数
    private int resolution = 3000;
    //軌跡の時間間隔
    private float timeStep = 1/60f;
    //どのレイヤーマスクに軌跡の当たり判定があるか
    public LayerMask collisionMask;

    //軌跡の終着点に表示スルプレハブ
    public GameObject impactMarkerPrefab;
    //衝突した位置に表示するマーカーのインスタンス
    private GameObject impactMarkerInstance;
    //軌跡の衝突点が見つかったかどうかのフラグ
    private bool impactPointFound = false;

    private void Awake()
    {
        // 初期化：インスタンス生成（1つだけ）
        if (impactMarkerPrefab != null)
        {
            impactMarkerInstance = Instantiate(impactMarkerPrefab);
            // 最初は非表示
            impactMarkerInstance.SetActive(false); 
        }
    }


    // 軌跡描画関数（投げる位置,投げる向きと大きさ）
    public void RaycastDrawTrajectory(Vector3 startPos, Vector3 velocity)
    {

        Debug.Log("SphereCastDrawTrajectory called with startPos: " + startPos + ", velocity: " + velocity);
        // 軌跡の描画点の数を設定
        lineRenderer.positionCount = resolution;
        // 重力取得
        Vector3 gravity = Physics.gravity;
        // 軌跡の衝突フラグは当初 false
        impactPointFound = false;

        // 描画する回数くりかえす
        for (int i = 0; i < resolution; i++)
        {
            // 一点ごとの時間の計算
            float t = i * timeStep;
            // 軌跡の位置を計算
            Vector3 point = startPos + velocity * t + 0.5f * gravity * t * t;
            // LineRendererの描画
            lineRenderer.SetPosition(i, point);

            // 衝突予測（オプション）
            if (i > 0)
            {
                // 前の描画点の位置を取得
                Vector3 prev = lineRenderer.GetPosition(i - 1);
                // 現在の描画店の前の描画店の位置の差分を取得
                Vector3 direction = point - prev;
                // ベクトルの長さを計算する
                float distance = direction.magnitude;

                // レイキャスト（現在の描画点位置, 差分を正規化, 衝突オブジェクト取得, ベクトルの長さ, 衝突有効にするオブジェクトマスク）
                if (Physics.Raycast(point, direction.normalized, out RaycastHit hit, distance, collisionMask))
                {
                    // 先の描画点を取得し、描画する
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    // インスタンスが存在していれば
                    if (impactMarkerInstance != null)
                    {
                        // マーカーを表示する
                        impactMarkerInstance.SetActive(true);
                        // マーカーの位置を衝突点に設定
                        impactMarkerInstance.transform.position = hit.point;
                        // マーカーの回転を衝突面の法線に合わせる
                        impactMarkerInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                        // マーカーフラグを true に設定
                        impactPointFound = true;
                    }
                    // 衝突点が見つかったので、ループを抜ける
                    break;
                }
            }
        }
        // 衝突点が見つからなかった場合、マーカーを非表示にする
        if (!impactPointFound && impactMarkerInstance != null)
        {
            impactMarkerInstance.SetActive(false);
        }   
    }



    public void HideTrajectory()
    {
        // 軌跡の描画点をリセット
        lineRenderer.positionCount = 0;
        // マーカーを非表示にする
        if (impactMarkerInstance != null)
            impactMarkerInstance.SetActive(false);
    }
}
