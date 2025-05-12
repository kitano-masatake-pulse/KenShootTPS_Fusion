using Fusion;

public class PlayerAvatar : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // “ü—Í•ûŒü‚ÌƒxƒNƒgƒ‹‚ğ³‹K‰»‚·‚é
            data.Direction.Normalize();
            // “ü—Í•ûŒü‚ğˆÚ“®•ûŒü‚Æ‚µ‚Ä‚»‚Ì‚Ü‚Ü“n‚·
            characterController.Move(data.Direction);
        }
    }
}