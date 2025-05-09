using UnityEngine;

public class SpinnerRotator : MonoBehaviour
{
    [SerializeField] private float stepAngle = 45f;         // ��]�p�x
    [SerializeField] private float stepInterval = 0.1f;      // ��]�̊Ԋu�i�b�j

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stepInterval)
        {
            transform.Rotate(0f, 0f, -stepAngle); // ���v���ɃJ�N�b�Ɖ�
            timer = 0f;
        }
    }
}
