using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationViewer : MonoBehaviour
{
    //�V���O���g��
    public static NotificationViewer Instance;
    [SerializeField] RectTransform panel;              // NotificationsPanel �� RectTransform
    [SerializeField] NotificationMessage prefab;           // NotificationItem �v���n�u
    [SerializeField] float displayTime = 3f;            // �\������
    [SerializeField] float fadeDuration = 0.5f;         // �t�F�[�h�A�E�g����
    [SerializeField] int maxPoolSize = 10;              // �v�[���T�C�Y

    Queue<NotificationMessage> pool = new Queue<NotificationMessage>();

    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject); // �V�[�����܂����ŕێ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����܂����ŕێ�
        }
        else
        {
            Destroy(gameObject); // ������������Ȃ��悤��
        }

        // �����v�[������
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
        item.transform.SetAsLastSibling(); // ��ԉ��ɔz�u
        item.SetText(message,isHost);
        item.gameObject.SetActive(true);
        StartCoroutine(HideAfter(item));
    }

    IEnumerator HideAfter(NotificationMessage item)
    {
        yield return new WaitForSeconds(displayTime);
        // CanvasGroup ���g���ăt�F�[�h
        float t = 0;
        var cg = item.GetComponent<CanvasGroup>();
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = 1 - (t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0;
        // ��Еt��
        item.gameObject.SetActive(false);
        cg.alpha = 1;
        pool.Enqueue(item);
    }
}
