using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerHP : NetworkBehaviour
{
    //誰のネットワークステートかわからない
    public PlayerNetworkState playerNetworkState;
    // Start is called before the first frame update

    
    //public void TakeDamage(PlayerRef AttackerPlayerRef, int Damage)
    //{
    //    Debug.Log($"TakeDamageMethod!");
    //    RPC_RequestDamage(AttackerPlayerRef, Damage);
    //}
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestDamage(PlayerRef AttackerPlayerRef, int Damage, RpcInfo info = default)
    {
        Debug.Log($"RPCRequestStart!");
        playerNetworkState.DamageHP(Damage, AttackerPlayerRef);
        //RPC_RelayDamage(AttackerPlayerRef, Damage, info);
    }

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer, TickAligned = false)]
    //public void RPC_RelayDamage(PlayerRef AttackerPlayerRef, int Damage, RpcInfo info = default)
    //{
    //    Debug.Log($"RPCRelayStart!");
    //    playerNetworkState.DamageHP(Damage, AttackerPlayerRef);
    //    Debug.Log($"Player {Object.InputAuthority} took {Damage} damage from {AttackerPlayerRef}, remaining HP: {playerNetworkState.CurrentHP}");
    //}
}
