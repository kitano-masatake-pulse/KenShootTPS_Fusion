using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

//静的に配置したいNetoworkObjectを生成するためのスクリプト
//再接続時の同期のために動的製紙絵を行う
public class LobbyNetworkObjectGenerator : MonoBehaviour
{

    public List<NetworkObject> spawnedNetworkObjects = new List<NetworkObject>();
    // Start is called before the first frame update


    private void OnEnable()
    {

        GameLauncher.OnNetworkRunnerGenerated += SpawnObjects;
    }

    void SpawnObjects(NetworkRunner runner )
    {
        if (!runner.IsServer) { return; }

        foreach (var obj in spawnedNetworkObjects)
        {
            if (obj == null) { continue; }
            Debug.Log("Spawned " + obj.name);
            runner.Spawn(obj, obj.transform.position, obj.transform.rotation, null);
        }




    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
