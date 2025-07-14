using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationViewer : MonoBehaviour
{
    //シングルトン
    public static NotificationViewer Instance;
    [SerializeField] RectTransform panel;              // NotificationsPanel の RectTransform
    [SerializeField] NotificationMessage prefab;           // NotificationItem プレハブ
    [SerializeField] float displayTime = 3f;            // 表示時間
    [SerializeField] float fadeDuration = 0.5f;         // フェードアウト時間
    [SerializeField] int maxPoolSize = 10;              // プールサイズ

    Queue<NotificationMessage> pool = new Queue<NotificationMessage>();

    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject); // シーンをまたいで保持
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいで保持
        }
        else
        {
            Destroy(gameObject); // 複数生成されないように
        }

        // 初期プール生成
        for (int i = 0; i < maxPoolSize; i++)
        {
            var item = Instantiate(prefab, panel);
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
        }
    }

    public void ShowMessage(string message, bool isHost = false)
    {
        var item = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, panel);
        item.transform.SetAsLastSibling(); // 一番下に配置
        item.SetText(message,isHost);
        item.gameObject.SetActive(true);
        StartCoroutine(HideAfter(item));
    }

    IEnumerator HideAfter(NotificationMessage item)
    {
        yield return new WaitForSeconds(displayTime);
        // CanvasGroup を使ってフェード
        float t = 0;
        var cg = item.GetComponent<CanvasGroup>();
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = 1 - (t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0;
        // 後片付け
        item.gameObject.SetActive(false);
        cg.alpha = 1;
        pool.Enqueue(item);
    }
}
