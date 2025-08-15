using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static TMPro.Examples.ObjectSpin;
using static UnityEngine.UI.Image;


public enum WeaponActionState
{ 
    Idle,
    ChangingWeapon,
    Reloading,
    Firing,
    SwordAttacking,
    GrenadePreparing,
    GrenadeThrowing,
    Stun, //行動不能中、武器アクションも移動もできない

}


public struct InputBufferStruct
{
    public bool jump ;
    public bool swordFireDown ; //発射ボタンの入力バッファ
    public bool reload; //リロードボタンの入力バッファ

    public InputBufferStruct(bool jump, bool swordFireDown, bool assaultFire, bool grenadeFireDown, bool grenadeFireUp, bool reload)
    {
        this.jump = jump;
        this.swordFireDown = swordFireDown;
        this.reload = reload;
    }


}

public class PlayerAvatar : NetworkBehaviour
{
    public GameObject headObject;
    [SerializeField] private GameObject bodyObject;

    private CharacterController characterController;
    [SerializeField] private HitboxRoot myPlayerHitboxRoot;

    TPSCameraController tpsCameraController; //TPSカメラコントローラーの参照

    // プレイヤーの身体能力を設定するための変数群
    [Header("Player Settings")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;
    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 1.5f; //レイキャストの距離


    private Vector3 velocity; //主に重力に使用


    private Transform tpsCameraTransform;
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;

    private List<ActionStruct> actionAnimationPlayList = new List<ActionStruct> { };  //再生すべきアクションアニメーションのリスト
    public IReadOnlyList<ActionStruct> ActionAnimationPlayList => actionAnimationPlayList;


    public Vector3 normalizedInputDirection = Vector3.zero; //入力方向の正規化されたベクトル。OnInput()で参照するためpublic

    //見た目周り
    private Renderer[] _renderers;

    //座標同期用のネットワークプロパティ
    [Networked] public Vector3 avatarPositionInHost { get; set; } = Vector3.zero; //ホスト環境でのアバター位置(入力権限のあるプレイヤーの位置を参照するために使用)
    [Networked] public Vector3 cameraForwardInHost { get; set; } = Vector3.zero; //カメラの向き(入力権限のあるプレイヤーの回転を参照するために使用)
    [Networked] public Vector3 normalizedInputDirectionInHost { get; set; } = Vector3.zero; //入力権限のあるプレイヤーの入力方向を参照するために使用
    [Header("Dummy Settings")]
    [SerializeField] bool isDummy = false;

    #region フラグ管理
    //行動可能かどうかのフラグ

    [SerializeField, Tooltip("デバッグ用の現在の状態")]
    private WeaponActionState currentWeaponActionState; //行動可能状態かどうか(移動・ジャンプ・武器アクションが可能かどうか)
    public WeaponActionState CurrentWeaponActionState
    {
        get { return currentWeaponActionState; }
        set { currentWeaponActionState = value; }
    }

    [Header("Player State Flags")]
    [SerializeField]
    private bool isImmobilized = false; //行動不能中かどうか(移動・ジャンプもできない)

    public bool wasGrounded;
    public bool isGroundedNow;



    //ADS中かどうか(エイムダウンサイト、スコープ覗き込み)
    private bool isADS = false;


    [SerializeField] private bool isHoming = false; // ホーミング中かどうか
    float timeElapsedUntilHomingFinish = 0f; // ホーミング開始からの経過時間

    public float TimeElapsedUntilHomingFinish
    {
        get { return timeElapsedUntilHomingFinish; }
        set { timeElapsedUntilHomingFinish = value; }
    }

    [SerializeField]private bool isFollowingCameraForward = true; //カメラの前方向に向くかどうか(デフォルトはtrue)


    float timer_isFollowingCameraForward = 0f; //カメラの前方向に向くまでのタイマー(0.1秒かけてカメラの前方向に向く)
    public float Timer_IsFollowingCameraForward
    {
        get { return timer_isFollowingCameraForward; }
        set { timer_isFollowingCameraForward = value; }
    }

    //private bool isInvincible = false; //無敵状態かどうか(デフォルトはfalse)
    private bool isDead = false; //死亡状態かどうか(デフォルトはfalse)



    public bool IsImmobilized
    {
        get { return isImmobilized; }
        set { isImmobilized = value; }
    }

    public bool IsHoming
    {
        get { return isHoming; }
        set { isHoming = value; }
    }
    public bool IsFollowingCameraForward
    {
        get { return isFollowingCameraForward; }
        set { isFollowingCameraForward = value; }
    }

    public bool IsDead
    {
        get { return isDead; }
        set
        {

            isDead = value;
            //if (isDead)
            //{
            //    currentWeaponActionState = WeaponActionState.Stun; //行動不能状態に設定
            //    isImmobilized = true; //行動不能フラグを設定
            //}
            //else
            //{
            //    currentWeaponActionState = WeaponActionState.Idle; //行動不能状態を解除
            //    isImmobilized = false; //行動不能フラグを解除
            //}

        }

    }

    // 先行入力の構造体。各武器の発射やリロードなどの入力をバッファリングするために使用
    public InputBufferStruct inputBuffer = new InputBufferStruct(false, false, false, false, false, false);


    float inputBufferTimer_jump = 0f; //ジャンプの入力バッファタイマー
    float inputBufferTimer_swordFireDown = 0f; //発射ボタンの入力バッファタイマー
    float inputBufferTimer_fireUp = 0f; //発射ボタンの入力バッファタイマー
    float inputBufferTimer_reload = 0f; //リロードボタンの入力バッファタイマー

    float inputBufferDuration_jump = 0.1f; //入力バッファの持続時間(0.1秒)
    float inputBufferDuration_swordFireDown = 0.1f; //入力バッファの持続時間(0.1秒)
    float inputBufferDuration_fireUp = 0.1f; //入力バッファの持続時間(0.1秒)
    float inputBufferDuration_reload = 0.1f; //入力バッファの持続時間(0.1秒)

    float stateTimer_ReturnToIdle = 0f; //武器アクション後、Idle状態に戻るまでのタイマー
    float duration_returnToIdle = 0f;


    #endregion



    //Weapon関連
    private WeaponType currentWeapon = WeaponType.AssaultRifle; //現在の武器タイプ
    public WeaponType CurrentWeapon => currentWeapon; //現在の武器タイプを取得するプロパティ
    [SerializeField] private Dictionary<WeaponType, WeaponBase> weaponClassDictionary = new Dictionary<WeaponType, WeaponBase>(); //武器タイプと武器の対応関係を保持する辞書
    public IReadOnlyDictionary<WeaponType, WeaponBase> WeaponClassDictionary => weaponClassDictionary; //武器クラス辞書を読み取り専用で公開するプロパティ

    // クラス生成・弾薬データ変更時のイベント
    public static event Action<PlayerAvatar> OnWeaponSpawned;
    public event Action<int, int> OnAmmoChanged;
    public event Action<WeaponType, int, int> OnWeaponChanged;
    public event Action<bool> OnADSChanged; // ADS状態変更イベント

    #region ホーミング関連
    //
    [Header("Homing Settings")]
    [SerializeField] private float chaseAngle = 90f; // FOVの角度（度単位）
    [SerializeField] private float chaseRange = 5f; // 射程距離
    [SerializeField] private float chaseSpeed = 12f; // 移動速度
    [SerializeField] private float maxTurnAnglePerFrame = 5f; // 追尾の角度（度単位）
    [SerializeField] private float homingDistanceThreshold = 0.1f; // ホーミングの距離しきい値
    [SerializeField] private float maxAlignToCameraAnglePerSecond = 360f; // 追尾の角度（度単位）
    [SerializeField] private float homingStartTime = 0.2f; // ホーミングの時間
    [SerializeField] private float homingTime = 1f; // ホーミングの時間
    [SerializeField] private float attackImmolizedTime = 1.5f; // 攻撃開始→攻撃後硬直終了までの時間
    [SerializeField] private float rotationDuration = 0.1f; //カメラの前方向に向くまでの時間(0.1秒かけてカメラの前方向に向く)
    private Vector3 homingMoveDirection = Vector3.forward; // ホーミング中の現在の移動方向
    private Transform currentTargetTransform; // 現在のターゲットのTransform
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionMask;

    #endregion



    Vector3 debug_lastFramePosition= Vector3.zero; //デバッグ用の前フレームの位置を保持する変数
    //----------------------ここまで変数宣言----------------------------------


    #region 初期化

    public override void Spawned()
    {
        //SetNickName($"Player({Object.InputAuthority.PlayerId})");
        //PlayerAvatarのRenderer
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        myPlayerHitboxRoot = GetComponent<HitboxRoot>();

        Hitbox[] myHitBoxes = myPlayerHitboxRoot.Hitboxes; //HitboxRootからHitboxを取得

        foreach (Hitbox hitbox in myHitBoxes)
        {
            if (hitbox is PlayerHitbox playerHitbox)
            {
                playerHitbox.hitPlayerRef = Object.InputAuthority; //PlayerHitboxのhitPlayerRefにInputAuthorityを設定
            }

        }

        // CharacterController コンポーネントを取得
        characterController = GetComponent<CharacterController>();

        if (HasInputAuthority)
        {
            //自分のアバターなら、TPSカメラに紐づける
            tpsCameraController = FindObjectOfType<TPSCameraController>();

            tpsCameraController.SetPlayerAvatar(this); //TPSカメラコントローラーに自身のPlayerAvatarを設定
            tpsCameraTransform = tpsCameraController.transform;

            // 入力管理オブジェクトを取得し、自身のアバターを登録
            NetworkInputManager networkInputManager = FindObjectOfType<NetworkInputManager>();
            networkInputManager.myPlayerAvatar = this;

            // TPSカメラのターゲットスクリプトに自身のTransformを設定
            TPSCameraTarget cameraTargetScript = FindObjectOfType<TPSCameraTarget>();
            if (cameraTargetScript != null)
            {
                cameraTargetScript.player = this.transform;
            }
        }

        //ApplyHostTransform();


        //characterController.Move(Vector3.zero); //初期位置での移動を防ぐために、初期位置でMoveを呼ぶ

        SetWeapons(); //武器の初期化を行う

        if (Object.InputAuthority == PlayerRef.None) { isDummy = true; }//ダミーキャラならisDummyをtrueにする
        else { isDummy = false; } //ダミーキャラでなければisDummyをfalseにする

        //Debug用のHitbox可視化
        HitboxRoot root = GetComponent<HitboxRoot>();
        if (HitboxDebugVisualizer.Instance != null)
        {
            HitboxDebugVisualizer.Instance.Register(root);
        }


        // 初期スポーンポイントにテレポートする
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RPC_RequestTeleportSpawnPoint(Object.InputAuthority);
        }
        else
        {
            TeleportToInitialSpawnPoint(new Vector3(0, 2, 0), Quaternion.Euler(0, 180, 0));
        }

    }


    void SetWeapons()
    {
        // 子オブジェクトから非アクティブも含めてすべてのWeaponBaseコンポーネントを取得
        WeaponBase[] weaponScripts = GetComponentsInChildren<WeaponBase>(includeInactive: true);

        // 取得したすべての武器スクリプトに対して処理を行う
        foreach (WeaponBase weapon in weaponScripts)
        {
            // 同じ武器タイプがまだ辞書に登録されていなければ追加
            if (!weaponClassDictionary.ContainsKey(weapon.weaponType))
            {
                weaponClassDictionary.Add(weapon.weaponType, weapon);
                weapon.currentMagazine = weapon.weaponType.MagazineCapacity(); //初期マガジンは最大値に設定
                weapon.currentReserve = weapon.weaponType.ReserveCapacity(); //初期リザーブは50

                weapon.playerAvatar = this; //武器のPlayerAvatarを設定

                if (weapon.weaponType == WeaponType.AssaultRifle)
                {
                    AssaultRifle rifle = weapon as AssaultRifle; //AssaultRifle型にキャスト
                    if (rifle != null)
                    {
                        rifle.TPSCameraTransform = tpsCameraTransform; //マズルのTransformをカメラのTransformに設定

                    }
                }
                else if (weapon.weaponType == WeaponType.SemiAutoRifle)
                {
                    SemiAutoRifle rifle = weapon as SemiAutoRifle; //SemiAutoRifle型にキャスト
                    if (rifle != null)
                    {
                        rifle.TPSCameraTransform = tpsCameraTransform; //マズルのTransformをカメラのTransformに設定
                    }
                }

            }
            else
            {
                // 同じ武器タイプがすでに登録済みであることを警告として出力
                Debug.LogWarning($"Weapon {weapon.weaponType} already exists in the dictionary.");
            }
        }


        //FindObjectOfType<HUDManager>()?.WeaponHUDInitialize(this);
        OnWeaponSpawned?.Invoke(this); //武器生成イベントを発火

    }


    public void TeleportToInitialSpawnPoint(Vector3 initialSpawnPoint, Quaternion? playerRotation = null)
    {
        characterController.enabled = false; // CharacterControllerを一時的に無効化

        var rot = playerRotation ?? Quaternion.identity;
        Debug.Log($"Teleporting to initial spawn point: {initialSpawnPoint}, rotation: {rot.eulerAngles}"); //デバッグログ
        // 初期スポーンポイントにテレポートする
        if (tpsCameraController != null) { tpsCameraController.SetYawPitch(rot.eulerAngles.y, rot.eulerAngles.x); }
        transform.SetPositionAndRotation(initialSpawnPoint, rot);

        characterController.enabled = true; // 再度有効化して、衝突判定をリセット

    }


    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        //Debug用のHitbox可視化
        HitboxRoot root = GetComponent<HitboxRoot>();
        if (HitboxDebugVisualizer.Instance != null)
        {
            HitboxDebugVisualizer.Instance.Unregister(root);
        }

    }

    public void StunPlayer()
    {
        currentWeaponActionState = WeaponActionState.Stun; //行動不能状態に設定
        isImmobilized = true; //行動不能フラグを設定
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ClearStun()
    {

        currentWeaponActionState = WeaponActionState.Idle; //行動不能状態を解除
        isImmobilized = false; //行動不能フラグを解除

    }


    #endregion


    #region Update系
    void Update()
    {

        if (HasInputAuthority)
        {
            CheckInput();
        }



        if (isDummy) //ダミーキャラなら、ダミー落下処理を行う
        {
            DummyFall();
        }

    }

    public override void FixedUpdateNetwork()
    {

        SynchronizeTransform();
    }

    int groundBufferFrames = 3;
    int ungroundedFrameCount = 0;

    // 入力をチェック、接地判定などアクションの実行可否は判定しない
    //順序管理のため、PlayerCallOrderから呼ばれる
    public void CheckInput()
    {


        if (Object.HasInputAuthority) //入力権限がない場合は何もしない
        {




            PlayerInputData localInputData = PlayerInputData.Default();
            if (!LocalInputHandler.isOpenMenu) //メニューが開いていないなら更新
            {
                localInputData = LocalInputHandler.CollectInput();
            }


            //死んでたら重力以外何もしない
            if (isDead)
            {
                // 重力を加算（ここを省略すれば浮く）
                velocity.y += gravity * Time.deltaTime;
                // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
                characterController.Move((velocity) * Time.deltaTime);
                // 着地しているなら重力リセット
                if (characterController.isGrounded)
                {
                    velocity.y = 0;
                }

                return;
            }

            ManageInputBufferDuration(localInputData); //入力バッファの持続時間を管理する




            //レイキャストで接地判定を行う新しい処理を書く
            isGroundedNow = characterController.isGrounded || CheckGround();


            if (inputBuffer.jump && isGroundedNow && currentWeaponActionState != WeaponActionState.Stun
                //&& !isImmobilized
                )//ここに接地判定を追加
            {
                Jump();
                inputBuffer.jump = false; //ジャンプのバッファをクリア
            }

            // 接地判定
            CheckLand();

            //武器アクションの状態管理

            //死んでる時と投擲準備中は、明示的にIdleに戻さないと復帰しない
            if (currentWeaponActionState != WeaponActionState.Stun && currentWeaponActionState != WeaponActionState.GrenadePreparing)
            {
                stateTimer_ReturnToIdle = Math.Max(stateTimer_ReturnToIdle - Time.deltaTime, 0); //状態タイマーを更新し、最大値を設定
                if (stateTimer_ReturnToIdle <= 0f) //状態タイマーが0以下になったら、Idle状態に戻す
                {
                    currentWeaponActionState = WeaponActionState.Idle; //現在のアクションをIdleに設定
                }
            }

            if (isHoming)
            {
                if (timeElapsedUntilHomingFinish > 0f)
                {
                    timeElapsedUntilHomingFinish -= Time.deltaTime; // ホーミング開始からの経過時間を更新
                }
                else
                {
                    timeElapsedUntilHomingFinish = 0f;
                    isHoming = false; // ホーミング終了
                    currentTargetTransform = null; // ターゲットをクリア

                }


            }



            //武器切り替え
            //武器切り替え条件を満たしている場合、武器を切り替える

            if (CanChangeWeapon(localInputData, inputBuffer, currentWeaponActionState)) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
            {
                ChangeWeapon(localInputData.weaponChangeScroll);
                return;
            }




            //武器毎のUpdate処理
            if (weaponClassDictionary.TryGetValue(currentWeapon, out WeaponBase currentWeaponScript))
            {


                currentWeaponScript.CalledOnUpdate(localInputData, inputBuffer, currentWeaponActionState); //武器のUpdate処理を呼び出す

            }


            normalizedInputDirection = localInputData.wasdInput.normalized;


            ChangeTransformLocally(normalizedInputDirection, tpsCameraTransform.forward);//ジャンプによる初速度も考慮して移動する
        }


    }


    void ManageInputBufferDuration(PlayerInputData localInputData)
    {


        //アクション入力をチェックする。アクションの実行可否は判定しない
        if (inputBuffer.jump)
        {
            inputBufferTimer_jump += Time.deltaTime;
            if (inputBufferTimer_jump >= inputBufferDuration_jump)
            {
                inputBuffer.jump = false; //バッファをクリア
                inputBufferTimer_jump = 0f; //タイマーをリセット
            }

        }
        //入力が入ったらバッファに追加する
        if (localInputData.JumpPressedDown) //ジャンプボタンが押されたら、バッファに追加
        {
            inputBuffer.jump = true;
            inputBufferTimer_jump = 0f; //タイマーをリセット
        }


        //射撃
        if (currentWeapon == WeaponType.Sword)
        {
            if (inputBuffer.swordFireDown)
            {
                inputBufferTimer_swordFireDown += Time.deltaTime;
                if (inputBufferTimer_swordFireDown >= inputBufferDuration_swordFireDown)
                {
                    inputBuffer.swordFireDown = false; //バッファをクリア
                    inputBufferTimer_swordFireDown = 0f; //タイマーをリセット
                }
            }
            if (localInputData.FirePressedDown) //発射ボタンが押されたら、バッファに追加
            {
                inputBuffer.swordFireDown = true;
                inputBufferTimer_swordFireDown = 0f; //タイマーをリセット
            }
        }





        //リロードのバッファをチェック
        if (currentWeapon.IsReloadable())
        {
            if (inputBuffer.reload)
            {
                inputBufferTimer_reload += Time.deltaTime;
                if (inputBufferTimer_reload >= inputBufferDuration_reload)
                {
                    inputBuffer.reload = false; //バッファをクリア
                    inputBufferTimer_reload = 0f; //タイマーをリセット
                }
            }

            if (localInputData.ReloadPressedDown) //リロードボタンが押されたら、バッファに追加
            {
                inputBuffer.reload = true;
                inputBufferTimer_reload = 0f; //タイマーをリセット
            }
        }
        else
        {
            //リロードが不要な武器の場合、リロードのバッファをクリア
            if (inputBuffer.reload)
            {
                inputBuffer.reload = false; //リロードのバッファをクリア
                inputBufferTimer_reload = 0f; //タイマーをリセット
            }
        }



        if (localInputData.FirePressedDown) //発射ボタンが押されたら、バッファに追加
        {
            inputBuffer.swordFireDown = true;
            inputBufferTimer_swordFireDown = 0f; //タイマーをリセット
        }

    }




    bool CanChangeWeapon(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition = (localInputData.weaponChangeScroll != 0f);

        bool stateCondition =
            currentAction != WeaponActionState.SwordAttacking &
            currentAction != WeaponActionState.GrenadeThrowing &
            currentAction != WeaponActionState.Stun;

        return inputCondition && stateCondition;



    }


    public void SetReturnTimeToIdle(float returnTime)
    {
        stateTimer_ReturnToIdle = returnTime; //アクション後、Idle状態に戻るまでの時間を設定
    }



    #endregion


    #region 見た目関連

    public void HideMesh()
    {
        foreach (var rend in _renderers)
        {
            bool isMesh = rend is MeshRenderer || rend is SkinnedMeshRenderer;
            if (!isMesh) continue;

            rend.enabled = false;
        }
    }

    public void ShowMesh()
    {
        foreach (var rend in _renderers)
        {
            bool isMesh = rend is MeshRenderer || rend is SkinnedMeshRenderer;
            if (!isMesh) continue;
            rend.enabled = true;
        }
    }





    #endregion

    #region transform変化


    //移動方向と向きたい前方向を元に、ローカルプレイヤーのTransformを変更する

    public void ChangeTransformLocally(Vector3 normalizedInputDir, Vector3 lookForwardDir)
    {
        Vector3 moveVelocity = Vector3.zero; // 初期化

        //方向変換
        if (isFollowingCameraForward)
        {
            //デバッグログ
            Vector3 bodyForward = new Vector3(lookForwardDir.x, 0f, lookForwardDir.z).normalized;
            // ローカルプレイヤーの移動処理


            if (bodyForward.sqrMagnitude > 0.0001f)
            {
                // プレイヤー本体の向きをカメラ方向に回転
                bodyObject.transform.forward = bodyForward;
            }

            //Debug.Log($"lookForwardDir. {lookForwardDir}"); //デバッグログ

            ;
            // headObject.transform.forward = lookForwardDir.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸によって変えること)
            Quaternion currentRotation = headObject.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(lookForwardDir.normalized);
            headObject.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 10f);

        }
        else
        {

            //タイマー更新
            if (timer_isFollowingCameraForward > 0f)
            { 
                if(timer_isFollowingCameraForward < rotationDuration)
                {
                    // カメラのY軸の回転をターゲットとする
                    float targetY = tpsCameraTransform.eulerAngles.y;
                    Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

                    // 回転速度に基づいて現在の向きを補間
                    bodyObject.transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        maxAlignToCameraAnglePerSecond * Time.deltaTime
                    );
                }
                timer_isFollowingCameraForward -= Time.deltaTime;

            }
            else
            {
                isFollowingCameraForward = true; //カメラの前方向に向く
                timer_isFollowingCameraForward = 0f; //タイマーをリセット
            }

            //ホーミング中かつ標的がいる場合、ターゲットに向かって移動する
            if (isHoming && currentTargetTransform != null)
            {

                ChangeAngleInHoming(bodyObject.transform, currentTargetTransform);

               

            }
            
        }

        //座標移動
        
        if (　currentWeaponActionState == WeaponActionState.Stun)
        {
            // 行動不能中は移動方向をゼロにする
            moveVelocity = Vector3.zero;
        }
        else if (currentWeaponActionState == WeaponActionState.SwordAttacking)
        {

            
            if (isHoming)
            {

                moveVelocity = StepInHoming(bodyObject.transform, currentTargetTransform);
            }
            else
                            {
                // 剣攻撃中の移動してない時間
                moveVelocity = Vector3.zero;
            }
        }
        else
        {
            // 入力方向に基づいて移動方向を計算
            moveVelocity = Quaternion.LookRotation(bodyObject.transform.forward, Vector3.up) * normalizedInputDir *moveSpeed;
            Debug.Log($"MoveVelocity:( {moveVelocity.x},{moveVelocity.y},{moveVelocity.z},InputData:{normalizedInputDir},KeyInput:D={Input.GetKey(KeyCode.D)},Axis={Input.GetAxis("Horizontal")}"); //デバッグログ
        }



        // 重力を加算（ここを省略すれば浮く）
        velocity.y += gravity * Time.deltaTime;
        // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる

        Vector3 MoveVec = (moveVelocity + velocity) * Time.deltaTime;
        characterController.Move(MoveVec);

        Debug.Log($"characterController.Move: ({MoveVec.x},{MoveVec.y},{MoveVec.z})"); //デバッグログ

        Vector3 positionDiff= bodyObject.transform.position - debug_lastFramePosition; //デバッグ用の前フレームの位置との差分を計算

        Debug.Log($"Move PositionDiff: ({positionDiff.x},{positionDiff.y},{positionDiff.z})"); //デバッグログ
        debug_lastFramePosition = bodyObject.transform.position; //デバッグ用の前フレームの位置を更新


        // 着地しているなら重力リセット
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
    }

    void DummyFall()
    {

        // 重力を加算（ここを省略すれば浮く）
        velocity.y += gravity * Time.deltaTime;
        // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
        characterController.Move((velocity) * Time.deltaTime);
        // 着地しているなら重力リセット
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
        Debug.Log($"Dummy {Object.InputAuthority} is falling. Position: {transform.position}"); //デバッグログ

    }

    #endregion


    #region ホーミング関連

    //ホーミング
    public bool TryGetClosestTargetInRange(Vector3 origin, Vector3 forward, float range, float fovAngle, out Transform targetTransform)
    {

        var hits = new List<LagCompensatedHit>();
        int hitCount = Runner.LagCompensation.OverlapSphere(
            origin,
            range,
            Object.InputAuthority,
            hits,
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.IgnoreInputAuthority // HitOptions.Noneを使用して、すべてのヒットを取得する
            ); // プレイヤーのレイヤーマスクを使用して近くの敵を検出

        Debug.Log("Homing hitCount:" + hitCount);

        float closestDistance = Mathf.Infinity;
        targetTransform = null;
        //float minDistance = Mathf.Infinity;

        foreach (LagCompensatedHit hit in hits)
        {
            Transform enemyTransform = hit.GameObject.transform;
            Transform enemyHeadTransform = null; // 敵の頭のTransformを格納する変数
            if (enemyTransform.TryGetComponent<PlayerAvatar>(out PlayerAvatar enemyPlayerAvatar))
            {
                // PlayerAvatarコンポーネントを持つ敵のみを対象とする
                enemyHeadTransform = enemyPlayerAvatar.headObject.transform;
            }
            else
            {
                if (enemyTransform.parent.TryGetComponent<PlayerAvatar>(out PlayerAvatar enemyParentPlayerAvatar))
                {
                    // 親にPlayerAvatarコンポーネントを持つ敵も対象とする
                    enemyHeadTransform = enemyParentPlayerAvatar.headObject.transform;
                }
                else
                {
                    Debug.Log("homing not have playerAvatar");
                    continue; // PlayerAvatarコンポーネントがない場合はスキップ
                }
                //continue; // PlayerAvatarコンポーネントがない場合はスキップ
            }


            Vector3 toEnemy = (enemyTransform.position - origin);
            float angle = Vector3.Angle(forward, toEnemy.normalized); // 必要に応じて forward をプレイヤーの forward に変更
            float distance = toEnemy.magnitude;



            if (angle > fovAngle / 2f || distance > range)
            {
                Debug.Log("homing not correct angle or distance");
                continue;
            }


            // Raycast視線チェック
            Vector3 from = headObject.transform.position;
            Vector3 to = enemyHeadTransform.position;
            Vector3 direction = (to - from).normalized;
            float rayDistance = Vector3.Distance(from, to);

            if (Physics.Raycast(from, direction, out RaycastHit hitInfo, rayDistance, obstructionMask))
            {
                // Rayが途中で何かに当たっていたら視線が通っていない
                Debug.Log("not homing because raycasthit something");
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetTransform = enemyTransform;
            }


        }

        Debug.Log("homing target:", targetTransform);
        return targetTransform != null;
    }


    void StartHoming(Transform targetTransform)
    {
        if (targetTransform != null)
        {
            isHoming = true;// ホーミングを開始

            // 初期方向をターゲットの方向に設定
            Vector3 toTarget = (targetTransform.position - transform.position);
            toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
            toTarget.Normalize(); // 正規化して方向ベクトルにする

            //bodyObject.transform.forward = toTarget; // プレイヤーの向きをターゲット方向に設定

            homingMoveDirection = toTarget; // 現在の移動方向を更新

            timeElapsedUntilHomingFinish = homingTime;


        }
        else
        {
            isHoming = true;// ホーミングを開始
                            // ターゲットが見つからない場合は前進するだけ
            timeElapsedUntilHomingFinish = homingTime;
        }
    }


    void Homing(Transform attackeTransform, Transform targetTransform)
    {
        // 移動

        ChangeAngleInHoming(attackeTransform, targetTransform); // ホーミング中の角度を調整

        if (currentTargetTransform != null)
        {
            Vector3 toTarget = (targetTransform.position - attackeTransform.position);
            Debug.Log($"Homing to target: {targetTransform.name} at position {targetTransform.position}");
            toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
            Vector3 toTargetDirection = toTarget.normalized; // 正規化して方向ベクトルにする

            // 今の移動方向から敵への方向との差を角度で計算
            float angleToTarget = Vector3.Angle(homingMoveDirection, toTargetDirection);

            // 角度差がある場合は補正
            if (angleToTarget > 0f)
            {
                // 回転角の制限（最大maxTurnAnglePerFrame度）
                float t = Mathf.Min(1f, maxTurnAnglePerFrame / angleToTarget);
                homingMoveDirection = Vector3.Slerp(homingMoveDirection, toTargetDirection, t);
            }

            Debug.Log($"Homing Move Direction: {homingMoveDirection},distance:{toTarget.magnitude}");
            if (toTarget.magnitude > homingDistanceThreshold)
            {
                characterController.Move(bodyObject.transform.forward * chaseSpeed * Time.deltaTime);
            }

            //transform.position += homingMoveDirection * chaseSpeed * Time.deltaTime;

            // 向きを移動方向に合わせる（任意）
            bodyObject.transform.forward = homingMoveDirection;

            //Debug.Log($"Homing towards {targetTransform.name}");
        }
        else
        {
            characterController.Move(bodyObject.transform.forward * chaseSpeed * Time.deltaTime);

        }


    }

    void ChangeAngleInHoming(Transform attackerTransform, Transform targetTransform)
    {
        if (targetTransform != null)
        {

            Vector3 toTarget = (targetTransform.position - attackerTransform.position);
            Debug.Log($"Homing to target: {targetTransform.name} at position {targetTransform.position}");
            toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
            Vector3 toTargetDirection = toTarget.normalized; // 正規化し.て方向ベクトルにする

            // 今の移動方向から敵への方向との差を角度で計算
            float angleToTarget = Vector3.Angle(homingMoveDirection, toTargetDirection);

            // 角度差がある場合は補正
            if (angleToTarget > 0f)
            {
                // 回転角の制限（最大maxTurnAnglePerFrame度）
                float t = Mathf.Min(1f, maxTurnAnglePerFrame / angleToTarget);
                homingMoveDirection = Vector3.Slerp(homingMoveDirection, toTargetDirection, t);
            }

            attackerTransform.forward = homingMoveDirection;

        }
    }

    Vector3 StepInHoming(Transform attackerTransform, Transform targetTransform)
    {

        Vector3 StepVector= Vector3.zero; // 初期化
        if (targetTransform != null)
        {
            Vector3 toTarget = (targetTransform.position - attackerTransform.position);
            toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
            Vector3 toTargetDirection = toTarget.normalized; // 正規化して方向ベクトルにする


            if (toTarget.magnitude > homingDistanceThreshold)
            {
                StepVector = attackerTransform.forward * chaseSpeed; // ステップベクトルを計算
            }


        }
        else 
        { 
            StepVector = attackerTransform.forward * chaseSpeed; // ターゲットがない場合は前進するだけ


        }

        Debug.Log($"Step Vector: {StepVector}, Target: {targetTransform?.name}");
        return StepVector; // ステップベクトルを返す


    }
 



    ////攻撃後の硬直時間の管理
    //IEnumerator PostAttackDelayCoroutine()
    //{
    //    yield return new WaitForSeconds(attackImmolizedTime);

    //    if (currentWeaponActionState != WeaponActionState.Stun)
    //    {
    //        isImmobilized = false; // 行動不能を解除
    //    }

    //    //currentWeaponActionState = WeaponActionState.Idle; // 現在のアクションをIdleに設定
    //    //キャラの向きをカメラの向きに徐々に戻す
    //    StartCoroutine(RotateToCameraOverTime(rotationDuration));

    //}

    ////0.1秒かけてカメラの前方向に向くコルーチン
    //private IEnumerator RotateToCameraOverTime(float duration)
    //{
    //    float elapsed = 0f;

    //    while (elapsed < duration)
    //    {
    //        // カメラのY軸の回転をターゲットとする
    //        float targetY = tpsCameraTransform.eulerAngles.y;
    //        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

    //        // 回転速度に基づいて現在の向きを補間
    //        bodyObject.transform.rotation = Quaternion.RotateTowards(
    //            transform.rotation,
    //            targetRotation,
    //            maxAlignToCameraAnglePerSecond * Time.deltaTime
    //        );

    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }
    //    isFollowingCameraForward = true; // カメラの向きに追従するように設定
    //}

    public void SetFollowingCameraForward(bool isFollowing)
    {
        isFollowingCameraForward = isFollowing; // カメラの向きに追従するかどうかを設定
    }

    #endregion


    #region 位置同期
    void SynchronizeTransform()
    {

        if (HasStateAuthority)
        {

            //ホストは、InputAuthorityのNetworkInputDataを参照してNetworkPropertyを更新する
            ShareDataFromInputAuthority();
            //ApplyInputAuthorityTransform(); //入力権限のあるプレイヤーのTransformを更新する

        }
        if (!HasInputAuthority && !isDummy)
        {

            //NetworkPropertyを参照して、ホストも他クライアントもTransformを更新する
            //Debug.Log($"{Runner.LocalPlayer}ApplyHostTransform called");
            ApplyTransformFromNetworkProperty();

        }
    }


    void ShareDataFromInputAuthority()
    {
        //Debug.Log($" Share Data From {Object.InputAuthority}");
        //入力権限のあるプレイヤーからのデータを共有する(ホストだけがGetInputで参照可能)
        if (GetInput(out NetworkInputData data))
        {
            avatarPositionInHost = data.avatarPosition; //ホスト環境でのアバター位置を更新
            cameraForwardInHost = data.headForward; //カメラの前方向を更新
            normalizedInputDirectionInHost = data.normalizedInputDirection; //入力方向を更新
        }


    }


    void ApplyTransformFromNetworkProperty()
    {

        //Debug.Log($"ApplyTransformFromNetworkProperty called. {Runner.LocalPlayer}");
        Vector3 bodyForward = new Vector3(cameraForwardInHost.x, 0f, cameraForwardInHost.z).normalized;
        // ローカルプレイヤーの移動処理     

        if (bodyForward.sqrMagnitude > 0.0001f)
        {
            // プレイヤー本体の向きをカメラ方向に回転
            bodyObject.transform.forward = bodyForward;
        }

        headObject.transform.forward = cameraForwardInHost.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸(upなのかforwardなのか)によって変えること)
        bodyObject.transform.position = avatarPositionInHost;



    }






    #endregion

　　private float extraRayLength = 1.0f;
    private float groundCheckDistance = 0.2f; // 接地判定の距離
    #region Jump関連
    void Jump()
    {
            SetActionAnimationPlayListForAllClients(ActionType.Jump); //アクションアニメーションのリストに追加
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); //ローカルでジャンプの初速度(velocity.y)を与える   
    }

    bool CheckGround()
    {
        Vector3 origin = transform.position; // キャラクターの中心位置を基準にする
        return Physics.Raycast(origin, Vector3.down, raycastDistance);
    }

    void CheckLand()
    {
        if (!wasGrounded && isGroundedNow)
        {
            Land();
        }
        wasGrounded = isGroundedNow; 
    }

    void Land()
    {
            SetActionAnimationPlayListForAllClients(ActionType.Land);   
    }

    #endregion


    #region Weapon関連



    public void SwordAction()
    {
        //近接武器の処理
        isImmobilized = true; //行動不能
        currentWeaponActionState = WeaponActionState.SwordAttacking; //武器アクション状態を近接攻撃に設定
        stateTimer_ReturnToIdle= attackImmolizedTime; //近接攻撃の待機時間を設定

        isFollowingCameraForward = false; // カメラの向きに追従しないように設定
        timer_isFollowingCameraForward = attackImmolizedTime; // カメラの向きに追従するまでの時間を設定

        inputBuffer.swordFireDown = false; //近接攻撃のバッファをクリア  

        //攻撃の当たり判定やアニメーションは変わらないので共通
        weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

        //StartCoroutine(PostAttackDelayCoroutine()); //攻撃後の硬直時間を管理するコルーチンを開始

        if (TryGetClosestTargetInRange(headObject.transform.position, bodyObject.transform.forward, chaseRange, chaseAngle, out Transform targetTransform))
        {
            // 近くに敵がいる場合
            currentTargetTransform = targetTransform; // 現在のターゲットを設定
            StartHoming(currentTargetTransform); //ホーミング開始
            Debug.Log($"Homing started towards {currentTargetTransform.name} from {headObject.transform.position}");

        }
        else
        {

            // 近くに敵がいない場合、ホーミングせず前進しながら攻撃
            StartHoming(null); 
            Debug.Log("No target found for homing. Attacking in place.");


        }


    }

    //射撃。マガジンが空でもリロードしない
    public void FireAction()
    {
        Debug.Log($"FireAction called for {currentWeapon.GetName()},{Runner.Tick}"); //デバッグログ
        currentWeaponActionState = WeaponActionState.Firing; //武器アクション状態を射撃に設定
        stateTimer_ReturnToIdle = currentWeapon.FireWaitTime(); //発射ダウンの時間を設定
        //isDuringWeaponAction = true;

        weaponClassDictionary[currentWeapon].Fire(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

        if (currentWeapon.RecoilAmount_Pitch() > 0f || currentWeapon.RecoilAmount_Yaw() > 0f)
        { tpsCameraController.StartRecoil(currentWeapon); }//リコイル開始

        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火

        //StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射ダウンのコルーチンを開始
        
    }
    





    public void PrepareGrenade()
    {

        currentWeaponActionState= WeaponActionState.GrenadePreparing; //武器アクション状態をグレネード投擲に設定
        stateTimer_ReturnToIdle = currentWeapon.FireWaitTime(); //発射ダウンの時間を設定
        weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射アップを追加


    }


    public void ThrowGrenade()
    {
        currentWeaponActionState = WeaponActionState.GrenadeThrowing; //武器アクション状態をグレネード投擲に設定
        stateTimer_ReturnToIdle = currentWeapon.FireWaitTime(); //発射ダウンの時間を設定

        weaponClassDictionary[currentWeapon].FireUp(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireUpAction()); //アクションアニメーションのリストに発射アップを追加
        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火


        Debug.Log($"FireUp {currentWeapon.GetName()}"); //デバッグログ


    }


    

    public void Reload()
    {
                    currentWeaponActionState= WeaponActionState.Reloading; //現在の武器アクション状態をリロード中に設定

            if (currentWeapon.CanADS())
            {
                CancelADS(); //現在の武器がADS可能ならADSをキャンセル
            }

            //StartCoroutine(ReloadRoutine(currentWeapon, currentWeapon.ReloadTime())); //リロード処理をコルーチンで実行

            stateTimer_ReturnToIdle= currentWeapon.ReloadTime(); //リロード後の待機時間を設定
        Debug.Log($"Reloading {currentWeapon.GetName()}"); //デバッグログ
            SetActionAnimationPlayListForAllClients(currentWeapon.ReloadAction()); //アクションアニメーションのリストにリロードを追加
           

    }

    public void InvokeAmmoChanged()
    {
        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火
    }

    void ChangeWeapon(float scrollValue)
    {
        
            int weaponCount = weaponClassDictionary.Count; //武器の総数を取得 
            WeaponType newWeaponType=WeaponType.Sword; //新しい武器タイプを格納する変数
            //武器の変更可能かどうかを判定
            if (scrollValue > 0f) //スクロールアップなら次の武器に変更
            {
               newWeaponType = (WeaponType)(((int)currentWeapon + 1 + weaponCount) % weaponCount); //武器変更アクションを取得
               
            }
            else if (scrollValue < 0f) //スクロールダウンなら前の武器に変更
            {

                newWeaponType = (WeaponType)(((int)currentWeapon - 1 + weaponCount) % weaponCount); //武器変更アクションを取得

               
            }


            if (weaponClassDictionary.ContainsKey(newWeaponType))
            {

                currentWeaponActionState = WeaponActionState.ChangingWeapon;

                weaponClassDictionary[currentWeapon].ResetOnChangeWeapon(); 




                if (currentWeapon.CanADS())
                { 
                    CancelADS(); //現在の武器がADS可能ならADSをキャンセル
                }

                currentWeapon = newWeaponType;
                RPC_NotifyChangeWeapon(newWeaponType);

                Debug.Log($"ChangingWeapon {currentWeapon.GetName()}"); //デバッグログ

                OnWeaponChanged?.Invoke(currentWeapon, weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //武器変更イベントを発火
                //OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].CurrentMagazine, weaponClassDictionary[currentWeapon].CurrentReserve); //弾薬変更イベントを発火


                SetActionAnimationPlayListForAllClients(currentWeapon.ChangeWeaponAction()); //アクションアニメーションのリストに武器変更を追加
                //StartCoroutine(ChangeWeaponRoutine(currentWeapon.WeaponChangeTime())); //武器変更処理をコルーチンで実行
                stateTimer_ReturnToIdle = currentWeapon.WeaponChangeTime(); //武器変更後の待機時間を設定

            }
            else
            {
                Debug.LogWarning($"Weapon {newWeaponType} not found in newWeapon dictionary.");
            }

    
    }

    public void ForceWeaponChange(WeaponType newWeapon)
    {
        weaponClassDictionary[currentWeapon].ResetOnChangeWeapon(); //現在の武器の状態をリセット
        if (currentWeapon.CanADS())
        {
            CancelADS(); //現在の武器がADS可能ならADSをキャンセル
        }
        currentWeapon = newWeapon;
        RPC_NotifyChangeWeapon(newWeapon);
        if(Runner.LocalPlayer == Object.InputAuthority) OnWeaponChanged?.Invoke(currentWeapon, weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //武器変更イベントを発火

    }





    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_NotifyChangeWeapon(WeaponType newWeapon, RpcInfo rpcInfo = default)
    {
        if (Runner.LocalPlayer != rpcInfo.Source)
        {
            currentWeapon = newWeapon;

        }


    }



    public void InitializeAllAmmo()//各武器の弾薬を初期化
    {
        foreach (var weapon in weaponClassDictionary.Values)
        {
            weapon.InitializeAmmo(); 
        }
        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火
    }




    public void SwitchADS()
    {

        

            if (isADS)
            {

                Debug.Log("ADS Off"); //デバッグログ
                isADS = false; //ADSを解除
                SetActionAnimationPlayListForAllClients(ActionType.ADS_Off); //アクションアニメーションのリストにADSオフを追加
                tpsCameraController.SetADS(isADS);
                if (currentWeapon.CanADS()) { WeaponClassDictionary[currentWeapon].SetADS(isADS); }

                OnADSChanged?.Invoke(isADS); //ADS状態変更イベントを発火


            }
            else
            {

                Debug.Log("ADS On"); //デバッグログ
                isADS = true; //ADSを有効化
                SetActionAnimationPlayListForAllClients(ActionType.ADS_On);
                tpsCameraController.SetADS(isADS);
                if (currentWeapon.CanADS()) { WeaponClassDictionary[currentWeapon].SetADS(isADS); }

                OnADSChanged?.Invoke(isADS); //ADS状態変更イベントを発火
            }
        
     
    }

   




    void CancelADS()
    {
        if (!HasInputAuthority) return;
        if (currentWeapon.CanADS())
        {
            isADS = false; //ADSを解除

            SetActionAnimationPlayListForAllClients(ActionType.ADS_Off); //アクションアニメーションのリストにADSオフを追加
            tpsCameraController.CancelADS(); //カメラのADSをキャンセル
            if (currentWeapon.CanADS()) { WeaponClassDictionary[currentWeapon].SetADS(false); }
            OnADSChanged?.Invoke(isADS); //ADS状態変更イベントを発火

        }
        
    }
   

    #endregion


    #region Action共通

    //全プレイヤーへアクションアニメーションのリストを同期するためのメソッド
    //自分はローカルで、他クライアントはRPCでプレイリストを更新する(時間付き)
    public void SetActionAnimationPlayListForAllClients(ActionType actionType)
    {
        if(isDummy)
        {
            //ダミーならアニメーションはしない
            Debug.Log("Don't Set Animation because it's dummy" );
            return; 
        
        }

        float calledTime = Runner.SimulationTime; //アクションが呼ばれた時間を取得

        SetActionAnimationPlayList(actionType, calledTime); //アクションアニメーションのリストにジャンプを追加
        
        RPC_RequestActionAnimation(actionType, calledTime); //RPCを送信して他のクライアントにアクションを通知
    }




    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestActionAnimation(ActionType actionType, float calledTime, RpcInfo info = default)
    {
        // RPC送信（即送信）
        // Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {calledTime}");
        Debug.Log("Before RPC_RequestActionAnimation actionType:" + actionType);
        RPC_ApplyActionAnimation(info.Source,actionType, calledTime); //アクションアニメーションのリストに追加するだけ(接地判定も座標変化もしない)
        Debug.Log("RPC_RequestActionAnimation actionType:" + actionType);

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyActionAnimation(PlayerRef sourcePlayer, ActionType actionType, float calledTime, RpcInfo info = default)
    {
        //Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        //Debug.Log($"SourcePlayer {sourcePlayer}");
        Debug.Log($"RPC_ApplyActionAnimation called. Source: {sourcePlayer}, ActionType: {actionType}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
           // Debug.Log($" Apply Jump of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");

            SetActionAnimationPlayList(actionType, calledTime);  // アクションアニメーションのリストにジャンプを追加

        }
        else
        {
           // Debug.Log($"Don't Apply Jump because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }

    }

    public void SetActionAnimationPlayList(ActionType actiontype, float calledtime)
    {

        actionAnimationPlayList.Add(new ActionStruct
        {
            actionType = actiontype,
            actionCalledTimeOnSimulationTime = calledtime
        });

        //Debug.Log($"Play List Added: {actiontype} at {calledtime}");

    }

    public void ClearActionAnimationPlayList()
    {
        actionAnimationPlayList.Clear();
    }

    #endregion

}