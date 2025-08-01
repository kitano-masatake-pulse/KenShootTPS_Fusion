using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class TeleportPositionSync : NetworkBehaviour
{
    [SerializeField, Tooltip("�ړ����x")]

    private NetworkTransform netTransform;

    public override void Spawned()
    {
        netTransform = GetComponent<NetworkTransform>();

        // �N���C�A���g������͂𑗂�
        if (Object.HasInputAuthority)
            Runner.ProvideInput = true;
    }

    void Update()
    {
        if (!Object.HasInputAuthority) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 3f);
    }

    public override void FixedUpdateNetwork()
    {
        //// �܂��z�X�g���ŕ����I�Ɉʒu���X�V
        //if (Object.HasStateAuthority)
        //{
        //    // �����Ńz�X�g���u��ԂȂ��v�̃e���|�[�g���������s
        //    netTransform.TeleportToPosition(transform.position);
        //}
    }
}
