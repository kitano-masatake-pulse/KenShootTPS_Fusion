using Cinemachine;
using Fusion;
using RootMotion.Demos;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

//CinemachineVirtualCameraにアタッチすること
public class TPSCameraController : MonoBehaviour
{



    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    Vector3 initialShoulderOffset=Vector3.zero; // 初期のショルダーオフセット

    [Header("マウス操作")]



    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 75f;
    private float yaw = 0f;
    private float pitch = 0f;
    public Transform cameraTarget;

    bool isSetCameraTarget=true;
    bool cursorLocked = true;

    //リコイル関連の変数(すべてdegree　かつ　マウス操作でのカメラ角度を0としたときの差分)

    private float currentRecoil_Pitch = 0f; // 現在のリコイル角度（ピッチ）
    private float currentRecoil_Yaw = 0f; // 現在のリコイル角度（ヨー）
    private float recoilTarget_Pitch = 0f; // リコイルの目標角度（ピッチ）
    private float recoilTarget_Yaw = 0f; // リコイルの目標角度（ヨー）
    private float recoverTarget_Pitch = 0f; // リコイル回復の目標角度(ピッチ)
    private float recoverTarget_Yaw = 0f; // リコイル回復の目標角度(ヨー)

    bool isRecoiling = false; // リコイル中かどうかのフラグ
    bool isRecovering = false; // リコイル回復中かどうかのフラグ



    [Header("障害物判定")]
    [SerializeField] private LayerMask obstacleLayer; // 障害物のレイヤー
    [SerializeField] private float obstacleBuffer = 0.05f; // 障害物からの距離

    [Header("リコイルのdebug用")]
    //リコイルのdebug用
    public float  debug_recoilAmount_Pitch= 5f; // リコイルの角度（ピッチ）
    public float debug_recoilSpeed_Pitch = 100f; // リコイルの角速度(ピッチ）
    public float debug_recoverSpeed_Pitch = 50f; // リコイル回復の角速度(ピッチ）
    public float debug_recoilLimit_Pitch = 30f; // リコイルの角度制限（ピッチ）

    public float debug_recoilAmount_Yaw = 0.5f; // リコイルの角度（ヨー）
    public float debug_recoilSpeed_Yaw=10f;
    public float debug_recoverSpeed_Yaw=5f;
    public float debug_recoilLimit_Yaw = 3f;

    // 各武器タイプのリコイル乱数パターン（ヨー）
    Dictionary<WeaponType, List<float>> recoilRandomPatterns_Yaw = new Dictionary<WeaponType, List<float>>(); 

    int seed_recoilYaw = 8997; // リコイルの乱数パターンのシード値（ヨー）


    [SerializeField]bool isDebugParameter = false; // デバッグモードかどうかのフラグ
    int recoilPatternIndex=0; // リコイルパターンのインデックス（ヨー）


    #region ADS関連

    [Header("References")]
    private Cinemachine3rdPersonFollow thirdPersonFollow;

    [Header("Camera Transition Settings")]
    [SerializeField] private Vector3 normalShoulderOffset = new Vector3(0.5f, 0, 0f);
    [SerializeField] private Vector3 adsShoulderOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float normalDistance = 3.5f;
    [SerializeField] private float adsDistance = 2.0f;
    [SerializeField] private float offsetLerpSpeed = 10f;

    [Header("FOV Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float adsFOV = 30f;
    [SerializeField] private float fovLerpSpeed = 15f;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Vector2 normalSensitivity = new Vector2(3.0f,1.5f);
    [SerializeField] private Vector2 adsSensitivity = new Vector2(1.5f, 0.75f);
    private Vector2 currentSensitivity;
    [SerializeField] private float mouseSenRange = 4.0f; // マウス感度の範囲 
    // マウス感度（X, Y）を補正する変数
    private float sensitivityMultiplier;
    // X・Y軸の反転（1: 正常, -1: 反転）
    private int directionX = 1; 
    private int directionY = 1;


    [SerializeField]float ADSRecoilMultiplier = 0.5f; // ADS時のリコイル倍率
    float currentRecoilMultiplier = 1f; // 現在のリコイル倍率


    private Vector3 targetOffset;
    private float targetDistance;
    private float targetFOV;

    private bool isADS = false;

    WeaponType currentRecoilingWeapon= WeaponType.AssaultRifle; // 現在リコイル中の武器タイプ（仮にAssaultRifleを使用）



    CameraInputData cameraInputData; // カメラ入力データ

    #endregion
    private void OnEnable()
    {
        OptionsManager.Instance.OnApplied -= UpdateMouseOption; // オプション適用イベントを購読解除
        OptionsManager.Instance.OnApplied += UpdateMouseOption; // オプション適用イベントを購読

    }
    private void OnDisable()
    {
        OptionsManager.Instance.OnApplied -= UpdateMouseOption; // オプション適用イベントを購読解除
    }


    void Start()
    {
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();


        

        InitializeADS(); // ADSの初期化

        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
           
                // 各武器タイプのリコイル乱数パターンを生成
                recoilRandomPatterns_Yaw[type] = GenerateRandomPattern(seed_recoilYaw, type.MagazineCapacity(), -1f, 1f); 
            
        }

    }
    void Update()
    {

        cameraInputData = CameraInputData.Default();
        if (!LocalInputHandler.isOpenMenu) { 
           cameraInputData = LocalInputHandler.CollectCameraInput(); // カメラ入力データを取得
        }
        //ApplyRecoil(WeaponType.AssaultRifle); // 仮の武器タイプを指定。実際には現在の武器タイプに応じて変更する必要があります
        ApplyRecoil(currentRecoilingWeapon); //リコイルの適用

        if ( isSetCameraTarget)
           {
            float mouseX = cameraInputData.mouseMovement.x * currentSensitivity.x * sensitivityMultiplier*directionX;
            float mouseY = cameraInputData.mouseMovement.y * currentSensitivity.y * sensitivityMultiplier*directionY;

            //Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}"); // デバッグ用ログ出力
            yaw += mouseX;
            pitch -= mouseY;

            //Debug.Log($"yaw: {yaw}, pitch: {pitch}"); // デバッグ用ログ出力

            //ApplyRecoil(currentRecoilingWeapon); //リコイルの適用

            

            //yaw +=; // リコイルのヨーを適用
            //pitch -= currentRecoil_Pitch; // リコイルのピッチを適用

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch - currentRecoil_Pitch, yaw + currentRecoil_Yaw, 0f);
            cameraTarget.rotation = rotation;

            //myPlayerAvatar.ChangeTransformLocally(); // アバターの向きをカメラ方向に合わせる処理を呼び出す
            if (thirdPersonFollow == null) return;



            

            ADStransition(); // ADSの補間処理を呼び出す

            
        }

    }

    #region マウス感度調整
    public void UpdateMouseOption(OptionData data)
    {  
        float mouseSensitivity = data.mouseSensitivity;
        //対数でマウス感度を1/4～4倍に調整(デフォルト値)
        sensitivityMultiplier = Mathf.Pow(mouseSenRange, mouseSensitivity);

        bool inverX = data.invertX; 
        bool inverY = data.invertY;
        directionX = inverX ? -1 : 1; 
        directionY = inverY ? -1 : 1;
    }
    #endregion

    #region　リコイル関連  

    public void ApplyRecoil(WeaponType weaponType)
    {

        if (isRecovering) { RecoverFromRecoil(weaponType); } //リコイル回復を先にしないと、リコイルが完了したのと同フレームでリコイルが開始されてしまう
        if (isRecoiling)  { Recoil(weaponType);  }



    }



    // リコイル開始時の処理(射撃時にPlayerAvatarから呼ばれる)
    public void StartRecoil(WeaponType weaponType)
    {
        // リコイル開始時の処理
        isRecoiling = true;
        isRecovering = false;
        currentRecoilingWeapon = weaponType; 



        // リコイルの目標角度を設定（仮の値。実際には武器タイプに応じて調整する必要があります）

        if (isDebugParameter)
        {
            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + debug_recoilAmount_Pitch, debug_recoilLimit_Pitch) * currentRecoilMultiplier; // ピッチのリコイル目標（仮の値）
            if (weaponType.RecoilAmount_Yaw() > 0f) recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex] * debug_recoilAmount_Yaw * currentRecoilMultiplier; // ヨーのリコイル目標（仮の値）
        }
        else
        {


            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + weaponType.RecoilAmount_Pitch(), weaponType.RecoilLimit_Pitch()) * currentRecoilMultiplier; // ピッチのリコイル目標（仮の値）
            if (weaponType.RecoilAmount_Yaw() > 0f) recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex]*weaponType.RecoilAmount_Yaw() *currentRecoilMultiplier ; // ヨーのリコイル目標（仮の値）

        }

       
            
        recoilPatternIndex++;
        if (recoilPatternIndex >= recoilRandomPatterns_Yaw[weaponType].Count)
        {
            recoilPatternIndex = 0; // パターンのインデックスをリセット
        }

    }


    // リコイル中の処理
    void Recoil(WeaponType weaponType)
    {

        //リコイルの目標角度に向かって現在のリコイル角度を更新
        if (isDebugParameter)
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, debug_recoilSpeed_Pitch * currentRecoilMultiplier * Time.deltaTime);
           currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, debug_recoilSpeed_Yaw * currentRecoilMultiplier * Time.deltaTime); // ヨーのリコイル角度も更新

        }
        else
        {
           

            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, weaponType.RecoilAngularVelocity_Yaw() * currentRecoilMultiplier * Time.deltaTime); // ヨーのリコイル角度も更新
        }



        //目標に達したらリコイルを終了
        if (Mathf.Approximately( currentRecoil_Pitch, recoilTarget_Pitch  ))
        {
            //リコイル回復を開始
            StartRecoverFromRecoil(weaponType);
        }

        

    }


    // リコイル回復開始時の処理
    public void StartRecoverFromRecoil(WeaponType weaponType)
    {
        isRecoiling = false;
        isRecovering = true;
       
        // リコイルの目標角度を設定（仮の値。実際には武器タイプに応じて調整する必要があります）
        recoverTarget_Pitch = 0f; // ピッチのリコイル回復目標
        recoverTarget_Yaw = 0f; // ヨーのリコイル回復目標
    }



    public void RecoverFromRecoil(WeaponType weaponType)
    {

        // リコイル回復の目標角度に向かって現在のリコイル角度を更新
        
        if (isDebugParameter)
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, debug_recoverSpeed_Pitch * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, debug_recoverSpeed_Yaw * currentRecoilMultiplier * Time.deltaTime); // ヨーのリコイル回復角度も更新

        }
        else
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, weaponType.RecoverAngularVelocity_Yaw() * currentRecoilMultiplier * Time.deltaTime); // ヨーのリコイル回復角度も更新
        }

        // 目標に達したらリコイル回復を終了
        if (Mathf.Approximately(currentRecoil_Pitch, recoverTarget_Pitch))
        {
            isRecovering = false;

        }

    }


    //リコイルをリセットする(武器切り替え時にPlayerAvatarから呼ばれる)
    public void ResetRecoil()
    {
        currentRecoil_Pitch = 0f;
        currentRecoil_Yaw = 0f;
        isRecoiling = false;
        isRecovering = false;
        recoilPatternIndex = 0; //リコイルパターンのインデックスをリセット

    }

    List<float> GenerateRandomPattern(int seed, int count, float min, float max)
    {
        List<float> patternList = new List<float>(); //パターンを格納するリスト
        patternList.Clear(); //リストをクリア

        System.Random rand = new System.Random(seed); //シード値を設定
        for (int i = 0; i < count; i++)
        {
            float randomValue = (float)rand.NextDouble(); //0から1の範囲でランダムな半径を生成
            randomValue = Mathf.Lerp(min, max, randomValue); //最小値と最大値の間で補間
            patternList.Add(randomValue); //リストに追加
        }

        return patternList;


    }

    public void EndFiring()
    {
        recoilPatternIndex= 0; // リコイルパターンのインデックスをリセット


    }


    #endregion

    #region ADS関連

    public void SetADS(bool ADSflag)
    {
        isADS= ADSflag;

        // カメラ寄せ・FOV切り替え
        targetOffset = ADSflag ? adsShoulderOffset : normalShoulderOffset;
        targetDistance = ADSflag ? adsDistance : normalDistance;
        targetFOV = ADSflag ? adsFOV : normalFOV;


        // 感度とリコイル倍率
        currentSensitivity = ADSflag ? adsSensitivity : normalSensitivity;
        currentRecoilMultiplier = ADSflag ? ADSRecoilMultiplier : 1f; // ADS時のリコイル倍率を適用


        //currentRecoilMultiplier = isADS ? adsRecoilMultiplier : normalRecoilMultiplier;

        // サイトUI
        //if (adsSightUI != null)
        //    adsSightUI.SetActive(isADS);
    }

    void ADStransition()
    {
        // 補間処理
        thirdPersonFollow.ShoulderOffset = Vector3.Lerp(
            thirdPersonFollow.ShoulderOffset,
            targetOffset,
            Time.deltaTime * offsetLerpSpeed
        );

        thirdPersonFollow.CameraDistance = Mathf.Lerp(
            thirdPersonFollow.CameraDistance,
            targetDistance,
            Time.deltaTime * offsetLerpSpeed
        );

        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(
            virtualCamera.m_Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovLerpSpeed
        );

    }

    void InitializeADS()
    {
        isADS = false;
        targetOffset = normalShoulderOffset;
        targetDistance = normalDistance;
        targetFOV = normalFOV;
        currentSensitivity = normalSensitivity;
        currentRecoilMultiplier= 1f;

    }

    public void CancelADS()
    {
        InitializeADS();
        thirdPersonFollow.ShoulderOffset= normalShoulderOffset;
        thirdPersonFollow.CameraDistance = normalDistance;
        virtualCamera.m_Lens.FieldOfView = normalFOV;
        currentSensitivity = normalSensitivity;
        currentRecoilMultiplier = 1f;



    }





    #endregion


    #region カーソル関連
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // 中央固定
        Cursor.visible = false;
        cursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // 通常カーソル
        Cursor.visible = true;
        cursorLocked = false;
    }

    #endregion



    

    public Transform GetTPSCameraTransform()
    {
        return this.gameObject.transform;
    }


    // TPSカメラのTransformを取得するメソッド
    //cinemachine colliderに同じ機能あったので、こちらは使用しない！！！！！
    void ControlDistanceWithCollision()
    {
        Debug.Log("ControlDistanceWithCollision called"); // デバッグ用ログ出力
        Vector3 rayStartPos = CalculateCameraPosition(cameraTarget, thirdPersonFollow.ShoulderOffset,adsDistance);

        Vector3 rayEndPos = CalculateCameraPosition(cameraTarget, thirdPersonFollow.ShoulderOffset, normalDistance);


        Vector3 direction = rayEndPos- rayStartPos ; // カメラの位置からターゲットの位置への方向ベクトルを計算

        RaycastHit hit;


        if (Physics.Raycast(rayStartPos , direction/ direction.magnitude, out hit, direction.magnitude, obstacleLayer))
        {
            // hitLayers に含まれるレイヤーのオブジェクトだけ検出

            Debug.Log($"Obstacle detected at distance: {hit.distance}"); // 障害物検出のデバッグ用ログ出力
            thirdPersonFollow.CameraDistance = hit.distance - obstacleBuffer; // カメラの距離を障害物までの距離からバッファを引いた値に設定
            


        }
        else
        {
            // 障害物がない場合はカメラの位置を更新
            
            thirdPersonFollow.CameraDistance= targetDistance; // targetDistanceをカメラの距離に設定
        }   




    }

    Vector3 CalculateCameraPosition(Transform target,Vector3 shoulderOffset,float cameraDistance)
    {
        // 1) ローカル空間でのオフセットベクトルを作成
        //    X : 横（右方向が正）
        //    Y : 上下（上方向が正）
        //    Z : 奥行（背後：-cameraDistance）
        var localOffset = new Vector3(
            shoulderOffset.x,
            shoulderOffset.y,
            shoulderOffset.z - cameraDistance
        );

        // 2) target の回転を使ってローカル→ワールド方向に変換
        //    TransformDirection は回転のみを適用
        Vector3 worldOffset = target.TransformDirection(localOffset);

        // 3) ワールド座標上の最終位置を計算
        return target.position + worldOffset;
    }

}