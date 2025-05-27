using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 wasdInputDirection;
    public Vector3 avatarPosition;
    public Vector3 avatarRotation;
    public Vector3 cameraForward;

}
