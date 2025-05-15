using UnityEngine;
using Fusion;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerCameraSetter : NetworkBehaviour
{
    [SerializeField]
    public Transform cameraFollowTarget; // 例: 背中の位置などに空の子オブジェクトを配置して指定

    void Start()
    {
        //バトルシーンでのみ実行
        string sceneName = SceneType.Battle.ToSceneName();
        if (HasInputAuthority && SceneManager.GetActiveScene().name == sceneName)
        {
            // シーンにあるVirtualCameraを取得
            var vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = cameraFollowTarget;
                vcam.LookAt = cameraFollowTarget;
                Debug.Log("Cinemachineカメラをプレイヤーに追従させました");
            }
        }
    }
}
