using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 Direction;
    public NetworkButtons Buttons;

    public bool IsJumping;

}

public enum NetworkInputButtons
{
    Jump
}
