using Fusion;
using UnityEngine;

public class GunRaycaster : NetworkBehaviour
{

    public TPSCameraController playerCamera; // 自分のTPSカメラ
    public float fireDistance = 100f;
    public LayerMask hitMask;

    public int weaponDamage = 10;


    public override void Spawned()
    {

        playerCamera = FindObjectOfType<TPSCameraController>();

    }


        // Start is called before the first frame update
        void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (HasInputAuthority) 
        { 
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Gun Fire!");
            GunFire(playerCamera.transform.position, playerCamera.transform.forward);


        }

        }

    }

    //originからdirection方向へ打つ
    public void GunFire(Vector3 origin, Vector3 direction)
    {

        Runner.LagCompensation.Raycast(
            origin,
            direction,
            fireDistance,
            Object.InputAuthority,
            out var hit,
            hitMask.value, //判定を行うレイヤーを制限する
            HitOptions.None);

        Debug.DrawRay(
            origin,
            direction * fireDistance,
            Color.red, 1f);


        Debug.Log("Hit?" + hit.GameObject);
        //着弾処理 
        if (hit.GameObject != null)
        {
            Debug.Log("Hit!" + hit.GameObject);

            //当たった対象がPlayerHitboxを持っていたらダメージ処理
            if (hit.Hitbox is PlayerHitbox playerHitbox)
            {

                CauseDamage(hit, weaponDamage);
            }
            else
            {
                Debug.Log("Hit! but not Player");
            }


        }
        

    }

    void CauseDamage(LagCompensatedHit hit, int weaponDamage)
    {

        //当たった対象がPlayerHitboxを持っていたらダメージ処理
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            targetHP.TakeDamage(myPlayerRef, weaponDamage);
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }


    //自作クラスを引数にとっても大丈夫？→重いのでダメ
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    //public void RPC_RequestDamage(PlayerRef targetPlayerRef, int Damage,RpcInfo info=default)
    //{
    //    Debug.Log($"RPCStart!");
    //    PlayerAvatar damagedPlayerAvatarScript = Runner.GetPlayerObject(targetPlayerRef).GetComponent<PlayerAvatar>();

    //    Debug.Log($"{targetPlayerRef} was attacked by {info.Source}, took {Damage} Damage ");
    //    // damagedPlayerAvatarScript.TakeDamage(playerNetworkState, Damage);


    //}
}
