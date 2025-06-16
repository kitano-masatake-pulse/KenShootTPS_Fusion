using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMarkGenerator : MonoBehaviour
{

    //�V���O���g����
    public static BulletMarkGenerator Instance { get; private set; }

    [SerializeField]GameObject bulletMarkPrefab; // �e���̃v���n�u���A�T�C�����邽�߂̕ϐ�

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void GenerateBulletMark(Vector3 hitPosition)
    { 
    
        // �����ɒe���𐶐����鏈����ǉ�
        // �Ⴆ�΁A�e���̃v���n�u���C���X�^���X������hitPosition�ɔz�u����Ȃ�

        GameObject bulletMark = Instantiate(bulletMarkPrefab, hitPosition, Quaternion.identity);
        Debug.Log("Bullet mark generated at: " + hitPosition);



    }




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
