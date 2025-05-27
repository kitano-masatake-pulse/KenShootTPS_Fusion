using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 transform;
    public Vector3 cameraForward;
    public bool jumpPressed;
    public bool attackClicked;

}