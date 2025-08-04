using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTargetSprict : MonoBehaviour
{
    private Camera mainCamera; // 対象のカメラ
    public Transform sphere;  // 移動させたいSphere
    private float distanceFromCamera = 20f; // カメラからの距離

    private void Start()
    {
        if (mainCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                mainCamera = camObj.GetComponent<Camera>();
            }
            else
            {
                Debug.LogWarning("MainCamera という名前のカメラが見つかりませんでした");
            }
        }
    }

    void Update()
    {
        if (mainCamera == null || sphere == null) return;

        // ゲーム画面の中央からワールドへのRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // カメラ正面に一定距離進んだ位置を求める
        Vector3 targetPosition = ray.origin + ray.direction * distanceFromCamera;

        // Sphereをそこに移動
        sphere.position = targetPosition;
    }
}
