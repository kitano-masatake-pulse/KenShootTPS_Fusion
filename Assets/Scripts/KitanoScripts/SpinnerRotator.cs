using UnityEngine;

public class SpinnerRotator : MonoBehaviour
{
    [SerializeField] private float stepAngle = 45f;         // 回転角度
    [SerializeField] private float stepInterval = 0.1f;      // 回転の間隔（秒）

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stepInterval)
        {
            transform.Rotate(0f, 0f, -stepAngle); // 時計回りにカクッと回す
            timer = 0f;
        }
    }
}
