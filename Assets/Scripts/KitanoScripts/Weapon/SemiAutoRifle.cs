using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiAutoRifle : WeaponBase
{

     protected override WeaponType weapon=> WeaponType.SemiAutoRifle; // 武器の種類を指定

    private Transform bulletFiringTransform; // Raycastの光源の位置を指定するTransform。ADSの有無によって変わる

    [SerializeField] private Transform muzzleTransform; //weaponにおける銃口。腰だめ時にはここから撃つ
    public Transform TPSCameraTransform;// TPSカメラのTransform。PlayerAvatarから設定される。ADS時にはここから撃つ

    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;

    float rayDrawingDuration = 1 / 60f; // Rayの描画時間(1/60秒)

    CharacterController avatarCharacterController; // キャラクターコントローラーを取得するための変数

    bool isADS = false;
    float reloadTimer = 0f; // リロードタイマー
    float reloadWaitTime = 1.5f; // リロードにかかる時間(秒)
    bool isWaitingForReload = false; // リロード待機中かどうかのフラグ

    #region 弾の拡散(スプレッド)
    //弾の拡散に関するパラメータ
    [SerializeField] float liftingSpreadGauge = 0f;
    [SerializeField] float randomSpreadGauge = 0f; // Spreadの計算に使う連射時間を蓄積する変数、最大は1

    float liftingSpreadRate = 0.1f; // Spreadの拡散速度(spreadGaugeが1発撃つといくら増えるか)
    float randomSpreadRate = 0.1f; // Spreadの拡散速度(spreadGaugeが1発撃つといくら増えるか)
    float liftingConvergenceRate = 0.3f; // Spreadの収束速度(preadGaugeが秒間いくら減るか)
    float randomConvergenceRate = 0.3f; // Spreadの収束速度(preadGaugeが秒間いくら減るか)

    //spraedGauge,moveSpeedに対する拡散の限界値
    [SerializeField] float liftingSpreadLimit = 1f;
    [SerializeField] float randomSpreadLimit = 1f;
    [SerializeField] float runninngSpreadLimit = 1f;


    //spraedGauge,,moveSpeedに対する拡散の倍率(degree/gauge)([x,y]=[pitch,yaw])
    [SerializeField] Vector2 liftingSpreadMultiplier = new Vector2(0, 0f);
    [SerializeField] Vector2 randomSpreadMultiplier = new Vector2(0, 0);
    [SerializeField] Vector2 runninngSpreadMultiplier = new Vector2(3f, 6f);

    //スプレッドパターンを決めるためのシード値
    int seed_RandomSpreadRadius = 11510; //ランダムスプレッドの半径のシード値
    int seed_RandomSpreadAngle = 11091; //ランダムスプレッドの角度のシード値
    int seed_RunningSpreadRadius = 05971;
    int seed_RunningSpreadAngle = 17116; //ランニングスプレッドの角度のシード値

    List<float> randomPattern_RandomSpreadRadius = new List<float>(); //ランダムスプレッドの半径のパターン
    List<float> randomPattern_RandomSpreadAngle = new List<float>(); //ランダムスプレッドの角度のパターン
    List<float> randomPattern_RunningSpreadRadius = new List<float>(); //ランニングスプレッドの半径のパターン
    List<float> randomPattern_RunningSpreadAngle = new List<float>(); //ランニングスプレッドの角度のパターン

    [SerializeField] float ADSspreadReduction = 0.8f; //ADS中かどうかのフラグ
    int spreadPatternIndex = 0; //スプレッドのパターンのインデックス



    #endregion


    [Header("音関係")]
    [SerializeField] private string  fireClipKey = "Weapon_Fire_SemiAutoRifle"; //射撃音のクリップキー
    [SerializeField] private float fireClipVolume = 1f; 
    [SerializeField] private float fireClipStartTime = 0f; 

    // Start is called before the first frame update
    public override void Spawned()
    {
        int magazineCapacity = weaponType.MagazineCapacity(); //マガジンの容量を取得

        //ランダムパターンを生成
        randomPattern_RandomSpreadRadius = GenerateRandomPattern(seed_RandomSpreadRadius, magazineCapacity, 0f, 1f); //ランダムスプレッドの半径のパターンを生成
        randomPattern_RandomSpreadAngle = GenerateRandomPattern(seed_RandomSpreadAngle, magazineCapacity, 0f, 2 * Mathf.PI); //ランダムスプレッドの角度のパターンを生成
        randomPattern_RunningSpreadRadius = GenerateRandomPattern(seed_RunningSpreadRadius, magazineCapacity, 0f, 1f); //ランニングスプレッドの半径のパターンを生成
        randomPattern_RunningSpreadAngle = GenerateRandomPattern(seed_RunningSpreadAngle, magazineCapacity, 0f, 2 * Mathf.PI); //ランニングスプレッドの角度のパターンを生成

        avatarCharacterController = GetComponentInParent<CharacterController>(); //親のキャラクターコントローラーを取得

        bulletFiringTransform = isADS ? TPSCameraTransform : muzzleTransform; //ADS状態に応じて弾の発射位置を切り替え


    }

    public override void CalledOnUpdate(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {

        //ADS
        if (CanADS(localInputData, inputBuffer, currentAction))
        {
            playerAvatar.SwitchADS();
        }

        if (isWaitingForReload)
        {
            reloadTimer += Time.deltaTime; //リロードタイマーを更新
            if (reloadTimer >= reloadWaitTime) //リロード時間が経過したら
            {
                isWaitingForReload = false; //リロード待機フラグを解除
                reloadTimer = 0f; //リロードタイマーをリセット

                FinishReload(); //リロード完了処理を呼び出す
                Debug.Log($"Reloaded {weaponType.GetName()}! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
            }



        }

        //リロード条件を満たしてたらリロード
        if (CanReload(localInputData, inputBuffer, currentAction))
        {
            isWaitingForReload = true; //リロード待機フラグを立てる
            //Reload(); //リロード処理を呼び出す
            playerAvatar.Reload();
            Debug.Log($"Reloading {weaponType.GetName()}! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
            return; //リロードしたら以降の処理は行わない
        }

        //射撃条件を満たしていたら射撃

        else if (CanFire(localInputData, inputBuffer, currentAction)) //連射中なら
        {
            if (IsMagazineEmpty())
            {
              //マガジンが空なら射撃できないので何もしない
                Debug.Log($"Cannot fire {weaponType.GetName()}! Magazine is empty. Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
                return;
            }
            else 
            {
                //Fire(); 
                playerAvatar.FireAction(); //PlayerAvatarの射撃処理を呼び出す
                Debug.Log($"Firing {weaponType.GetName()} stay! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
                return;

            }
                
        }

    }

    public bool CanADS(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool stateCondition =
            currentAction == WeaponActionState.Idle ||
            currentAction == WeaponActionState.Firing;


        return localInputData.ADSPressedDown && stateCondition; 
        //ADSボタンが押されていて、現在のアクションがアイドル状態であることを確認

    }

    public override bool CanFire(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        //発射可能かどうかを判定
        return localInputData.FirePressedDown && currentAction == WeaponActionState.Idle ; 
        //連射中かつ弾がある場合に発射可能
    }


    public override void Fire()
    {
        base.FireDown();
        currentMagazine--;

        //射撃音の再生
        PlayFireSEForAllClients();


        Vector3 spreadDirection =
            SpreadRaycastDirection
            (muzzleTransform.forward,
            liftingSpreadGauge,
            randomSpreadGauge,
            avatarCharacterController.velocity.magnitude,
            spreadPatternIndex,
            isADS); //射線の拡散を計算


        // ここにセミオートライフルの特有の処理を追加
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);
        Debug.Log($"{weaponType.GetName()} fired down in SemiAutoRifle!");
    }


    void GunRaycast(Vector3 origin, Vector3 direction)
    {
        Runner.LagCompensation.Raycast(
              origin,
              direction,
              fireDistance,
              Object.InputAuthority,
              out var hit,
             playerLayer | obstructionLayer, //判定を行うレイヤーを制限する
              HitOptions.IgnoreInputAuthority);


        //射線の可視化
        Vector3 rayEnd = Vector3.zero;
        rayEnd = origin + direction * fireDistance; // ヒットポイントがない場合は剣の長さまでのRayを描画

        if (hit.Point != null && hit.Point != Vector3.zero)
        {
            rayEnd = hit.Point; // ヒットしたポイントがある場合はそこまでのRayを描画
            Debug.Log("Hit Point: " + rayEnd);
        }
        else
        {
            rayEnd = origin + direction * fireDistance;  // ヒットポイントがない場合は剣の長さまでのRayを描画
            Debug.Log("No Hit Point, using fireDistance: " + rayEnd);

        }

        base.GenerateLineOfFireGorAllClients(origin, rayEnd); //着弾位置にRayを生成
        Debug.Log($"GenerateLineOfFire from {origin} to {rayEnd} with hit point: {hit.Point}");



        if (RaycastLinePoolManager.Instance != null)
        {

            RaycastLinePoolManager.Instance.ShowRay(origin, rayEnd, Color.red, rayDrawingDuration);
            Debug.Log("Raycast Line drawn from " + origin + " to " + rayEnd);
        }



        Debug.Log("Hit?" + hit.GameObject);
        //着弾処理 
        if (hit.GameObject != null)
        {
            Debug.Log("Hit!" + hit.GameObject);

            if (BulletMarkGenerator.Instance != null)
            {
                BulletMarkGenerator.Instance.GenerateBulletMark(hit.Point); //着弾位置に弾痕を生成

            }
            //当たった対象がPlayerHitboxを持っていたらダメージ処理
            if (hit.Hitbox is PlayerHitbox playerHitbox)
            {

                CauseDamage(hit, weaponType.Damage());
            }
            else
            {
                Debug.Log("Hit! but not Player");
            }


        }

    }


    private Vector3 SpreadRaycastDirection(Vector3 direction, float liftingSpreadParam, float randomSpreadParam, float moveSpeed, int spreadIndex, bool ADSflag)
    {
        Vector3 spreadDirection = Vector3.zero;




        //リフティングスプレッド(連射時間によって上方向へぶれる)
        Vector2 liftingSpredDir = Vector2.zero; //リフティングスプレッドのベクトル

        liftingSpredDir = Mathf.Clamp(liftingSpreadParam, 0, liftingSpreadLimit) * liftingSpreadMultiplier; //X軸方向のスプレッドを計算

        //ランダムスプレッド(連射時間によってランダムにぶれる)
        Vector2 randomSpreadDir = Vector2.zero; //ランダムスプレッドのベクトル

        float radius_rand = randomPattern_RandomSpreadRadius[spreadIndex]; //ランダムスプレッドの半径を取得
        float angle_rand = randomPattern_RandomSpreadAngle[spreadIndex]; //ランダムスプレッドの角度を取得

        //半径と角度の乱数からランダムスプレッドを計算
        randomSpreadDir.x = radius_rand * Mathf.Cos(angle_rand) * randomSpreadGauge * randomSpreadMultiplier.x;
        randomSpreadDir.y = radius_rand * Mathf.Sin(angle_rand) * randomSpreadGauge * randomSpreadMultiplier.y;


        //ランニングスプレッド(移動速度に応じてランダムにぶれる)
        Vector2 runningSpreadDir = Vector2.zero; //ランニングスプレッドのベクトル

        float moveSpeedClamp = Mathf.Clamp(moveSpeed, 0, runninngSpreadLimit); //移動速度を制限
        float radius_run = randomPattern_RunningSpreadRadius[spreadIndex]; //ランニングスプレッドの半径を取得
        float angle_run = randomPattern_RunningSpreadAngle[spreadIndex]; //ランニングスプレッドの角度を取得

        runningSpreadDir.x = radius_run * Mathf.Cos(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.x;
        runningSpreadDir.y = radius_run * Mathf.Sin(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.y;

        float ADSmultiplier = ADSflag ? ADSspreadReduction : 1f; //ADS時はスプレッドを半分にする


        //合計して射線の拡散を計算
        Vector2 totalSpreadDir = (liftingSpredDir + randomSpreadDir + runningSpreadDir) * ADSmultiplier; //総合的なスプレッドのベクトル
        Debug.Log($"Total Spread Direction: {totalSpreadDir}"); //デバッグ用ログ出力

        Quaternion spreadRot = Quaternion.Euler(-totalSpreadDir.x, totalSpreadDir.y, 0f); //スプレッドの回転を計算(軸の問題でpitchは正負反転)

        spreadDirection = spreadRot * direction; //元の射線方向にスプレッドの回転を適用

        //元のdirectionとの内積をとる
        float dotProduct = Vector3.Dot(direction.normalized, spreadDirection.normalized); //元の射線方向との内積を計算
        Debug.Log($"total Dot Product: {dotProduct}"); //デバッグ用ログ出力


        return spreadDirection;
    }

    //スプレッドのパターンを生成するメソッド
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

    public override void SetADS(bool ADSflag)
    {
        isADS = ADSflag; //ADS状態を更新
        Debug.Log($"SemiAutoRifle ADS state changed: {isADS}");
        bulletFiringTransform = isADS ? TPSCameraTransform : muzzleTransform; //ADS状態に応じて弾の発射位置を切り替え

    }

    public override void ResetOnChangeWeapon()
    {
         reloadTimer = 0f;
        isWaitingForReload = false;
    }

    #region 音関係
    void PlayFireSEForAllClients()
    {

        if (AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManagerが存在し、クリップキーが設定されている場合

        if (Runner.LocalPlayer==Object.InputAuthority)
        {
            SoundHandle SEHandle = AudioManager.Instance.PlaySound(fireClipKey, SoundCategory.Weapon, fireClipStartTime, fireClipVolume, SoundType.OneShot, this.transform.position);
            RPC_RequestPlayFireSE(); //RPCを呼び出して全クライアントに音を再生させる
        }
        
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestPlayFireSE(RpcInfo  rpcinfo=default)
    {
        if (AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManagerが存在し、クリップキーが設定されている場合
        RPC_ApplyPlayFireSE(rpcinfo.Source); //RPCを呼び出して音を再生
        Debug.Log("RPC_PlayFireSE called on " + Runner.LocalPlayer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyPlayFireSE(PlayerRef sourcePlayer)
    {
        if (Runner.LocalPlayer != sourcePlayer)
        {
            if(AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManagerが存在し、クリップキーが設定されている場合

            SoundHandle SEHandle = 
                AudioManager.Instance.PlaySound
                (fireClipKey, 
                SoundCategory.Weapon, 
                fireClipStartTime, 
                fireClipVolume, 
                SoundType.OneShot, 
                this.transform.position
                );
        }

    }
    #endregion

}
