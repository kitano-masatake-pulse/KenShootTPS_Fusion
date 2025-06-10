using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLinePoolManager : MonoBehaviour
{
    public static RaycastLinePoolManager Instance { get; private set; }

    [Header("Line Renderer �v���n�u�i�ׂ����j")]
    [SerializeField] private LineRenderer linePrefab;

    [Header("�v�[���T�C�Y")]
    [SerializeField] private int poolSize = 50;

    [Header("���̑���")]
    [SerializeField] private float defaultWidth = 0.05f;

    private readonly Queue<LineRenderer> pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var line = Instantiate(linePrefab, transform);
            line.positionCount = 2;
            line.startWidth = defaultWidth;
            line.endWidth = defaultWidth;
            line.gameObject.SetActive(false);
            pool.Enqueue(line);
        }
    }

    /// <summary>
    /// Ray ����莞�ԕ\������
    /// </summary>
    public void ShowRay(Vector3 start, Vector3 end, Color? color ,float duration = 0.5f)
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("RayLine Pool is empty!");
            return;
        }

        var line = pool.Dequeue();
        line.gameObject.SetActive(true);
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        // �F��ݒ�i�w�肪����΂��̐F�A�Ȃ����red�j
        Color finalColor = color ?? Color.red;
        line.startColor = finalColor;
        line.endColor = finalColor;

        StartCoroutine(ReleaseAfter(line, duration));
    }

    private IEnumerator ReleaseAfter(LineRenderer line, float delay)
    {
        yield return new WaitForSeconds(delay);
        line.gameObject.SetActive(false);
        pool.Enqueue(line);
    }
}
