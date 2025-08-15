using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SceneTransitionAnchor : NetworkBehaviour
{
    private Coroutine processAndNotifyCoroutine;

    public override void Spawned()
    {
        processAndNotifyCoroutine = null;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RequestSceneTransition(SceneType nextScene)
    {
        if (processAndNotifyCoroutine != null)
        {
            return;
        }
        

        processAndNotifyCoroutine = StartCoroutine(ProcessAndNotifyCoroutine(nextScene));

    }
    
    private IEnumerator ProcessAndNotifyCoroutine(SceneType nextScene)
    {
        Debug.Log("SceneChange:AnchorCoroutine");
        yield return StartCoroutine(SceneTransitionManager.Instance.TransitionProcessCoroutine(nextScene, true));
        RPC_NotifySceneTransitionFinished(nextScene); // シーン変更完了を通知
        processAndNotifyCoroutine = null;
    }
    
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_NotifySceneTransitionFinished(SceneType nextScene, RpcInfo info = default)
    {
        Debug.Log("SceneChange:AnchorFinished");

        SceneTransitionManager.Instance.OnTransitionProcessFinished(nextScene,info.Source);
    }

}