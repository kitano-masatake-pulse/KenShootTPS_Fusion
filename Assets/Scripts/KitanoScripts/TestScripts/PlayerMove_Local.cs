using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove_Local : MonoBehaviour
{

    [SerializeField]
    private GameObject headObject;


    [SerializeField]
    private GameObject bodyObject;

    CharacterController characterController;

    public float moveSpeed = 3f;
    [SerializeField]
    private float gravity = -9.81f;

    private Transform tpsCameraTransform;

    private Vector3 velocity; //主に重力に使用


    // Start is called before the first frame update
    void Start()
    {
        TPSCameraController tpsCameraController = FindObjectOfType<TPSCameraController>();

        tpsCameraTransform = tpsCameraController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    public void ChangeTransformLocally()
    {
        // if (!HasInputAuthority) { return; }

        Vector3 cameraForward = tpsCameraTransform.forward;

        Vector3 bodyForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
        // ローカルプレイヤーの移動処理



        if (bodyForward.sqrMagnitude > 0.0001f)
        {
            // プレイヤー本体の向きをカメラ方向に回転
            bodyObject.transform.forward = bodyForward;
        }

        headObject.transform.up = cameraForward.normalized; // カメラの方向を頭の向きに設定(アバターの頭の軸によって変えること)


        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 moveDirection = Quaternion.LookRotation(bodyForward, Vector3.up) * inputDirection.normalized;





        // 重力を加算（ここを省略すれば浮く）
        velocity.y += gravity * Time.deltaTime;
        // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
        characterController.Move((moveDirection * moveSpeed + velocity) * Time.deltaTime);
        // 着地しているなら重力リセット
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
    }


}
