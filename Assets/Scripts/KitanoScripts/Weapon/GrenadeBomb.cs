using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class GrenadeBomb : NetworkBehaviour
{
    WeaponType weaponType=WeaponType.Grenade; // 武器の種類を保持する変数(WeaponTypeスクリプトを参照する)


    PlayerRef throwPlayer;

    // Start is called before the first frame update
    [SerializeField] private LayerMask playerLayer;
    public LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public LayerMask ObstructionLayer => obstructionLayer;

    [SerializeField] private float directHitRadius = 1f; // 直撃判定の半径
    [SerializeField] private float blastHitRadius = 5f; // 爆風の半径

    [SerializeField] private float minBlastDamage = 20f; // 爆風の最小ダメージ


    [SerializeField] private float damageDuration = 1f; // 爆発時間(当たり時間)
    [SerializeField] private float explosionDelay = 3.5f; // 爆発までの遅延時間


    private float rayDrawingDuration = 1f; // Rayの描画時間


    //Raycastの方向を計算するための変数
    [SerializeField] float cornRayAngleDeg = 30f; // 円錐形の角度
    [SerializeField] int cornRayNum = 10; // 円錐形の放射状のRayの本数

    [Header("音関係")]
    [SerializeField] private string timerClipKey; // 爆発音のクリップ
    [SerializeField] private float timerInterVal =1f ; // 爆発音の間隔
    [SerializeField][Range(0f, 1f)] private float timerClipVolume = 1f; // 爆発音の音量
    private float timerElapsed = 0f; // タイマーの経過時間

    [SerializeField] private string explosionClipKey; // 爆発音のクリップ
    [SerializeField][Range(0f, 1f)] private float explosionClipVolume = 1f; // 爆発音の音量

    [Header("VFX関係")]
    //[SerializeField] private ParticleSystem particlePrefab; // Inspectorに登録するパーティクルシステムのプレハブ
    [SerializeField] GameObject explosionPrefab; // 半透明の球プレハブ
    [SerializeField] private LayerMask wallMask; // 壁のレイヤーマスク
    [SerializeField] private float normalOffset = 0.1f; // SphereCastの半径
    [SerializeField] private float castRadius = 0.1f; // SphereCastの半径
    [SerializeField] ExplosionSphereSpawner spawner;


    public override void Spawned()
    {
        //base.Spawned();
        // Initialization code here, if needed

        ExplosionSphereSpawner spawner = GetComponent<ExplosionSphereSpawner>();

        if (HasStateAuthority)
        {
            StartCoroutine(DamageCoroutine());
        }
        else
        {
            StartCoroutine(OnlyEffectCoroutine());


        }
    }

    private void Update()
    {
        timerElapsed += Time.deltaTime; // タイマーの経過時間を更新
        if (timerElapsed > timerInterVal)
        { 
            timerElapsed = 0f; // タイマーをリセット

            //タイマー音を再生する
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(timerClipKey)) // AudioManagerが存在し、クリップキーが設定されている場合
            {
                SoundHandle SEHandle=AudioManager.Instance.PlaySound(timerClipKey, SoundCategory.Weapon,0f,explosionClipVolume, SoundType.OneShot,this.transform.position,this.transform);
            }
            else
            {
                Debug.LogWarning("AudioManager or timerClipKey is not set!");
            }

        }


    }


    private IEnumerator OnlyEffectCoroutine()
    {

        yield return new WaitForSeconds(explosionDelay);
        //爆発音を再生する
        SoundHandle exHandle = AudioManager.Instance.PlaySound(explosionClipKey, SoundCategory.Weapon, 0f, explosionClipVolume, SoundType.OneShot, this.transform.position, this.transform);
        //タイマーの音を停止する
        timerElapsed = 0f; // タイマーをリセット

        // 爆発範囲の描画
        SpawnParticle(this.transform.position, damageDuration);
    }   


    private IEnumerator DamageCoroutine()
    {
        yield return new WaitForSeconds(explosionDelay); // 爆発までの遅延時間を待つ

        //爆発音を再生する
        SoundHandle exHandle = AudioManager.Instance.PlaySound(explosionClipKey, SoundCategory.Weapon, 0f, explosionClipVolume, SoundType.OneShot, this.transform.position, this.transform);

        //タイマーの音を停止する
        timerElapsed = 0f; // タイマーをリセット

        // 爆発範囲の描画
        SpawnParticle(this.transform.position, damageDuration);

        float elapsed = 0f;
        List<HitboxRoot> alreadyDamagedPlayers = new List<HitboxRoot>(); // すでにダメージを与えたプレイヤーを記録するリスト(今のフレームでダメージが確定した人も含む)


        // 爆発範囲の描画
        if (OverlapSphereVisualizer.Instance != null)
        {
            OverlapSphereVisualizer.Instance.ShowSphere(this.transform.position, blastHitRadius, rayDrawingDuration, "Sword Attack Area", Color.blue); // 攻撃判定の範囲を可視化する
        }
        else
        {
            Debug.LogWarning("OverlapSphereVisualizer.Instance is null! Please ensure it is set up in the scene.");
        }

        while (elapsed < damageDuration)
        {
            //タイマーの音を停止する
            timerElapsed = 0f; // タイマーをリセット

            CollisionDetection(alreadyDamagedPlayers);

            elapsed += Time.deltaTime;
            yield return null;

        }
        //ダメージ用のリストをクリア
        alreadyDamagedPlayers.Clear();

        Runner.Despawn(this.Object); // 爆発後にオブジェクトを破棄する

    }

    void CollisionDetection(List<HitboxRoot> alreadyDamaged)
    {

        var hits = new List<LagCompensatedHit>();
        Debug.Log($"GrenadeBomb CollisionDetection called. Already damaged count: {alreadyDamaged.Count},Runner:{Runner!=null}");
        int hitCount = Runner.LagCompensation.OverlapSphere( // 攻撃判定を行う
            this.transform.position,
            blastHitRadius,
            Object.InputAuthority,
            hits,
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.None // HitOptions.Noneを使用して、すべてのヒットを取得する
            );



        Debug.Log($"GrenadeBomb OverlapSphere hit count: {hitCount}"); // ヒット数をログに出力

        if (hitCount > 0)
        {

            Dictionary<LagCompensatedHit, float> damagedHitsWithDistance = new Dictionary<LagCompensatedHit, float>();//このフレームでダメージを与えるプレイヤーとそのヒット情報
            foreach (var hit in hits)
            {



                // 当たった対象がPlayerHitboxを持っていたら障害物の判定
                if (hit.Hitbox is PlayerHitbox playerHitbox)
                {
                    HitboxRoot targetPlayerRoot = playerHitbox.Root;

                    if (alreadyDamaged.Contains(targetPlayerRoot)) // 既にダメージを与えたプレイヤーならスキップ
                    {
                        Debug.Log($"Already hit player {targetPlayerRoot}, skipping damage.");
                        continue;
                    }

                    bool isHitCornRay = TryRaycastCornRadialAndGetDistance(hit, out LagCompensatedHit raycastHit, out float hitDistance);


                    if(isHitCornRay )// 円錐形のRaycastを行う
                    {
                        alreadyDamaged.Add(targetPlayerRoot); // ヒットしたプレイヤーのRootを記録
                        damagedHitsWithDistance[hit] = hitDistance; // ダメージを与えたヒット情報、ヒット距離を記録
                    }
                }
                else
                {
                    Debug.Log("Hit but not a PlayerHitbox: " + hit.GameObject);
                }
            }

            foreach (var kv in damagedHitsWithDistance)
            {
                CauseDamage(kv.Key, GetDamageByDistance(kv.Value));
            }


            damagedHitsWithDistance.Clear();



        }
        else
        {
            Debug.Log("No hits detected in OverlapSphere!");
        }




    }

    // 円錐形のRaycastを行う
    bool TryRaycastCornRadialAndGetDistance(LagCompensatedHit sphereHit, out LagCompensatedHit raycastHit, out float minHitDistance)
    {
        raycastHit = default; // 初期化
        minHitDistance = float.PositiveInfinity;
        Vector3 explosionDirection = sphereHit.GameObject.transform.position - this.transform.position; // 剣の方向を計算(後々、被弾側のspineを参照するようにする)
        List<Vector3> rayDirections = CornRaycastDirections(explosionDirection, cornRayAngleDeg, cornRayNum); // 30度の円錐形の方向に4本のRayを放射状に飛ばす

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           this.transform.position,
           direction,
           blastHitRadius,   // 爆風の範囲でレイキャスト
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionLayer, // 判定を行うレイヤーを制限する。プレイヤーと障害物のレイヤーを指定
           HitOptions.IncludePhysX

            );


            //Debug.DrawRay(this.transform.position, direction * blastHitRadius, Color.blue, rayDrawingDuration);


            if (RaycastLinePoolManager.Instance != null)
            {
                Vector3 rayEnd = Vector3.zero;

                //rayEnd = this.transform.position + direction * blastHitRadius; // ヒットポイントがない場合は剣の長さまでのRayを描画


                if (hitResult.Point != null && hitResult.Point!=Vector3.zero)
                {
                    rayEnd = hitResult.Point; // ヒットしたポイントがある場合はそこまでのRayを描画
                }
                else
                {
                    rayEnd = this.transform.position + direction * blastHitRadius; // ヒットポイントがない場合は爆風範囲までのRayを描画

                }

                RaycastLinePoolManager.Instance.ShowRay(this.transform.position, rayEnd, Color.blue, rayDrawingDuration);
            }

            if (hitResult.GameObject != null)
            {
                Debug.Log($"Bomb Raycast hit. Layer:{hitResult.GameObject.layer},isPlayerLayer:{((1 << hitResult.GameObject.layer) & playerLayer) != 0}");
            }

            //最近接の着弾位置を更新 
            if (hitResult.GameObject != null && ((1 << hitResult.GameObject.layer) & playerLayer) != 0 && hitResult.Distance < minHitDistance)
            {
                raycastHit = hitResult; // ヒットした情報を返す
                minHitDistance = hitResult.Distance; // ヒットした距離を更新

            }


            Debug.Log($"Raycast direction: {direction}");
        }


        if (minHitDistance <= blastHitRadius)
        {
            return true; // いずれかのRayがヒットした場合はtrueを返す

        }
        else
        {
            Debug.Log("No hits detected!");
            return false; // どのRayもヒットしなかった場合はfalseを返す
        }




    }

    //対象のいる方向から、飛ばすRaycastの方向を計算するメソッド
    List<Vector3> CornRaycastDirections(Vector3 axisDirection, float cornAngleDeg, int radialRayNum) // 方向と放射状のRayの本数(引数の方向にも飛ばすので、合計で[radialRayNum+1]本になる)
    {
        List<Vector3> directions = new List<Vector3>();


        // 軸ベクトルを正規化
        Vector3 axis = axisDirection.normalized;

        // 円錐の開き角の半分（ラジアンに変換）
        float theta = cornAngleDeg * 0.5f * Mathf.Deg2Rad;

        // 軸と垂直なベクトルを作る（任意でOK）
        Vector3 ortho = Vector3.Cross(axis, Vector3.up); // 外積を計算している
        if (ortho == Vector3.zero) ortho = Vector3.Cross(axis, Vector3.right);
        ortho.Normalize();

        // 円錐の表面上の1点（軸から角度thetaに回転）
        Quaternion tilt = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, Vector3.Cross(axis, ortho));
        Vector3 baseVec = tilt * axis;


        // 最初の1本は軸方向に設定
        directions.Add(axis);
        // 等間隔で回転して[radialRayNum]個作成
        for (int i = 0; i < radialRayNum; i++)
        {
            float angleAroundAxis = (360f / radialRayNum) * i;
            Quaternion rot = Quaternion.AngleAxis(angleAroundAxis, axis);
            Vector3 e = rot * baseVec;
            directions.Add(e.normalized); // 必要なら長さaに揃える
        }


        return directions;
    }


    public  void CauseDamage(LagCompensatedHit hit, int weaponDamage)
    {

        //当たった対象がPlayerHitboxを持っていたらダメージ処理
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            Debug.Log($"GrenadeBomb throwPlayer {throwPlayer}");
            Debug.Log($"GrenadeBomb InputAuthority {Object.InputAuthority}");
            targetHP.RPC_RequestDamage(myPlayerRef, weaponDamage);
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }

    private int GetDamageByDistance(float distance)
    {

        if (distance <= directHitRadius) return weaponType.Damage();
        if (distance >= blastHitRadius) return 0;
        float t = (distance - directHitRadius) / (blastHitRadius - directHitRadius);
        return Mathf.RoundToInt(Mathf.Lerp(weaponType.Damage(), minBlastDamage, t));
    }


    public void SetThrowPlayer(PlayerRef playerRef)
    {
        throwPlayer = playerRef; // 投げたプレイヤーのPlayerRefを設定する
        Debug.Log($"GrenadeBomb: SetThrowPlayer called with PlayerRef: {playerRef}");
    }

    /// <summary>
    /// 指定位置にパーティクルを出し、再生時間に合わせて速度を調整
    /// </summary>
    public void SpawnParticle(Vector3 position, float targetDuration)
    {
        //Vector3 desiredPos=this.transform.position; // 位置はGrenadeBombの位置を使用
        //Vector3 spawnPos = desiredPos;
        //Quaternion spawnRot = Quaternion.identity;

        //// どの方向からでも拾えるように、少しだけ外側へ押し出すSphereCast
        //if (Physics.SphereCast(desiredPos + Vector3.forward * 0.01f, castRadius, Vector3.back,
        //                       out RaycastHit hit, 0.05f, wallMask, QueryTriggerInteraction.Ignore)
        //    || Physics.SphereCast(desiredPos + Vector3.right * 0.01f, castRadius, Vector3.left,
        //                          out hit, 0.05f, wallMask, QueryTriggerInteraction.Ignore)
        //    || Physics.SphereCast(desiredPos + Vector3.up * 0.01f, castRadius, Vector3.down,
        //                          out hit, 0.05f, wallMask, QueryTriggerInteraction.Ignore))
        //{
        //    spawnPos = hit.point + hit.normal * normalOffset;         // めり込み防止
        //    spawnRot = Quaternion.LookRotation(hit.normal);           // 面の外向きへ向ける
        //    Debug.Log($"GrenadeBomb: SpawnParticle position adjusted to {spawnPos} based on SphereCast hit. DesiredPos:{desiredPos}");
        //}

        // インスタンス化
        spawner.Spawn(
             explosionPrefab,
             position: transform.position,
             startRadius: 0.5f, growTime: 0.15f, endRadius: blastHitRadius,
             startAlpha: 0.9f, endAlpha: 0.3f,
             duration: 1f, fadeTime: 0.1f
         );
    }




}
