using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 wasdinputDirection;
    public Vector3 cameraForward;
    public bool jumpPressed;
    public bool attackClicked;

}