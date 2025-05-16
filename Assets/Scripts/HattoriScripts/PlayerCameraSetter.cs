using UnityEngine;
using Fusion;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerCameraSetter : NetworkBehaviour
{
    [SerializeField]
    public Transform cameraFollowTarget; // ��: �w���̈ʒu�Ȃǂɋ�̎q�I�u�W�F�N�g��z�u���Ďw��

    void Start()
    {
        //�o�g���V�[���ł̂ݎ��s
        string sceneName = SceneType.Battle.ToSceneName();
        if (HasInputAuthority && SceneManager.GetActiveScene().name == sceneName)
        {
            // �V�[���ɂ���VirtualCamera���擾
            var vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = cameraFollowTarget;
                vcam.LookAt = cameraFollowTarget;
                Debug.Log("Cinemachine�J�������v���C���[�ɒǏ]�����܂���");
            }
        }
    }
}
