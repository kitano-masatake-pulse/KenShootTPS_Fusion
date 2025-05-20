using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro nameLabel;

    [SerializeField]
    private GameObject headObject;
    

    [SerializeField]
    private GameObject bodyObject;

     NetworkCharacterControllerPrototype characterController;


    [SerializeField]
    private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    //private NetworkCharacterControllerPrototype characterController;

    //private void Awake()
    //{
    //    characterController = GetComponent<NetworkCharacterControllerPrototype>();
    //}

    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})");


        characterController=GetComponent<NetworkCharacterControllerPrototype>();

        if (HasInputAuthority)
        {
            FindObjectOfType<TPSCameraController>().SetCameraToMyAvatar(this);
        }



    }


    public override void FixedUpdateNetwork()
    {

        Debug.Log("NetworkInput");
        if (GetInput(out NetworkInputData data))
        {


            Vector3 bodyForward = new Vector3(data.cameraForward.x, 0f, data.cameraForward.z).normalized;

            if (bodyForward.sqrMagnitude > 0.0001f)
            {
                // プレイヤー本体の向きをカメラ方向に回転
                bodyObject.transform.forward = bodyForward;
            }

            // cameraForward から pitch を求める 上蓋が前面を向くように
            float pitch = - Mathf.Asin(data.cameraForward.y) * Mathf.Rad2Deg + 90;
            // 頭部回転を仰角だけに限定
            headObject.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // キャラのY軸回転を適用
            Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            // 入力方向のベクトルを正規化する
            //data.wasdInputDirection.Normalize();

            Vector3 moveDirection = yRot * data.wasdInputDirection.normalized;  // 入力方向のベクトルを正規化する
            // 入力方向を移動方向としてそのまま渡す
            characterController.Move(moveDirection);
        }
    }
    private void LateUpdate()
    {
        // プレイヤー名のテキストを、常にカメラの正面向きにする
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }

    // プレイヤー名をテキストに設定する
    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }
}