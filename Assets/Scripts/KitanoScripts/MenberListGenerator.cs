using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class MenberListGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] NetworkObject memberListPrefab; // �v���n�u�̎Q��

    void Awake()
    {
        // �v���n�u���ݒ肳��Ă��Ȃ��ꍇ�̓G���[���b�Z�[�W��\��
        if (memberListPrefab == null)
        {
            Debug.LogError("Member List Prefab is not assigned in the inspector.");
        }

        // NetworkRunner�̐�����҂��߂ɁAOnNetworkRunnerGenerated�C�x���g��o�^
        GameLauncher.OnNetworkRunnerGenerated -= GenerateMenberList;
        GameLauncher.OnNetworkRunnerGenerated += GenerateMenberList;
    }
    void GenerateMenberList(NetworkRunner _runner )
    { 

        if (memberListPrefab == null)
        {
            Debug.LogError("Member List Prefab is not assigned in the inspector.");
            return;
        }
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is null. Cannot spawn member list.");
            return;
        }
        if (_runner.IsServer == false)
        {
            Debug.LogWarning("This method should only be called on the server.");
            return;
        }

        NetworkObject menberlist = _runner.Spawn(
        memberListPrefab,
        //Vector3.zero,
        new Vector3(1, 1, 1), // �ʒu���w��
        Quaternion.identity,
        PlayerRef.None
        // �� ������ OnBeforeSpawned �f���Q�[�g
        //,(runner, spawnedObj) => {spawnedObj.transform.SetParent(this.transform, false); }

        

    );

        menberlist.gameObject.GetComponent<LobbyPingDisplay>().AddCallbackMe(_runner);

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
