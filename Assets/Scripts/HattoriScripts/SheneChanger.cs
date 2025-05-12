using Fusion;
using UnityEngine;

public class SceneTransitionStarter : MonoBehaviour
{
    [Header("Network Runner�i�������������j")]
    private NetworkRunner runner;

    [Header("�J�ڐ�̃V�[����")]
    public string sceneName = "BattleScene"; // Build Settings �ɓo�^�ς݂̃V�[����

    public async void StartNetworkGame()
    {
        // ���ł�Runner�����邩�`�F�b�N
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        // �K�v�Ȃ���͌�����^����i�v���C���[���삪����ꍇ�j
        runner.ProvideInput = true;

        // �V�[���J�ڗp�̃f�t�H���g�}�l�[�W����ǉ�
        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "KenShootTPS_Fusion_DeathMatch",
            Scene = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity"), // �������͖����I�� index
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError("�Q�[���J�n�Ɏ��s���܂���: " + result.ShutdownReason);
        }
    }
}