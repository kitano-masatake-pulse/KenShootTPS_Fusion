using UnityEngine;

//�A�^�b�`����UI���J�����̕����Ɍ�����X�N���v�g
public class Billboard : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        // ���C���J�������L���b�V��
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Canvas �̑O�ʁiforward�j���J�����̕����ɍ��킹��
        transform.rotation = mainCamera.transform.rotation;
    }
}