using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    // 現在のシーンの「基準」：UIが無い通常時にロックする？
    [SerializeField]
    bool gameplayLocksCursor = true;

    // UI等からの一時的な解放リクエストの集合（重複防止）
    readonly HashSet<object> uiHolders = new HashSet<object>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;this.transform.SetParent(null); DontDestroyOnLoad(gameObject);
        SceneTransitionManager.OnSceneLoad += OnSceneChanged;
    }
    void OnDestroy()
    {
        if (Instance == this) SceneTransitionManager.OnSceneLoad -= OnSceneChanged;
    }

    void OnSceneChanged(SceneType type)
    {
        Debug.Log($"CursorManager: OnSceneChanged({type})");
        // 例：シーン名でざっくり切替（本番はScriptableObjectで明示指定が綺麗）
        if (type.ToSceneName().Contains("Lobby") || type.ToSceneName().Contains("Battle"))
        {
            SetScenePolicy(true);   // ロック基準
            Debug.Log("CursorManager: ロック基準を設定しました。");
        }
        else
        {
            SetScenePolicy(false);  // 非ロック基準
            Debug.Log("CursorManager: 非ロック基準を設定しました。");
        }
    }

    public void SetScenePolicy(bool locksInGameplay)
    {
        gameplayLocksCursor = locksInGameplay;
        Recalc();
    }

    public void RequestUI(object owner)
    {
        uiHolders.Add(owner);
        Recalc();
    }
    public void ReleaseUI(object owner)
    {
        uiHolders.Remove(owner);
        Recalc();
    }

    void Recalc()
    {
        bool anyUI = uiHolders.Count > 0;
        if (anyUI)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            if (gameplayLocksCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
