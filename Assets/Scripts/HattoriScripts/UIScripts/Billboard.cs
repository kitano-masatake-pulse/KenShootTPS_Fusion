using UnityEngine;

//アタッチしたUIをカメラの方向に向けるスクリプト
public class Billboard : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        // メインカメラをキャッシュ
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Canvas の前面（forward）をカメラの方向に合わせる
        transform.rotation = mainCamera.transform.rotation;
    }
}