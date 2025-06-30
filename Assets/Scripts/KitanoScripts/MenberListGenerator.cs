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

        Debug.Log($"GenerateMenberList called. runner:{_runner}");
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
        if (_runner != null)
        { Debug.Log($"NetworkRunner is not null. Runner:{_runner}"); }

        if (_runner.IsServer == false)
        {
            Debug.LogWarning("NetworkRunner is not a server. Cannot spawn member list.");
            return;
        }
        _runner.Spawn(
        memberListPrefab,
        Vector3.zero,
        Quaternion.identity,
        PlayerRef.None,
        // �� ������ OnBeforeSpawned �f���Q�[�g
        (runner, spawnedObj) => {
            spawnedObj.transform.SetParent(this.transform, false);
        }
    );

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
