using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Fusion;

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
    

    bool isBattleScene = false;

    bool isSetCameraTarget=true;

    private NetworkRunner runner;


    private PlayerAvatar myPlayerAvatar;

    bool isADSNow = false; //ADS中かどうかのフラグ


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


    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        LockCursor();

    }
    void Update()
    {
        
        ApplyRecoil(WeaponType.AssaultRifle); // 仮の武器タイプを指定。実際には現在の武器タイプに応じて変更する必要があります

        if ( isSetCameraTarget)
           {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;
            yaw += mouseX;
            pitch -= mouseY;

            ApplyRecoil(WeaponType.AssaultRifle); //リコイルの適用
            yaw += currentRecoil_Yaw; // リコイルのヨーを適用
            pitch -= currentRecoil_Pitch; // リコイルのピッチを適用


            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
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
        recoilTarget_Pitch = currentRecoil_Pitch+weaponType.RecoilAmount_Pitch(); // ピッチのリコイル目標（仮の値）
        recoilTarget_Yaw = currentRecoil_Yaw + weaponType.RecoilAmount_Yaw(); // ヨーのリコイル目標（仮の値）

    }


    // リコイル中の処理
    void Recoil(WeaponType weaponType)
    {
        //リコイルの目標角度に向かって現在のリコイル角度を更新
        currentRecoil_Pitch= Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * Time.deltaTime);


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
        currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * Time.deltaTime);



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