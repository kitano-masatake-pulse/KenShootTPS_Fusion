using Fusion;
using Unity;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{ 
    [SerializeField]
    private GameObject headObject;
    

    [SerializeField]
    private GameObject bodyObject;

    CharacterController characterController;

    public float moveSpeed = 3f;
    [SerializeField]
    private float gravity = -9.81f;

    [SerializeField]
    private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    private Vector3 velocity; //主に重力に使用


    [SerializeField]
    private PlayerHitbox myPlayerHitbox;


    private PlayerNetworkState playerNetworkState;




    public override void Spawned()
    {
        //SetNickName($"Player({Object.InputAuthority.PlayerId})");

        myPlayerHitbox.myPlayerRef = GetComponent<NetworkObject>().InputAuthority;

        characterController = GetComponent<CharacterController>();

        playerNetworkState= GetComponent<PlayerNetworkState>();

        if (HasInputAuthority)
        {
            //自分のアバターなら、TPSカメラに紐づける
            FindObjectOfType<TPSCameraController>().SetCameraToMyAvatar(this);
        }



    }



    public override void FixedUpdateNetwork()
    {

        //Debug.Log("NetworkInput");
        if ( GetInput(out NetworkInputData data))
        {


            if (HasStateAuthority)
            {

                Vector3 bodyForward = new Vector3(data.cameraForward.x, 0f, data.cameraForward.z).normalized;

                if (bodyForward.sqrMagnitude > 0.0001f)
                {
                    // プレイヤー本体の向きをカメラ方向に回転
                    bodyObject.transform.forward = bodyForward;
                }

                //体と同じ方法でforwardつかってみて実装する
                Vector3 headUp = data.cameraForward.normalized;

                headObject.transform.up = headUp;


                Vector3 moveDirection = Quaternion.LookRotation(bodyForward, Vector3.up) * data.wasdInputDirection.normalized;  // 入力方向のベクトルを正規化する
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
            }

        }



    }



    public void TakeDamage(int DamageAmount)
    {
        Debug.Log($"TakeDamage {DamageAmount}");
        //playerNetworkState.Damage(DamageAmount);みたいなのを書く
    }
}