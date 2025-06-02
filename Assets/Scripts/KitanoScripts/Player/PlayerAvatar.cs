using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;

using Unity;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.Examples.ObjectSpin;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private GameObject headObject;
    [SerializeField] private GameObject bodyObject;

    CharacterController characterController;
    //WeaponHandler weaponHandler;
    private PlayerNetworkState playerNetworkState;
    private Transform tpsCameraTransform;


    [SerializeField]
    private PlayerHitbox myPlayerHitbox;

    // プレイヤーの身体能力を設定するための変数群
    public float moveSpeed = 3f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;

    private Vector3 velocity; //主に重力に使用


    [SerializeField]
    private Transform cameraTarget;
    public Transform CameraTarget => cameraTarget;


    [SerializeField] private Transform hostTransform;

    public List<ActionStruct> actionAnimationPlayList=new List<ActionStruct> { }   ;  //再生すべきアクションアニメーションのリスト

    public Vector3 normalizedInputDirection=Vector3.zero; //入力方向の正規化されたベクトル


    [Networked] public Vector3 avatarPositionInHost { get; set; } = Vector3.zero; //ホスト環境でのアバター位置(入力権限のあるプレイヤーの位置を参照するために使用)
    [Networked] public Vector3 cameraForwardInHost { get; set; } = Vector3.zero; //カメラの向き(入力権限のあるプレイヤーの回転を参照するために使用)
    [Networked] public Vector3 normalizedInputDirectionInHost { get; set; } = Vector3.zero; //入力権限のあるプレイヤーの入力方向を参照するために使用


    //行動可能かどうかのフラグ
    private bool isDuringWeaponAction = false; //武器アクション(射撃、リロード、武器切り替え)中かどうか
    private bool isImmobilized = false; //行動不能中かどうか(移動・ジャンプもできない)



    //Weapon関連
    private WeaponType currentWeapon = WeaponType.AssaultRifle ; //現在の武器タイプ
    [SerializeField]private Dictionary<WeaponType, WeaponBase> weaponClassDictionary = new Dictionary<WeaponType, WeaponBase>(); //武器タイプと武器の対応関係を保持する辞書
    //[SerializeField] private Dictionary<WeaponType, float> reloadTimeDictionary= new Dictionary<WeaponType, float>(); //武器タイプとリロード時間の対応関係を保持する辞書
    [SerializeField] private float weaponChangeTime = 0.5f; //武器変更時間(秒)

    // クラス生成・弾薬データ変更時のイベント
    public static event Action<PlayerAvatar> OnWeaponSpawned;
    public event Action<int, int> OnAmmoChanged;
    public event Action<WeaponType> OnWeaponChanged;

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

        //ApplyHostTransform();

        characterController.Move(Vector3.zero); //初期位置での移動を防ぐために、初期位置でMoveを呼ぶ

        SetWeapons(); //武器の初期化を行う

    }

    void SetWeapons()
    {
        WeaponBase[] weaponScripts = GetComponentsInChildren<WeaponBase>(includeInactive: true);

        foreach (WeaponBase weapon in weaponScripts)
        {
            if (!weaponClassDictionary.ContainsKey(weapon.weaponType))
            {
                weaponClassDictionary.Add(weapon.weaponType, weapon);
                weapon.CurrentMagazine = weapon.weaponType.MagazineCapacity(); //初期マガジンは最大値に設定
                weapon.CurrentReserve = 50; //初期リザーブは50
            }
            else
            {
                Debug.LogWarning($"Weapon {weapon.weaponType} already exists in the dictionary.");
            }
        }

        OnWeaponSpawned?.Invoke(this); //武器生成イベントを発火



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
            TryFireDown();
            Debug.Log($"FirePressedDown {currentWeapon.GetName()}"); //デバッグログ
        }

        if (localInputData.FirePressedStay) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ
        {
            TryFire();
            Debug.Log($"FirePressedStay {currentWeapon.GetName()}"); //デバッグログ
        }

        if (localInputData.FirePressedUp) //発射ボタンが押され続けている間、武器の発射処理を呼ぶ
        {
            FireUp();
        }

        if (localInputData.ReloadPressedDown) //リロードボタンが押されたら、武器のリロード処理を呼ぶ
        {
            TryReload();
        }



        if (localInputData.weaponChangeScroll != 0f) //武器変更のスクロールがあれば、武器の変更処理を呼ぶ
        {
            TryChangeWeapon(localInputData.weaponChangeScroll);
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

    //単発武器の射撃
    void TryFireDown()
    {

        if (CanFire()) //発射可能かどうかを判定
        { 
            FireDown(); //現在の武器の発射処理を呼ぶ


            //SetActionAnimationPlayList(ActionType.Fire, Runner.SimulationTime); //アクションアニメーションのリストに射撃を追加
            Debug.Log($"FireDown {currentWeapon.GetName()}"); //デバッグログ

        }
        else
        {
            Debug.Log($"Cannot fire {currentWeapon.GetName()}: Magazine is empty or not ready to fire.");
        }

    }

    //連射武器の射撃

    void TryFire()
    {
        if (CanFire()  && !currentWeapon.isOneShotWeapon()) //連射武器で、かつ発射可能な場合
        {
            Fire(); //現在の武器の発射処理を呼ぶ
            //weaponClassDictionary[currentWeapon].Fire(); //現在の武器の発射処理を呼ぶ
            Debug.Log($"FireStay {currentWeapon.GetName()}"); //デバッグログ

        }

     
    }
    void FireDown()
    {

        weaponClassDictionary[currentWeapon].FireDown(); //現在の武器の発射処理を呼ぶ
        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].CurrentMagazine, weaponClassDictionary[currentWeapon].CurrentReserve); //弾薬変更イベントを発火
       
    }

    void Fire()
    {
        
        
        weaponClassDictionary[currentWeapon].Fire(); //現在の武器の発射処理を呼ぶ
        OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].CurrentMagazine, weaponClassDictionary[currentWeapon].CurrentReserve); //弾薬変更イベントを発火
        
    }


    void FireUp()
    {
        if (currentWeapon == WeaponType.AssaultRifle)
        {
            SetActionAnimationPlayList(ActionType.FireEnd_AssaultRifle , Runner.SimulationTime);
        }


        //連射武器の発射ボタンを離したときの処理は特にないので、何もしない
        //もし必要なら、ここで連射武器の発射を止める処理を追加することができる
    }



    void TryReload()
    {
        //武器のリロード処理を呼ぶ
        if (CanReload())
        {
            Reload();
        }
        else
        {
            Debug.Log("Cannot reload now."); //リロードできない場合のデバッグログ
        }
    }

    void TryChangeWeapon(float scrollValue)
    {
        if (CanChangeWeapon())
        {
            //武器の変更可能かどうかを判定
            if (scrollValue > 0f) //スクロールアップなら次の武器に変更
            {
                int weaponCount= Enum.GetValues(typeof(WeaponType)).Length; //武器の総数を取得  
                ChangeWeapon((WeaponType)(((int)currentWeapon + 1 + weaponCount) % weaponCount));
            }
            else if (scrollValue < 0f) //スクロールダウンなら前の武器に変更
            {
                int weaponCount = Enum.GetValues(typeof(WeaponType)).Length; //武器の総数を取得  
                ChangeWeapon((WeaponType)(((int)currentWeapon - 1 + weaponCount) % weaponCount));
            }
        }
        else
        {
            Debug.Log("Cannot change weapon now."); //武器変更できない場合のデバッグログ
        }

    }
    bool CanFire()
    {
        //武器の発射可能かどうかを判定する処理を追加する
        //例えば、リロード中や武器変更中は発射できないなど
        if ( isDuringWeaponAction || isImmobilized)
        {
            return false;
        }
        else if (weaponClassDictionary[currentWeapon].CurrentMagazine <= 0)
        { 
            return false; //現在の武器の弾薬がない場合は発射できない
            Debug.Log($"Cannot fire {currentWeapon.GetName()}: Magazine is empty.");
        }
            return true;
            Debug.Log($"Can fire {currentWeapon.GetName()}: Magazine: {weaponClassDictionary[currentWeapon].CurrentMagazine}, Reserve: {weaponClassDictionary[currentWeapon].CurrentReserve}");
    }

    bool CanReload()
    {
        //武器のリロード可能かどうかを判定する処理を追加する
        //例えば、リロード中や武器変更中はリロードできないなど
        if (isDuringWeaponAction || isImmobilized)
        {
            return false;
        }
        else if (weaponClassDictionary[currentWeapon].CurrentMagazine >= currentWeapon.MagazineCapacity())
        {
            return false; //現在の武器のマガジンが満タンの場合はリロードできない
        }
        return true;
    }

    bool CanChangeWeapon()
    {
        //武器の変更可能かどうかを判定する処理を追加する
        //例えば、リロード中や武器変更中は変更できないなど
        if (isDuringWeaponAction || isImmobilized)
        {
            return false;
        }
        return true;
    }

      //Fire()はWeaponBaseクラスのメソッドとして実装している

    void Reload()
    { 
        isDuringWeaponAction = true; //リロード中フラグを立てる
        StartCoroutine(ReloadRoutine(currentWeapon, currentWeapon.ReloadTime())); //リロード処理をコルーチンで実行
    }

    void ChangeWeapon(WeaponType newWeaponType)
    {
        if (weaponClassDictionary.ContainsKey(newWeaponType))
        {
            isDuringWeaponAction = true; //武器変更中フラグを立てる
            currentWeapon = newWeaponType;
            
            OnWeaponChanged?.Invoke(currentWeapon); //武器変更イベントを発火
            OnAmmoChanged?.Invoke(weaponClassDictionary[currentWeapon].CurrentMagazine, weaponClassDictionary[currentWeapon].CurrentReserve); //弾薬変更イベントを発火


            //SetActionAnimationPlayList(.ChangeWeaponTo_ , Runner.SimulationTime); //アクションアニメーションのリストに武器変更を追加
            StartCoroutine(ChangeWeaponRoutine(weaponChangeTime)); //武器変更処理をコルーチンで実行

        }
        else
        {
            Debug.LogWarning($"Weapon {newWeaponType} not found in weapon dictionary.");
        }
    }


    //コルーチン
    private IEnumerator　ReloadRoutine(WeaponType reloadedWeaponType, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //localState.SetAmmo(weaponType, localState.GetMaxAmmo(weaponType));

        int currentMagazine = weaponClassDictionary[reloadedWeaponType].CurrentMagazine;
        int currentReserve = weaponClassDictionary[reloadedWeaponType].CurrentReserve;
        int magazineCapacity = reloadedWeaponType.MagazineCapacity();
        int reloadededAmmo = Mathf.Min(currentReserve, magazineCapacity - currentMagazine); //リロードされる弾薬数

        weaponClassDictionary[reloadedWeaponType].CurrentMagazine += reloadededAmmo; //マガジンにリロードされた弾薬を追加
        weaponClassDictionary[reloadedWeaponType].CurrentReserve -= reloadededAmmo; //リザーブからリロードされた弾薬を減らす


        isDuringWeaponAction = false;

        OnAmmoChanged?.Invoke(weaponClassDictionary[reloadedWeaponType].CurrentMagazine, weaponClassDictionary[reloadedWeaponType].CurrentReserve); //弾薬変更イベントを発火
        Debug.Log("リロード完了！");
    }

    //コルーチン
    private IEnumerator ChangeWeaponRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        //localState.SetAmmo(weaponType, localState.GetMaxAmmo(weaponType));


        isDuringWeaponAction = false;
        Debug.Log("武器切り替え完了！");
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


}