using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTargetSprict : MonoBehaviour
{
    private Camera mainCamera; // �Ώۂ̃J����
    public Transform sphere;  // �ړ���������Sphere
    private float distanceFromCamera = 20f; // �J��������̋���

    private void Start()
    {
        if (mainCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                mainCamera = camObj.GetComponent<Camera>();
            }
            else
            {
                Debug.LogWarning("MainCamera �Ƃ������O�̃J������������܂���ł���");
            }
        }
    }

    void Update()
    {
        if (mainCamera == null || sphere == null) return;

        // �Q�[����ʂ̒������烏�[���h�ւ�Ray���΂�
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        // �J�������ʂɈ�苗���i�񂾈ʒu�����߂�
        Vector3 targetPosition = ray.origin + ray.direction * distanceFromCamera;

        // Sphere�������Ɉړ�
        sphere.position = targetPosition;
    }
}
