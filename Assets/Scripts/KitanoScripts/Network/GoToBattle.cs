using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToBattle : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClickStartBattle()
    {
        // シーン遷移を行う
        NetworkSceneTransitionManager.Instance.ChangeScene(SceneType.Battle);
    }
}
