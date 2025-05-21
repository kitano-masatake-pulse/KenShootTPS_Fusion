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

    CharacterController characterController;

    public float moveSpeed = 3f;
    public float gravity = -9.81f;

    [SerializeField]
    private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    private Vector3 velocity; //主に重力に使用

    //private NetworkCharacterControllerPrototype characterController;

    //private void Awake()
    //{
    //    characterController = GetComponent<NetworkCharacterControllerPrototype>();
    //}



    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})");


        
        characterController = GetComponent<CharacterController>();

        if (HasInputAuthority)
        {
            //自分のアバターなら、TPSカメラに紐づける
            FindObjectOfType<TPSCameraController>().SetCameraToMyAvatar(this);
        }



    }


    public override void FixedUpdateNetwork()
    {

        Debug.Log("NetworkInput");
        if ( GetInput(out NetworkInputData data))
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
            //headObject.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            //体と同じ方法でforwardつかってみて実装する
            Vector3 headUp = data.cameraForward.normalized;

            headObject.transform.up = headUp;

            // キャラのY軸回転を適用
            // Quaternion yRot = Quaternion.Euler(data.cameraForward.x, 0f, data.cameraForward.z);
            // Quaternion yRot = Quaternion.Euler(data.cameraForward.x, 0f, data.cameraForward.z);

            // 入力方向のベクトルを正規化する
            //data.wasdInputDirection.Normalize();

            Vector3 moveDirection =Quaternion.LookRotation(bodyForward,Vector3.up) * data.wasdInputDirection;  // 入力方向のベクトルを正規化する
                                                                                                               // 入力方向を移動方向としてそのまま渡す


            // 重力を加算（ここを省略すれば浮く）
           
            velocity.y += gravity * Runner.DeltaTime;

            // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
            characterController.Move((moveDirection * moveSpeed + velocity) * Runner.DeltaTime);

            // 着地しているなら重力リセット
            if (characterController.isGrounded)
            {
                velocity.y = 0;
            }

            //bodyObject.transform.Translate(moveDirection*moveSpeed*Runner.DeltaTime);
            //characterController.Move(moveDirection);
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