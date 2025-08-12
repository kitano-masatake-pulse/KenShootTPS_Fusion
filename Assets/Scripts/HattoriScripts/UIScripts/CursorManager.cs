using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    // ���݂̃V�[���́u��v�FUI�������ʏ펞�Ƀ��b�N����H
    bool gameplayLocksCursor = true;

    // UI������̈ꎞ�I�ȉ�����N�G�X�g�̏W���i�d���h�~�j
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
        // ��F�V�[�����ł�������ؑցi�{�Ԃ�ScriptableObject�Ŗ����w�肪�Y��j
        if (type.ToSceneName().Contains("Lobby") || type.ToSceneName().Contains("Battle"))
        {
            SetScenePolicy(true);   // ���b�N�
        }
        else
        {
            SetScenePolicy(false);  // �񃍃b�N�
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
