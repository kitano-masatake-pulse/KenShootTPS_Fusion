using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class PlayerFollowCamera : NetworkBehaviour
{
    private Transform cameraTransform;
    private bool shouldFollow = false;

    [SerializeField]
    private Vector3 cameraOffset = new Vector3(0, 5, -7); // �J�����ʒu����

    public override void Spawned()
    {
        // �����̃v���C���[���ǂ������m�F�iHasInputAuthority�j
        if (HasInputAuthority)
        {
            cameraTransform = Camera.main.transform;
            shouldFollow = true;

            cameraTransform.position = transform.position + cameraOffset;
            cameraTransform.LookAt(transform);
        }
    }

    void LateUpdate()
    {
        if (shouldFollow && cameraTransform != null)
        {
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                transform.position + cameraOffset,
                Time.deltaTime * 5f
            );
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
        }
    }
}
