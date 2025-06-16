using Cinemachine;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//CinemachineVirtualCameraにアタッチすること
public class TPSCameraController : MonoBehaviour
{



    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Header("マウス操作")]
    [SerializeField] private float sensitivityX = 3f;
    [SerializeField] private float sensitivityY = 1.5f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 75f;
    private float yaw = 0f;
    private float pitch = 0f;
    public Transform cameraTarget;
    



    bool isSetCameraTarget=true;

    private NetworkRunner runner;


    private PlayerAvatar myPlayerAvatar;

  


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

    

    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        LockCursor();


        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            if (type.RecoilAmount_Yaw() > 0f)
            {
                // 各武器タイプのリコイル乱数パターンを生成
                recoilRandomPatterns_Yaw[type] = GenerateRandomPattern(seed_recoilYaw, type.MagazineCapacity(), -1f, 1f); 
            }
        }

    }
    void Update()
    {
        
        ApplyRecoil(WeaponType.AssaultRifle); // 仮の武器タイプを指定。実際には現在の武器タイプに応じて変更する必要があります

        if ( isSetCameraTarget)
           {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

            //Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}"); // デバッグ用ログ出力
            yaw += mouseX;
            pitch -= mouseY;

            //Debug.Log($"yaw: {yaw}, pitch: {pitch}"); // デバッグ用ログ出力

            ApplyRecoil(WeaponType.AssaultRifle); //リコイルの適用

            

            //yaw +=; // リコイルのヨーを適用
            //pitch -= currentRecoil_Pitch; // リコイルのピッチを適用

            //Debug.Log($"Adjusted yaw: {yaw}, Adjusted pitch: {pitch}"); // リコイル適用後の角度をログ出力

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch - currentRecoil_Pitch, yaw + currentRecoil_Yaw, 0f);
            cameraTarget.rotation = rotation;

            //myPlayerAvatar.ChangeTransformLocally(); // アバターの向きをカメラ方向に合わせる処理を呼び出す

           


        }

        // Escapeキーでモード切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
                UnlockCursor();
            else
                LockCursor();
        }
    }


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


        // リコイルの目標角度を設定（仮の値。実際には武器タイプに応じて調整する必要があります）

        if (isDebugParameter)
        {
            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + debug_recoilAmount_Pitch, debug_recoilLimit_Pitch); // ピッチのリコイル目標（仮の値）
            recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex] * debug_recoilAmount_Yaw; // ヨーのリコイル目標（仮の値）
        }
        else
        {


            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + weaponType.RecoilAmount_Pitch(), debug_recoilLimit_Yaw); // ピッチのリコイル目標（仮の値）
            recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex]*weaponType.RecoilAmount_Yaw(); // ヨーのリコイル目標（仮の値）

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
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, debug_recoilSpeed_Pitch * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, debug_recoilSpeed_Yaw * Time.deltaTime); // ヨーのリコイル角度も更新

        }
        else
        {
           

            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, weaponType.RecoilAngularVelocity_Yaw() * Time.deltaTime); // ヨーのリコイル角度も更新
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
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, debug_recoverSpeed_Pitch * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, debug_recoverSpeed_Yaw * Time.deltaTime); // ヨーのリコイル回復角度も更新

        }
        else
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, weaponType.RecoverAngularVelocity_Yaw() * Time.deltaTime); // ヨーのリコイル回復角度も更新
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

}