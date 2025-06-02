using Fusion;
using System.Collections.Generic;

using Unity;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private GameObject headObject;
    [SerializeField] private GameObject bodyObject;

    CharacterController characterController;
    WeaponHandler weaponHandler;
    private PlayerNetworkState playerNetworkState;
    private Transform tpsCameraTransform;


    [SerializeField]
    private PlayerHitbox myPlayerHitbox;

    // プレイヤーの身体能力を設定するための変数群
    public float moveSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;



    [SerializeField]
    private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;


    [SerializeField] private Transform hostTransform;

    public List<ActionStruct> actionAnimationPlayList=new List<ActionStruct> { }   ;  //再生すべきアクションアニメーションのリスト

    public Vector3 normalizedInputDirection=Vector3.zero; //入力方向の正規化されたベクトル


    [Networked] public Vector3 avatarPositionInHost { get; set; } = Vector3.zero; //ホスト環境でのアバター位置(入力権限のあるプレイヤーの位置を参照するために使用)
    [Networked] public Vector3 cameraForwardInHost { get; set; } = Vector3.zero; //カメラの向き(入力権限のあるプレイヤーの回転を参照するために使用)
    [Networked] public Vector3 normalizedInputDirectionInHost { get; set; } = Vector3.zero; //入力権限のあるプレイヤーの入力方向を参照するために使用


    private Vector3 velocity; //主に重力に使用




    //Weapon関連
    private WeaponType currentWeapon = WeaponType.Sword; //現在の武器タイプ

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

            weaponHandler = GetComponent<WeaponHandler>();

        }

        //ApplyHostTransform();

        characterController.Move(Vector3.zero); //初期位置での移動を防ぐために、初期位置でMoveを呼ぶ



    }


     void Update()
    {
        if (HasInputAuthority)
        {
            InputAvailableCheck();
        }
    }


    // 入力をチェック、接地判定などアクションの実行可否も判定する
    //順序管理のため、PlayerCallOrderから呼ばれる
    public void InputAvailableCheck()
    {

        if (!Object.HasInputAuthority) return; //入力権限がない場合は何もしない

        PlayerInputData localInputData = LocalInputHandler.CollectInput();

        


        if (localInputData.JumpPressedDown)//ここに接地判定を追加
        {
            TryJump();
        }

        if (localInputData.FirePressedDown) //発射ボタンが押されたら、武器の発射処理を呼ぶ
        {
            weaponHandler.TryFireDown();
        }

        if (localInputData.FirePressedStay) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ
        {
            weaponHandler.TryFire();
        }

        if (localInputData.ReloadPressedDown) //リロードボタンが押されたら、武器のリロード処理を呼ぶ
        {
            weaponHandler.TryReload();
        }



        if (localInputData.weaponChangeScroll != 0f) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
        {
            weaponHandler.TryChangeWeapon(localInputData.weaponChangeScroll);
        }



        normalizedInputDirection = localInputData.wasdInput.normalized;

        ChangeTransformLocally(normalizedInputDirection, tpsCameraTransform.forward);//ジャンプによる初速度も考慮して移動する

    }

    void TryJump()
    {
        bool isGrounded = characterController.isGrounded; // 接地判定を取得
        if (isGrounded) 
        {

            Jump();// ジャンプは初速度(velocity.y)を与える
        }
    }



  




    //移動方向と向きたい前方向を元に、ローカルプレイヤーのTransformを変更する

    public void ChangeTransformLocally(Vector3 normalizedInputDir, Vector3 lookForwardDir)
    {

        Vector3 bodyForward = new Vector3(lookForwardDir.x, 0f, lookForwardDir.z).normalized;
        // ローカルプレイヤーの移動処理


        if (bodyForward.sqrMagnitude > 0.0001f)
        {
            // プレイヤー本体の向きをカメラ方向に回転
            bodyObject.transform.forward = bodyForward;
        }

        headObject.transform.up = lookForwardDir.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸によって変えること)

        Vector3 moveDirection = Quaternion.LookRotation(bodyForward, Vector3.up) * normalizedInputDir;

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





    void Jump()
    {


        float jumpCalledTime = Runner.SimulationTime;

        JumpLocally(jumpCalledTime);// ローカルでジャンプの初速度を設定＆アクションアニメーションのリストに追加


        // RPC送信
        RPC_RequestJump(jumpCalledTime);　// RPCを送信して他のクライアントにジャンプを通知


    }


    void JumpLocally(float calledTime)
    {
        Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); ; // ジャンプの初速度を設定
        Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime},velocityY:{velocity.y}");
        SetActionAnimationPlayList(ActionType.Jump, calledTime); //アクションアニメーションのリストにジャンプを追加

    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestJump(float calledTime, RpcInfo info = default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {calledTime}");
        RPC_ApplyJump(info.Source, calledTime); //アクションアニメーションのリストに追加するだけ(接地判定も座標変化もしない)


    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyJump(PlayerRef sourcePlayer, float calledTime, RpcInfo info = default)
    {
        Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        Debug.Log($"SourcePlayer {sourcePlayer}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
            Debug.Log($" Apply Jump of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");

            SetActionAnimationPlayList(ActionType.Jump, calledTime);  // アクションアニメーションのリストにジャンプを追加


        }
        else
        {
            Debug.Log($"Don't Apply Jump because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }

    }


    public void TakeDamage(int DamageAmount)
    {
        Debug.Log($"TakeDamage {DamageAmount}");
        //playerNetworkState.Damage(DamageAmount);みたいなのを書く
    }


   




    public void SetActionAnimationPlayList(ActionType actiontype, float calledtime)
    {

        actionAnimationPlayList.Add(new ActionStruct
        {
            actionType = actiontype,
            actionCalledTimeOnSimulationTime = calledtime
        });

    }

    public void ClearActionAnimationPlayList()
    {
        actionAnimationPlayList.Clear();
    }












    public override void FixedUpdateNetwork()
    {

        SynchronizeTransform();
    }




    //位置同期
    void SynchronizeTransform()
    {

        if (HasStateAuthority)
        {

            //ホストは、InputAuthorityのNetworkInputDataを参照してNetworkPropertyを更新する
            ShareDataFromInputAuthority();
            //ApplyInputAuthorityTransform(); //入力権限のあるプレイヤーのTransformを更新する

        }
        if (!HasInputAuthority)
        {

                //NetworkPropertyを参照して、ホストも他クライアントもTransformを更新する
                //Debug.Log($"{Runner.LocalPlayer}ApplyHostTransform called");
                ApplyTransformFromNetworkProperty();
            
        }
    }


    void ShareDataFromInputAuthority()
    { 
        Debug.Log($" Share Data From {Object.InputAuthority}");
        //入力権限のあるプレイヤーからのデータを共有する(ホストだけがGetInputで参照可能)
        if (GetInput(out NetworkInputData data))
        {
            avatarPositionInHost = data.avatarPosition; //ホスト環境でのアバター位置を更新
            cameraForwardInHost = data.cameraForward; //カメラの前方向を更新
            normalizedInputDirectionInHost = data.normalizedInputDirection; //入力方向を更新
        }


    }


    void ApplyTransformFromNetworkProperty()
    {

        Debug.Log($"ApplyTransformFromNetworkProperty called. {Runner.LocalPlayer}");
        Vector3 bodyForward = new Vector3(cameraForwardInHost.x, 0f, cameraForwardInHost.z).normalized;
            // ローカルプレイヤーの移動処理     

            if (bodyForward.sqrMagnitude > 0.0001f)
            {
                // プレイヤー本体の向きをカメラ方向に回転
                bodyObject.transform.forward = bodyForward;
            }

            headObject.transform.up = cameraForwardInHost.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸(upなのかforwardなのか)によって変えること)


            bodyObject.transform.position = avatarPositionInHost;

            //Debug.Log($"ApplyInputAuthorityTransform {data.avatarPosition} {data.avatarRotation}");
            hostTransform.localPosition = Vector3.zero; //ホストの位置をリセットする
            hostTransform.localRotation = Quaternion.identity; //ホストの回転をリセットする

        

    }

    void ApplyInputAuthorityTransform()
    {

        if (GetInput(out NetworkInputData data))
        {


            Vector3 bodyForward = new Vector3(data.cameraForward.x, 0f, data.cameraForward.z).normalized;
            // ローカルプレイヤーの移動処理     

            if (bodyForward.sqrMagnitude > 0.0001f)
            {
                // プレイヤー本体の向きをカメラ方向に回転
                bodyObject.transform.forward = bodyForward;
            }

            headObject.transform.up = data.cameraForward; // カメラの方向を頭の向きに設定(アバターの頭の軸(upなのかforwardなのか)によって変えること)


            bodyObject.transform.position = data.avatarPosition;

            //Debug.Log($"ApplyInputAuthorityTransform {data.avatarPosition} {data.avatarRotation}");
            hostTransform.localPosition = Vector3.zero; //ホストの位置をリセットする
            hostTransform.localRotation = Quaternion.identity; //ホストの回転をリセットする

        }


    }


    void ApplyHostTransform()
    {

        this.transform.position = hostTransform.position;
        hostTransform.localPosition = Vector3.zero; //ホストの位置をリセットする
        this.transform.rotation = hostTransform.rotation;
        hostTransform.localRotation = Quaternion.identity; //ホストの回転をリセットする

    }



}