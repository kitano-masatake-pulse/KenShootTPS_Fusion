using Fusion;
using Unity;
using Unity.VisualScripting;
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

    private Transform tpsCameraTransform;

    [SerializeField] private Transform hostTransform;



    public override void Spawned()
    {
        //SetNickName($"Player({Object.InputAuthority.PlayerId})");

        myPlayerHitbox.myPlayerRef = GetComponent<NetworkObject>().InputAuthority;

        characterController = GetComponent<CharacterController>();

        playerNetworkState = GetComponent<PlayerNetworkState>();

        if (HasInputAuthority)
        {
            //自分のアバターなら、TPSカメラに紐づける
            TPSCameraController tpsCameraController = FindObjectOfType<TPSCameraController>();

            tpsCameraTransform = tpsCameraController.transform;

            //tpsCameraController.SetCameraToMyAvatar(this);

            NetworkInputManager networkInputManager = FindObjectOfType<NetworkInputManager>();

            networkInputManager.myPlayerAvatar = this;

            TPSCameraTarget cameraTargetScript = FindObjectOfType<TPSCameraTarget>();
            if(cameraTargetScript != null)
            {
                cameraTargetScript.player = this.transform;
            }
            


        }

        ApplyHostTransform();

        characterController.Move(Vector3.zero); //初期位置での移動を防ぐために、初期位置でMoveを呼ぶ



    }


     void Update()
    {
        if (HasInputAuthority)
        {
            ChangeTransformLocally();
        }
    }



    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority) { return; }

        //InputAuthorityの位置を参照してホストが位置を動かす
        if (HasStateAuthority && !HasInputAuthority)
        {

            Debug.Log("ApplyInputAuthorityTransform called");
            ApplyInputAuthorityTransform();
        }

        //入力者以外のクライアントがホストの状態に合わせる
        if (!HasInputAuthority && !HasStateAuthority)
        {
            ApplyHostTransform();
        }
    }






    public void ChangeTransformLocally()
    {
       // if (!HasInputAuthority) { return; }

        Vector3 cameraForward = tpsCameraTransform.forward;

        Vector3 bodyForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
        // ローカルプレイヤーの移動処理


        if (bodyForward.sqrMagnitude > 0.0001f)
        {
            // プレイヤー本体の向きをカメラ方向に回転
            bodyObject.transform.forward = bodyForward;
        }

        headObject.transform.up = cameraForward.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸によって変えること)


        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 moveDirection = Quaternion.LookRotation(bodyForward, Vector3.up) * inputDirection.normalized;

        // 重力を加算（ここを省略すれば浮く）
        velocity.y += gravity * Time.deltaTime;
        // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
        characterController.Move((moveDirection * moveSpeed + velocity) * Time.deltaTime);
        // 着地しているなら重力リセット
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
    }



    void ApplyInputAuthorityTransform()
    {
        //Debug.Log($"GetInput {GetInput(out NetworkInputData datum)}");
        if (GetInput(out NetworkInputData data))
        { 

            this.transform.position = data.avatarPosition;
            this.transform.rotation = Quaternion.Euler(data.avatarRotation);
            Debug.Log($"ApplyInputAuthorityTransform {data.avatarPosition} {data.avatarRotation}");
            hostTransform.localPosition = Vector3.zero; //ホストの位置をリセットする
            hostTransform.localRotation = Quaternion.identity; //ホストの回転をリセットする

        }
    
    }


    void ApplyHostTransform()
    { 
    
        this.transform.position = hostTransform.position;
        hostTransform.localPosition=Vector3.zero; //ホストの位置をリセットする
        this.transform.rotation = hostTransform.rotation;
        hostTransform.localRotation = Quaternion.identity; //ホストの回転をリセットする

    }


  





    public void TakeDamage(int DamageAmount)
    {
        Debug.Log($"TakeDamage {DamageAmount}");
        //playerNetworkState.Damage(DamageAmount);みたいなのを書く
    }
}