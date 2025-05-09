using UnityEngine;

public class SpinnerRotator : MonoBehaviour
{
    [SerializeField] private float stepAngle = 45f;         // ‰ñ“]Šp“x
    [SerializeField] private float stepInterval = 0.1f;      // ‰ñ“]‚ÌŠÔŠui•bj

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stepInterval)
        {
            transform.Rotate(0f, 0f, -stepAngle); // Œv‰ñ‚è‚ÉƒJƒNƒb‚Æ‰ñ‚·
            timer = 0f;
        }
    }
}
