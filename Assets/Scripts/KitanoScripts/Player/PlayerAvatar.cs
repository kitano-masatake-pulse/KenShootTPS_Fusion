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
    Dead, //死亡状態

}


public struct InputBufferStruct
{
    public bool jump ;
    public bool swordFireDown ; //発射ボタンの入力バッファ
    public bool assaultFire; //発射ボタンの入力バッファ
    public bool grenadeFireDown; //発射ボタンの入力バッファ
    public bool grenadeFireUp; //発射ボタンの入力バッファ
    public bool reload; //リロードボタンの入力バッファ

    public InputBufferStruct(bool jump, bool swordFireDown, bool assaultFire, bool grenadeFireDown, bool grenadeFireUp, bool reload)
    {
        this.jump = jump;
        this.swordFireDown = swordFireDown;
        this.assaultFire = assaultFire;
        this.grenadeFireDown = grenadeFireDown;
        this.grenadeFireUp = grenadeFireUp;
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
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;

    private Vector3 velocity; //主に重力に使用


    private Transform tpsCameraTransform;
    [SerializeField] private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;

    private List<ActionStruct> actionAnimationPlayList = new List<ActionStruct> { };  //再生すべきアクションアニメーションのリスト
    public IReadOnlyList<ActionStruct> ActionAnimationPlayList => actionAnimationPlayList;


    public Vector3 normalizedInputDirection = Vector3.zero; //入力方向の正規化されたベクトル。OnInput()で参照するためpublic


    //座標同期用のネットワークプロパティ
    [Networked] public Vector3 avatarPositionInHost { get; set; } = Vector3.zero; //ホスト環境でのアバター位置(入力権限のあるプレイヤーの位置を参照するために使用)
    [Networked] public Vector3 cameraForwardInHost { get; set; } = Vector3.zero; //カメラの向き(入力権限のあるプレイヤーの回転を参照するために使用)
    [Networked] public Vector3 normalizedInputDirectionInHost { get; set; } = Vector3.zero; //入力権限のあるプレイヤーの入力方向を参照するために使用

    [SerializeField] bool isDummy = false;

    #region フラグ管理
    //行動可能かどうかのフラグ

    private WeaponActionState currentWeaponActionState; //行動可能状態かどうか(移動・ジャンプ・武器アクションが可能かどうか)
    public WeaponActionState CurrentWeaponActionState
    {
        get { return currentWeaponActionState; }
        set { currentWeaponActionState = value; }
    }


    private bool isDuringWeaponAction = false; //武器アクション(射撃、リロード、武器切り替え)中かどうか
    private bool isImmobilized = false; //行動不能中かどうか(移動・ジャンプもできない)

    public bool wasGrounded;
    public bool isGroundedNow;



    //ADS中かどうか(エイムダウンサイト、スコープ覗き込み)
    private bool isADS = false;


    private bool isHoming = false; // ホーミング中かどうか
    private bool isFollowingCameraForward = true; //カメラの前方向に向くかどうか(デフォルトはtrue)
    //private bool isInvincible = false; //無敵状態かどうか(デフォルトはfalse)


    //各変数のgetter/setter
    public bool IsDuringWeaponAction
    {
        get { return isDuringWeaponAction; }
        set { isDuringWeaponAction = value; }
    }
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


    //ホーミング関連
    [SerializeField] private float chaseAngle = 90f; // FOVの角度（度単位）
    [SerializeField] private float chaseRange = 5f; // 射程距離
    [SerializeField] private float chaseSpeed = 6f; // 移動速度
    [SerializeField] private float maxTurnAnglePerFrame = 5f; // 追尾の角度（度単位）
    [SerializeField] private float maxAlignToCameraAnglePerSecond = 360f; // 追尾の角度（度単位）
    [SerializeField] private float homingTime = 2f; // ホーミングの時間
    [SerializeField] private float attackImmolizedTime = 3f; // 攻撃開始→攻撃後硬直終了までの時間
    [SerializeField] private float rotationDuration = 0.1f; //カメラの前方向に向くまでの時間(0.1秒かけてカメラの前方向に向く)
    private Vector3 homingMoveDirection = Vector3.forward; // ホーミング中の現在の移動方向
    private Transform currentTargetTransform; // 現在のターゲットのTransform
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstructionMask;


    //----------------------ここまで変数宣言----------------------------------


    #region 初期化

    public override void Spawned()
    {
        //SetNickName($"Player({Object.InputAuthority.PlayerId})");


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


        TeleportToInitialSpawnPoint(); // 初期スポーンポイントにテレポートする

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

                if (weapon.weaponType == WeaponType.AssaultRifle)
                {
                    AssaultRifle rifle = weapon as AssaultRifle; //AssaultRifle型にキャスト
                    if (rifle != null)
                    {
                        rifle.muzzleTransform = tpsCameraTransform; //マズルのTransformをカメラのTransformに設定

                    }
                }
                else if (weapon.weaponType == WeaponType.SemiAutoRifle)
                {
                    SemiAutoRifle rifle = weapon as SemiAutoRifle; //SemiAutoRifle型にキャスト
                    if (rifle != null)
                    {
                        rifle.muzzleTransform = tpsCameraTransform; //マズルのTransformをカメラのTransformに設定
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



    public void TeleportToInitialSpawnPoint()
    {         // 初期スポーンポイントにテレポートする
        Vector3 initialSpawnPoint = new Vector3(UnityEngine.Random.Range(0, 5f), 1f, UnityEngine.Random.Range(0, 5f)); // 初期スポーンポイントの座標を設定
        transform.position = initialSpawnPoint; // プレイヤーの位置を初期スポーンポイントに設定
        characterController.enabled = false; // CharacterControllerを一時的に無効化
        characterController.enabled = true; // 再度有効化して、衝突判定をリセット
    }

    public void TeleportToInitialSpawnPoint(Vector3 initialSpawnPoint)
    {         // 初期スポーンポイントにテレポートする
        transform.position = initialSpawnPoint; // プレイヤーの位置を初期スポーンポイントに設定
        characterController.enabled = false; // CharacterControllerを一時的に無効化
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

            PlayerInputData localInputData = LocalInputHandler.CollectInput();

            ManageInputBufferDuration(localInputData); //入力バッファの持続時間を管理する




            bool isGrounded = characterController.isGrounded; // 接地判定を取得

            if (inputBuffer.jump && isGrounded)//ここに接地判定を追加
            {
                Jump();
                inputBuffer.jump = false; //ジャンプのバッファをクリア
            }
            isGroundedNow = characterController.isGrounded; // 現在の接地判定を取得
            if (!wasGrounded && isGroundedNow)
            {
                // 接地した瞬間に呼ばれる処理
                Land();
            }
            wasGrounded = isGroundedNow; // 前回の接地状態を更新


            //武器アクションの状態管理
            stateTimer_ReturnToIdle = Math.Max(stateTimer_ReturnToIdle - Time.deltaTime, 0); //状態タイマーを更新し、最大値を設定
            if (stateTimer_ReturnToIdle <= 0f) //状態タイマーが0以下になったら、Idle状態に戻す
            {
                currentWeaponActionState = WeaponActionState.Idle; //現在のアクションをIdleに設定
                isDuringWeaponAction = false; //武器アクション中フラグをfalseに設定
            }

            //武器切り替え
            //武器切り替え条件を満たしている場合、武器を切り替える

            if (CanChangeWeapon(localInputData, inputBuffer, currentWeaponActionState)) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
            {
                ChangeWeapon(localInputData.weaponChangeScroll);
                return;
            }

            // 接地判定
            CheckLand();


            //武器毎のUpdate処理
            if (weaponClassDictionary.TryGetValue(currentWeapon, out WeaponBase currentWeaponScript))
            {


                currentWeaponScript.CalledOnUpdate(localInputData, inputBuffer, currentWeaponActionState); //武器のUpdate処理を呼び出す

            }



            //if (currentWeapon == WeaponType.Sword)
            //{


            //    if (hasBufferedInput_swordFireDown)
            //    {
            //        // Swordの場合はFireDownは無視
            //        hasBufferedInput_swordFireDown = false; // SwordではFireDownのバッファをクリア

            //    }

            //}
            //else if (currentWeapon == WeaponType.AssaultRifle)
            //{
            //    if (!hasBufferedInput_swordFireDown)
            //    { 


            //    }

            //}
            //else
            //{
            //    if (localInputData.FirePressedDown) //発射ボタンが押されたら、武器の発射処理を呼ぶ
            //    {
            //        FireDown();
            //        //Debug.Log($"FirePressedDown {currentWeapon.GetName()}"); //デバッグログ
            //    }

            //}



            //if (localInputData.FirePressedStay) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ
            //{
            //    Fire();
            //    //Debug.Log($"FirePressedStay {currentWeapon.GetName()}"); //デバッグログ
            //}

            //if (localInputData.FirePressedUp) //発射ボタンが離されたら、武器の発射処理を呼ぶ
            //{
            //    if (currentWeapon != WeaponType.Grenade) // Swordの場合はFireUpは無視
            //    {
            //        FireUp();
            //        //Debug.Log($"FirePressedUp {currentWeapon.GetName()}"); //デバッグログ
            //    }



            //}

            //if (inputBuffer.reload) //リロードボタンが押されたら、武器のリロード処理を呼ぶ
            //{
            //    Reload();
            //    inputBuffer.reload = false; //リロードのバッファをクリア
            //}



            //if (localInputData.weaponChangeScroll != 0f) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
            //{
            //    ChangeWeapon(localInputData.weaponChangeScroll);
            //}


            //ExecuteWeaponActions(localInputData);

            if (localInputData.ADSPressedDown) //ADSボタンが押されたら、ADSの処理を呼ぶ
            {
                SwitchADS();
                Debug.Log($"ADSPressedDown"); //デバッグログ
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





        if (inputBuffer.grenadeFireUp)
        {
            inputBufferTimer_fireUp += Time.deltaTime;
            if (inputBufferTimer_fireUp >= inputBufferDuration_fireUp)
            {
                inputBuffer.grenadeFireUp = false; //バッファをクリア
                inputBufferTimer_fireUp = 0f; //タイマーをリセット
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
        if (localInputData.FirePressedUp) //発射ボタンが離されたら、バッファに追加
        {
            inputBuffer.grenadeFireUp = true;
            inputBufferTimer_fireUp = 0f; //タイマーをリセット
        }


    }

    void ExecuteBufferedActions()
    {
        //バッファされたアクションを実行する
        if (inputBuffer.jump && characterController.isGrounded)
        {
            Jump();
            inputBuffer.jump = false; //バッファをクリア
            inputBufferTimer_jump = 0f; //タイマーをリセット
        }
        if (inputBuffer.swordFireDown && IsViableActionState())
        {
            FireDown();
            inputBuffer.swordFireDown = false; //バッファをクリア
            inputBufferTimer_swordFireDown = 0f; //タイマーをリセット
        }
        if (inputBuffer.grenadeFireUp && IsViableActionState())
        {
            FireUp();
            inputBuffer.grenadeFireUp = false; //バッファをクリア
            inputBufferTimer_fireUp = 0f; //タイマーをリセット
        }
        if (inputBuffer.reload && IsViableActionState())
        {
            Reload();
            inputBuffer.reload = false; //バッファをクリア
            inputBufferTimer_reload = 0f; //タイマーをリセット

        }
    }


    //排他的な武器アクションの実行を行う
    void ExecuteWeaponActions(PlayerInputData localInputData)
    {
        //武器切り替え

        if (localInputData.weaponChangeScroll != 0f && IsViableActionState()) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
        {
            ChangeWeapon(localInputData.weaponChangeScroll);
            return;
        }


        //リロード

        if (currentWeapon.IsReloadable())
        {
            if (inputBuffer.reload && IsViableActionState())
            {
                Reload();
                inputBuffer.reload = false; //バッファをクリア
                inputBufferTimer_reload = 0f; //タイマーをリセット
                return;
            }
        }


        //射撃

        if (currentWeapon == WeaponType.Sword)
        {
            if (inputBuffer.swordFireDown && IsViableActionState())
            {
                FireDown();
                inputBuffer.swordFireDown = false; //バッファをクリア
                inputBufferTimer_swordFireDown = 0f; //タイマーをリセット
            }

        }
        else if (currentWeapon == WeaponType.AssaultRifle)
        {
            if (inputBuffer.assaultFire && IsViableActionState())
            {
                FireDown();
            }
            if (localInputData.FirePressedStay && IsViableActionState()) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ

            {
                Fire();
                //Debug.Log($"FirePressedStay {currentWeapon.GetName()}"); //デバッグログ
            }
            if (inputBuffer.grenadeFireUp && IsViableActionState())
            {
                FireUp();
                inputBuffer.grenadeFireUp = false; //バッファをクリア
                inputBufferTimer_fireUp = 0f; //タイマーをリセット
            }

        }
        else if (currentWeapon == WeaponType.Grenade)
        {
            if (inputBuffer.grenadeFireUp && IsViableActionState())
            {
                FireUp();
                inputBuffer.grenadeFireUp = false; //バッファをクリア
                inputBufferTimer_fireUp = 0f; //タイマーをリセット
            }
        }
        else
        {
            if (localInputData.FirePressedDown) //発射ボタンが押されたら、武器の発射処理を呼ぶ
            {
                FireDown();
                //Debug.Log($"FirePressedDown {currentWeapon.GetName()}"); //デバッグログ
            }




            if (localInputData.FirePressedStay) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ
            {
                Fire();
                //Debug.Log($"FirePressedStay {currentWeapon.GetName()}"); //デバッグログ
            }

            if (localInputData.FirePressedUp) //発射ボタンが離されたら、武器の発射処理を呼ぶ
            {
                if (currentWeapon != WeaponType.Grenade) // Swordの場合はFireUpは無視
                {
                    FireUp();
                    //Debug.Log($"FirePressedUp {currentWeapon.GetName()}"); //デバッグログ
                }
            }
        }
    }


    bool IsViableActionState()
    {
        //行動可能かどうかを判定する
        return !isDuringWeaponAction && !isImmobilized; //武器アクション中、行動不能中は行動不可
    }


    bool CanChangeWeapon(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition = (localInputData.weaponChangeScroll != 0f);

        bool stateCondition =
            currentAction != WeaponActionState.SwordAttacking &
            currentAction != WeaponActionState.GrenadeThrowing &
            currentAction != WeaponActionState.Dead;


        return inputCondition && stateCondition;




            isGroundedNow = characterController.isGrounded; // 現在の接地判定を取得
            if (!wasGrounded && isGroundedNow)
            {
                // 接地した瞬間に呼ばれる処理
                Land();
            }
            wasGrounded = isGroundedNow; // 前回の接地状態を更新



            ChangeTransformLocally(normalizedInputDirection, tpsCameraTransform.forward);//ジャンプによる初速度も考慮して移動する



    }

    #endregion


    #region transform変化


    //移動方向と向きたい前方向を元に、ローカルプレイヤーのTransformを変更する

    public void ChangeTransformLocally(Vector3 normalizedInputDir, Vector3 lookForwardDir)
    {

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
            if (isHoming)
            {
                Homing(bodyObject.transform.position, currentTargetTransform); //ホーミング中はターゲットに向かって移動する

            }
            else
            {
                //コルーチン(RotateToCameraOverTime(float duration))で、カメラの前方向に徐々に向きを合わせる
            }
        }

        Vector3 moveDirection = Vector3.zero; // 初期化

        if (isImmobilized)
        {
            // 行動不能中は移動方向をゼロにする
            moveDirection = Vector3.zero;

        }
        else
        {
            // 入力方向に基づいて移動方向を計算
            moveDirection = Quaternion.LookRotation(bodyObject.transform.forward, Vector3.up) * normalizedInputDir;
        }



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


        Collider[] hits = Physics.OverlapSphere(origin, range, playerLayer); // プレイヤーのレイヤーマスクを使用して近くの敵を検出
        float closestDistance = Mathf.Infinity;
        targetTransform = null;
        //float minDistance = Mathf.Infinity;




        foreach (Collider hit in hits)
        {
            Transform enemyTransform = hit.transform;
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
                    continue; // PlayerAvatarコンポーネントがない場合はスキップ
                }
                //continue; // PlayerAvatarコンポーネントがない場合はスキップ
            }


            Vector3 toEnemy = (enemyTransform.position - origin);
            float angle = Vector3.Angle(forward, toEnemy.normalized); // 必要に応じて forward をプレイヤーの forward に変更
            float distance = toEnemy.magnitude;



            if (angle > fovAngle / 2f || distance > range)
            {
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
                continue;
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                targetTransform = enemyTransform;
            }


        }

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

            bodyObject.transform.forward = toTarget; // プレイヤーの向きをターゲット方向に設定

            homingMoveDirection = toTarget; // 現在の移動方向を更新

            StartCoroutine(HomingCoroutine()); // ホーミング時間の管理を開始


        }
    }


    void Homing(Vector3 attackerTransform, Transform targetTransform)
    {


        Vector3 toTarget = (targetTransform.position - attackerTransform);
        toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
        toTarget.Normalize(); // 正規化して方向ベクトルにする

        // 今の移動方向から敵への方向との差を角度で計算
        float angleToTarget = Vector3.Angle(homingMoveDirection, toTarget);

        // 角度差がある場合は補正
        if (angleToTarget > 0f)
        {
            // 回転角の制限（最大maxTurnAnglePerFrame度）
            float t = Mathf.Min(1f, maxTurnAnglePerFrame / angleToTarget);
            homingMoveDirection = Vector3.Slerp(homingMoveDirection, toTarget, t);
        }

        // 移動
        characterController.Move(homingMoveDirection * chaseSpeed * Time.deltaTime);
        //transform.position += homingMoveDirection * chaseSpeed * Time.deltaTime;

        // 向きを移動方向に合わせる（任意）
        bodyObject.transform.forward = homingMoveDirection;

        //Debug.Log($"Homing towards {targetTransform.name}");



    }

    //ホーミング時間の管理
    IEnumerator HomingCoroutine()
    {
        yield return new WaitForSeconds(homingTime);

        isHoming = false; // ホーミングを終了
    }




    //攻撃後の硬直時間の管理
    IEnumerator PostAttackDelayCoroutine()
    {
        yield return new WaitForSeconds(attackImmolizedTime);

        isImmobilized = false; // 行動不能を解除

        isDuringWeaponAction = false; // 武器アクション中フラグを解除
        //currentWeaponActionState = WeaponActionState.Idle; // 現在のアクションをIdleに設定
        //キャラの向きをカメラの向きに徐々に戻す
        StartCoroutine(RotateToCameraOverTime(rotationDuration));

    }

    //0.1秒かけてカメラの前方向に向くコルーチン
    private IEnumerator RotateToCameraOverTime(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
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

            elapsed += Time.deltaTime;
            yield return null;
        }
        isFollowingCameraForward = true; // カメラの向きに追従するように設定
    }

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


        isGroundedNow = CheckGround(); // 接地判定を行う

        if (isGroundedNow) 
        {

            SetActionAnimationPlayListForAllClients(ActionType.Jump); //アクションアニメーションのリストに追加
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); //ローカルでジャンプの初速度(velocity.y)を与える
        }
    }

    bool CheckGround()
    {
        Vector3 origin = transform.position; // キャラクターの中心位置を基準にする
        float rayLength = 1.5f; // レイの長さを設定
        return Physics.Raycast(origin, Vector3.down, rayLength);
    }

    void CheckLand()
    {
        isGroundedNow = CheckGround(); // 接地判定を行う

        if (!wasGrounded && isGroundedNow)
        {
            Land();
        }

        wasGrounded = isGroundedNow; // 前回の接地状態を更新
    }

    void Land()
    {
        if (!wasGrounded && isGroundedNow)
        {
            SetActionAnimationPlayListForAllClients(ActionType.Land);
        }
    }

    #endregion


    #region Weapon関連



    public void SwordAction()
    {
        //近接武器の処理
        isImmobilized = true; //行動不能
        isDuringWeaponAction = true;
        currentWeaponActionState = WeaponActionState.SwordAttacking; //武器アクション状態を近接攻撃に設定
        stateTimer_ReturnToIdle= attackImmolizedTime; //近接攻撃の待機時間を設定

        isFollowingCameraForward = false; // カメラの向きに追従しないように設定

        //攻撃の当たり判定やアニメーションは変わらないので共通
        //weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

        StartCoroutine(PostAttackDelayCoroutine()); //攻撃後の硬直時間を管理するコルーチンを開始

        if (TryGetClosestTargetInRange(headObject.transform.position, bodyObject.transform.forward, chaseRange, chaseAngle, out Transform targetTransform))
        {
            // 近くに敵がいる場合
            currentTargetTransform = targetTransform; // 現在のターゲットを設定
            StartHoming(currentTargetTransform); //ホーミング開始
            Debug.Log($"Homing started towards {currentTargetTransform.name} from {headObject.transform.position}");

        }
        else
        {
            // 近くに敵がいない場合、ホーミングせずその場で攻撃

            Debug.Log("No target found for homing. Attacking in place.");


        }


    }

    public void FireAction()
    {
        currentWeaponActionState = WeaponActionState.Firing; //武器アクション状態を射撃に設定
        //isDuringWeaponAction = true;

        weaponClassDictionary[currentWeapon].Fire(); //現在の武器の発射処理を呼ぶ
        SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

        if (currentWeapon.RecoilAmount_Pitch() > 0f || currentWeapon.RecoilAmount_Yaw() > 0f)
        { tpsCameraController.StartRecoil(currentWeapon); }//リコイル開始

        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火

        //StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射ダウンのコルーチンを開始
        stateTimer_ReturnToIdle = currentWeapon.FireWaitTime(); //発射ダウンの時間を設定
    }

    //単発武器の射撃。マガジンが空ならリロードする
    void FireDown()
    {
        if (!CanWeaponAction()) { return; } //発射可能かどうかを判定

        Debug.Log($"FireDown {currentWeapon.GetName()}"); //デバッグログ

        if (currentWeapon == WeaponType.Sword)
        {
            SwordAction(); //近接武器の処理を呼ぶ
        }
        else if (currentWeapon == WeaponType.Grenade)
        {
            weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
            SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加
        }
        else if (currentWeapon == WeaponType.SemiAutoRifle)
        {







            //遠距離武器の処理
            if (weaponClassDictionary[currentWeapon].IsMagazineEmpty())
            {
               // Reload(); //マガジンが空ならリロードする
            }
            else
            {

                isDuringWeaponAction = true;
                weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
                SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

                if (currentWeapon.RecoilAmount_Pitch() > 0f || currentWeapon.RecoilAmount_Yaw() > 0f) tpsCameraController.StartRecoil(currentWeapon);//リコイル開始

                OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火

                StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射ダウンのコルーチンを開始
                                                                           //Debug.Log($"FireDown {currentWeapon.GetName()}"); //デバッグログ
            }

        }
        else if (currentWeapon == WeaponType.AssaultRifle)
        { 
        
        
        }











            /////
            ///

            //    if (CanWeaponAction()) //発射可能かどうかを判定
            //{
            //    if (currentWeapon != WeaponType.Sword)
            //    {
            //        //遠距離武器の処理
            //        if (weaponClassDictionary[currentWeapon].IsMagazineEmpty())
            //        {
            //            Reload(); //マガジンが空ならリロードする
            //        }
            //        else
            //        {

            //            isDuringWeaponAction = true;
            //            weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
            //            SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加

            //            if (currentWeapon.RecoilAmount_Pitch() > 0f || currentWeapon.RecoilAmount_Yaw() > 0f) tpsCameraController.StartRecoil(currentWeapon);//リコイル開始

            //            OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火

            //            StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射ダウンのコルーチンを開始
            //                                                                       //Debug.Log($"FireDown {currentWeapon.GetName()}"); //デバッグログ
            //        }

            //    }
            //    else
            //    {




            //    }
            //}

            //else
            //{
            //    //Debug.Log($"Cannot fire {currentWeapon.GetName()}: Magazine is empty or not ready to fire.");
            //}

    }

    //連射武器の射撃。マガジンが空でもリロードしない

    public void Fire()
    {
        

            isDuringWeaponAction = true;
            currentWeaponActionState = WeaponActionState.Firing; //武器アクション状態を射撃に設定

            weaponClassDictionary[currentWeapon].Fire(); //現在の武器の発射処理を呼ぶ

            tpsCameraController.StartRecoil(currentWeapon); //リコイル開始


            //アクションアニメーションのリストに発射を追加
            SetActionAnimationPlayListForAllClients(currentWeapon.FireDownAction()); //アクションアニメーションのリストに発射ダウンを追加


            OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //弾薬変更イベントを発火
            //StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射ダウンのコルーチンを開始

            stateTimer_ReturnToIdle = currentWeapon.FireWaitTime(); //発射後の待機時間を設定

       

}



    void FireUp()
    {




        if (CanWeaponAction()) { return; }//連射武器で、かつ発射可能な場合
        Debug.Log($"FireUp {currentWeapon.GetName()}"); //デバッグログ

        if (currentWeapon==WeaponType.Grenade)
            { 
            
                //グレネードの処理
                
                weaponClassDictionary[currentWeapon].FireUp(); //現在の武器の発射処理を呼ぶ
                SetActionAnimationPlayListForAllClients(currentWeapon.FireUpAction()); //アクションアニメーションのリストに発射アップを追加
                //StartCoroutine(FireRoutine(currentWeapon.FireWaitTime())); //発射アップのコルーチンを開始
                //tpsCameraController.EndFiring(); //TPSカメラの発射終了処理を呼ぶ
                Debug.Log($"FireUp {currentWeapon.GetName()}"); //デバッグログ


            }




            if (!currentWeapon.isOneShotWeapon())
            {

                SetActionAnimationPlayListForAllClients(currentWeapon.FireUpAction()); //アクションアニメーションのリストに発射ダウンを追加
                tpsCameraController.EndFiring();



                //ここにもコルーチンつける？→一旦つけない
                //Debug.Log($"FireUp {currentWeapon.GetName()}"); //デバッグログ
            }
        
    }

    //リロード処理
    //boolを返す

    public void Reload()
    {
        
            isDuringWeaponAction = true; //リロード中フラグを立てる
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
                isDuringWeaponAction = true; //武器変更中フラグを立てる

                weaponClassDictionary[currentWeapon].ResetOnChangeWeapon(); 




                if (currentWeapon.CanADS())
                { 
                    CancelADS(); //現在の武器がADS可能ならADSをキャンセル
                }

                currentWeapon = newWeaponType;

                Debug.Log($"ChangingWeapon {currentWeapon.GetName()}"); //デバッグログ

                OnWeaponChanged?.Invoke(currentWeapon, weaponClassDictionary[currentWeapon].currentMagazine, weaponClassDictionary[currentWeapon].currentReserve); //武器変更イベントを発火
                //OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].CurrentMagazine, weaponClassDictionary[currentWeapon].CurrentReserve); //弾薬変更イベントを発火


                SetActionAnimationPlayListForAllClients(currentWeapon.ChangeWeaponAction()); //アクションアニメーションのリストに武器変更を追加
                //StartCoroutine(ChangeWeaponRoutine(currentWeapon.WeaponChangeTime())); //武器変更処理をコルーチンで実行
                stateTimer_ReturnToIdle = currentWeapon.WeaponChangeTime(); //武器変更後の待機時間を設定

            }
            else
            {
                Debug.LogWarning($"Weapon {newWeaponType} not found in weapon dictionary.");
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


    bool CanWeaponAction()
    {
        //武器の発射可能かどうかを判定する処理を追加する
        //例えば、リロード中や武器変更中は発射できないなど
        if ( isDuringWeaponAction || isImmobilized)
        {
            return false;
        }
      
            return true;
           
    }


    private IEnumerator FireRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //localState.SetAmmo(weaponType, localState.GetMaxAmmo(weaponType));



        isDuringWeaponAction = false;
        
    }


    //コルーチン
    private IEnumerator　ReloadRoutine(WeaponType reloadedWeaponType, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //localState.SetAmmo(weaponType, localState.GetMaxAmmo(weaponType));

        weaponClassDictionary[reloadedWeaponType].FinishReload(); //リロード処理を呼ぶ

        OnAmmoChanged?.Invoke(weaponClassDictionary[reloadedWeaponType].currentMagazine, weaponClassDictionary[reloadedWeaponType].currentReserve); //弾薬変更イベントを発火

        //isDuringWeaponAction = false;
        Debug.Log("リロード完了！");
    }

    //コルーチン
    //コルーチン
    private IEnumerator ChangeWeaponRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //localState.SetAmmo(weaponType, localState.GetMaxAmmo(weaponType));



        isDuringWeaponAction = false;
        Debug.Log("武器切り替え完了！");
    }


    public void SwitchADS()
    {

        if (currentWeapon.CanADS() )
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
        else 
        {             //現在の武器がADSできない場合は何もしない
            Debug.Log($"Weapon {currentWeapon.GetName()} cannot ADS.");
        }
    }

  

    void CancelADS()
    {
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




    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestActionAnimation(ActionType actionType, float calledTime, RpcInfo info = default)
    {
        // RPC送信（即送信）
       // Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {calledTime}");
        RPC_ApplyActionAnimation(info.Source,actionType, calledTime); //アクションアニメーションのリストに追加するだけ(接地判定も座標変化もしない)


    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyActionAnimation(PlayerRef sourcePlayer, ActionType actionType, float calledTime, RpcInfo info = default)
    {
        //Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        //Debug.Log($"SourcePlayer {sourcePlayer}");
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