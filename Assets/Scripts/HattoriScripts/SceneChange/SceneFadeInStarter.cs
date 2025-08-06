using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeInStarter : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.sceneLoaded -= StartScene;
        SceneManager.sceneLoaded += StartScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= StartScene; // シーンロード時のイベントを解除
    }
    private void StartScene(Scene scene, LoadSceneMode mode)
    {
        SceneTransitionManager.Instance.StartScene();
    }
}
