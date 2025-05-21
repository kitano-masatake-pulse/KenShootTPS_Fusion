using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 wasdInputDirection;
    public Vector3 cameraForward;
    public bool jumpPressed;

}