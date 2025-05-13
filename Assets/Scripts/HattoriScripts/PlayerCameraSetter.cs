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
        if (HasInputAuthority && SceneManager.GetActiveScene().name == "LobbyScene")
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
