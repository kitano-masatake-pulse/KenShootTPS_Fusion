using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMarkGenerator : MonoBehaviour
{

    //シングルトン化
    public static BulletMarkGenerator Instance { get; private set; }

    [SerializeField]GameObject bulletMarkPrefab; // 弾痕のプレハブをアサインするための変数

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
    
        // ここに弾痕を生成する処理を追加
        // 例えば、弾痕のプレハブをインスタンス化してhitPositionに配置するなど

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
