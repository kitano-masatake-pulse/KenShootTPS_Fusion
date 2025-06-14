using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : WeaponBase
{

    protected override WeaponType weapon => WeaponType.AssaultRifle; // 武器の種類を指定

    public Transform muzzleTransform; // 銃口(=Raycastの光源)の位置を指定するTransform。一旦、PlayerAvatarからcameraをせっていする

    CharacterController avatarCharacterController; // キャラクターコントローラーを取得するための変数



    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;


    float rayDrawingDuration=1/60f; // Rayの描画時間(1/60秒)


    #region 弾の拡散(スプレッド)
    //弾の拡散に関するパラメータ
    [SerializeField]float liftingSpreadGauge = 0f;
    [SerializeField] float randomSpreadGauge = 0f; // Spreadの計算に使う連射時間を蓄積する変数、最大は1

    float liftingSpreadRate = 0.1f; // Spreadの拡散速度(spreadGaugeが1発撃つといくら増えるか)
    float randomSpreadate = 0.1f; // Spreadの拡散速度(spreadGaugeが1発撃つといくら増えるか)
    float liftingConvergenceRate = 0.3f; // Spreadの収束速度(preadGaugeが秒間いくら減るか)
    float randomConvergenceRate = 0.3f; // Spreadの収束速度(preadGaugeが秒間いくら減るか)

    //spraedGauge,moveSpeedに対する拡散の限界値
    [SerializeField] float liftingSpreadLimit = 1f;
    [SerializeField] float randomSpreadLimit = 1f;
    [SerializeField] float runninngSpreadLimit = 1f;


    //spraedGauge,,moveSpeedに対する拡散の倍率(degree/gauge)([x,y]=[pitch,yaw])
    [SerializeField] Vector2 liftingSpreadMultiplier = new Vector2(5f, 0f);
    [SerializeField] Vector2 randomSpreadMultiplier = new Vector2(3f,6f);
    [SerializeField] Vector2 runninngSpreadMultiplier = new Vector2(3f, 6f);

    //スプレッドパターンを決めるためのシード値
    int seed_RandomSpreadRadius = 65115; //ランダムスプレッドの半径のシード値
    int seed_RandomSpreadAngle = 11597; //ランダムスプレッドの角度のシード値
    int seed_RunningSpreadRadius = 11710;
    int seed_RunningSpreadAngle = 81168; //ランニングスプレッドの角度のシード値

    List<float> randomPattern_RandomSpreadRadius = new List<float>(); //ランダムスプレッドの半径のパターン
    List<float> randomPattern_RandomSpreadAngle = new List<float>(); //ランダムスプレッドの角度のパターン
    List<float> randomPattern_RunningSpreadRadius = new List<float>(); //ランニングスプレッドの半径のパターン
    List<float> randomPattern_RunningSpreadAngle = new List<float>(); //ランニングスプレッドの角度のパターン

    #endregion



    bool isADSNow = false; //ADS中かどうかのフラグ
    bool isConvergenceNow = false; //収束中かどうかのフラグ
    int  spreadPatternIndex = 0; //スプレッドのパターンのインデックス



    [SerializeField]float debug_moveSpeed = 0f; //デバッグ用の移動速度(実際の移動速度を代入する)


    

    protected override void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }



    // Start is called before the first frame update
    void Start()
    {
        int magazineCapacity = weaponType.MagazineCapacity(); //マガジンの容量を取得

        //ランダムパターンを生成
        randomPattern_RandomSpreadRadius = GenerateRandomPattern(seed_RandomSpreadRadius, magazineCapacity, 0f, 1f); //ランダムスプレッドの半径のパターンを生成
        randomPattern_RandomSpreadAngle = GenerateRandomPattern(seed_RandomSpreadAngle, magazineCapacity, 0f, 2*Mathf.PI); //ランダムスプレッドの角度のパターンを生成
        randomPattern_RunningSpreadRadius = GenerateRandomPattern(seed_RunningSpreadRadius, magazineCapacity, 0f, 1f); //ランニングスプレッドの半径のパターンを生成
        randomPattern_RunningSpreadAngle = GenerateRandomPattern(seed_RunningSpreadAngle, magazineCapacity, 0f, 2 * Mathf.PI); //ランニングスプレッドの角度のパターンを生成

        avatarCharacterController=GetComponentInParent<CharacterController>(); //親のキャラクターコントローラーを取得


    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpreadGauge(-liftingConvergenceRate * Time.deltaTime, -randomConvergenceRate * Time.deltaTime); //弾の拡散を収束させる
        
    }

    public override void FireDown()
    {
        base.FireDown();
        spreadPatternIndex = 0; //スプレッドパターンのインデックスをリセット

        Vector3 spreadDirection = SpreadRaycastDirection(muzzleTransform.forward, liftingSpreadGauge, randomSpreadGauge, debug_moveSpeed, spreadPatternIndex); //射線の拡散を計算
        GunRaycast(muzzleTransform.position, spreadDirection);
     
        UpdateSpreadGauge(liftingSpreadRate, randomSpreadate); //弾の拡散を更新

    }


    public override void Fire()
    { 
        base.Fire();
        spreadPatternIndex++; //スプレッドパターンのインデックスを更新
        if (spreadPatternIndex >= randomPattern_RandomSpreadRadius.Count) //スプレッドパターンのインデックスが範囲外になったらリセット
        {
            spreadPatternIndex = 0;
        }

        Vector3 spreadDirection = 
            SpreadRaycastDirection(
                muzzleTransform.forward, 
                liftingSpreadGauge, 
                randomSpreadGauge, 
                avatarCharacterController.velocity.magnitude, 
                spreadPatternIndex); //射線の拡散を計算
        
        
        
        GunRaycast(muzzleTransform.position,spreadDirection);
        UpdateSpreadGauge(liftingSpreadRate, randomSpreadate); //弾の拡散を更新
        // ここにアサルトライフル特有の発射処理を追加することができます
    }

    void GunRaycast(Vector3 origin, Vector3 direction)
    {

        //射線の拡散を計算する
        //Vector3 spreadDirection = SpreadRaycastDirection(direction, liftingSpreadGauge, randomSpreadGauge,debug_moveSpeed); //射線の拡散を計算


        Runner.LagCompensation.Raycast(
              origin,
              direction,
              fireDistance,
              Object.InputAuthority,
              out var hit,
             playerLayer | obstructionLayer, //判定を行うレイヤーを制限する
              HitOptions.IgnoreInputAuthority | HitOptions.IncludePhysX

              );

       if(RaycastLinePoolManager.Instance != null)
            {
            Vector3 rayEnd = Vector3.zero;
            rayEnd = origin + direction * fireDistance; // ヒットポイントがない場合は剣の長さまでのRayを描画


            //rayEnd = origin + direction * fireDistance; // ヒットポイントがない場合は剣の長さまでのRayを描画

            if (hit.Point != null && hit.Point!=Vector3.zero)
            {
                rayEnd = hit.Point; // ヒットしたポイントがある場合はそこまでのRayを描画
                Debug.Log("Hit Point: " + rayEnd);
            }
            else
            {
                rayEnd = origin + direction * fireDistance;  // ヒットポイントがない場合は剣の長さまでのRayを描画
                Debug.Log("No Hit Point, using fireDistance: " + rayEnd);

            }

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


    //射線の拡散を計算するメソッド
    //それぞれのスプレッドを[pitch,yaw]の２次元ベクトルとして計算し、足し合わせた結果を使って元の射線方向を回転させる
    private Vector3 SpreadRaycastDirection(Vector3 direction ,float　liftingSpreadParam ,float randomSpreadParam,float moveSpeed,int spreadIndex)
    {
        Vector3 spreadDirection = Vector3.zero;

      

        //リフティングスプレッド(連射時間によって上方向へぶれる)
        Vector2 liftingSpredDir = Vector2.zero; //リフティングスプレッドのベクトル

        liftingSpredDir= Mathf.Clamp(liftingSpreadParam, 0, liftingSpreadLimit) * liftingSpreadMultiplier; //X軸方向のスプレッドを計算

        //ランダムスプレッド(連射時間によってランダムにぶれる)
        Vector2 randomSpreadDir = Vector2.zero; //ランダムスプレッドのベクトル

        float radius_rand = randomPattern_RandomSpreadRadius[spreadIndex]; //ランダムスプレッドの半径を取得
        float angle_rand = randomPattern_RandomSpreadAngle[spreadIndex]; //ランダムスプレッドの角度を取得

        //半径と角度の乱数からランダムスプレッドを計算
        randomSpreadDir.x = radius_rand * Mathf.Cos(angle_rand)  * randomSpreadGauge * randomSpreadMultiplier.x; 
        randomSpreadDir.y = radius_rand * Mathf.Sin(angle_rand) * randomSpreadGauge * randomSpreadMultiplier.y; 


        //ランニングスプレッド(移動速度に応じてランダムにぶれる)
        Vector2 runningSpreadDir = Vector2.zero; //ランニングスプレッドのベクトル

        float moveSpeedClamp = Mathf.Clamp(moveSpeed, 0, runninngSpreadLimit); //移動速度を制限
        float radius_run = randomPattern_RunningSpreadRadius[spreadIndex]; //ランニングスプレッドの半径を取得
        float angle_run = randomPattern_RunningSpreadAngle[spreadIndex]; //ランニングスプレッドの角度を取得

        runningSpreadDir.x = radius_run * Mathf.Cos(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.x;
        runningSpreadDir.y = radius_run * Mathf.Sin(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.y;



        //合計して射線の拡散を計算
        Vector2 totalSpreadDir = liftingSpredDir + randomSpreadDir + runningSpreadDir; //総合的なスプレッドのベクトル
        Debug.Log($"Total Spread Direction: {totalSpreadDir}"); //デバッグ用ログ出力

        Quaternion spreadRot= Quaternion.Euler(-totalSpreadDir.x, totalSpreadDir.y, 0f); //スプレッドの回転を計算(軸の問題でpitchは正負反転)

        spreadDirection = spreadRot * direction; //元の射線方向にスプレッドの回転を適用
         
        //元のdirectionとの内積をとる
        float dotProduct = Vector3.Dot(direction.normalized, spreadDirection.normalized); //元の射線方向との内積を計算
        Debug.Log($"total Dot Product: {dotProduct}"); //デバッグ用ログ出力


        return spreadDirection;
    }

    //SpreadGaugeを更新するメソッド
    void UpdateSpreadGauge( float liftingDifference,float randomDifference)
    { 
        liftingSpreadGauge= Mathf.Clamp(liftingSpreadGauge + liftingDifference, 0, liftingSpreadLimit); //リフティングスプレッドの更新
        randomSpreadGauge = Mathf.Clamp(randomSpreadGauge + randomDifference, 0, randomSpreadLimit); //ランダムスプレッドの更新

    }


    //スプレッドのパターンを生成するメソッド
    List<float>  GenerateRandomPattern(int seed, int count,float min ,float max)
    {
        List<float> patternList = new List<float>(); //パターンを格納するリスト
        patternList.Clear(); //リストをクリア

        System.Random rand = new System.Random(seed); //シード値を設定
        for (int i = 0; i < count; i++)
        {
            float randomValue = (float)rand.NextDouble() ; //0から1の範囲でランダムな半径を生成
            randomValue = Mathf.Lerp(min, max, randomValue); //最小値と最大値の間で補間
            patternList.Add(randomValue); //リストに追加
        }

        return patternList;


    }
}
