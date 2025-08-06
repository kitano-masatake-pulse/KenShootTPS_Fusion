
using UnityEngine;

public class WeaponIdleSync : MonoBehaviour
{
    public Animator animator;

    // ���C���[�̃C���f�b�N�X
    private int upperLayer = 1; // �㔼�g
    private int lowerLayer = 0; // �����g

    private int BlendTreeTagHash = Animator.StringToHash("BlendTree"); // �㔼�gBlendTree���i���m�Ɂj

    private bool hasEnteredIdleState = false;




    void Update()
    {
        //�����A�����[�h�═��؂�ւ��Ȃǂ̓���̃A�j���[�V�������I�������A�㉺���g�̃X�e�C�g��0�ɂ���
        //��邱�ƁB���񂱂̃X�e�C�g�J�ڂ���1��ڂ������s���鏈��
        //�A�j���[�^�[���Ń^�O��ύX�BIK�Ń^�O���g���Ȃ��Ȃ�̂ŁA�O���l�[�h�̂悤�Ɋ֐��ŕ�����i�A�j���[�V�����n���h���[�j
        bool isBlendTreeState = animator.GetCurrentAnimatorStateInfo(1).tagHash == BlendTreeTagHash;
        if (isBlendTreeState && !hasEnteredIdleState)
        {
            Debug.Log("�㔼�g�E�����g�̍Đ��ʒu�����Z�b�g");

            ResetLayerState(upperLayer);
            ResetLayerState(lowerLayer);
            hasEnteredIdleState=true;
        }
        if (!isBlendTreeState && hasEnteredIdleState)
        {
            hasEnteredIdleState = false;
        }
    }
    void ResetLayerState(int layerIndex)
    {
        // ���݂̃X�e�[�g�����擾
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // �����X�e�[�g�ɍēx�J�ځinormalizedTime = 0f�j
        animator.Play(stateInfo.fullPathHash, layerIndex, 0f);
    }

    void SyncUpperWithLower()
    {
        //�����g��BlendTree�ŁA���ݓ����Ă���A�j���[�V������normalizedTime���擾����
        Debug.Log("�㔼�g���O���l�[�h��BlendTree�Ɉړ����� �� �����g������������");

        Vector2 lowerDir = new Vector2(
        animator.GetFloat("Horizontal"),
        animator.GetFloat("Vertical")
        );

        //�����g��BlendTree�̕������擾
        string direction = GetCurrentBlendDirection(lowerDir);
        Debug.Log($"�����g��Blend����: {direction}");

        //���K�����ꂽ���Ԃ��擾
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(lowerLayer).normalizedTime % 1f;

        //�㔼�g��BlendTree���A�����g��BlendTree�Ɠ����A�j���[�V�����ɑJ�ڂ�����
        int upperStateHash = direction switch
        {
            "Idle" => Animator.StringToHash("empty idle"),
            "Forward" => Animator.StringToHash("empty running"),
            "Back" => Animator.StringToHash("empty running back"),
            "Left" => Animator.StringToHash("empty left strafe"),
            "Right" => Animator.StringToHash("empty right strafe"),
            _ => Animator.StringToHash("empty idle")
        };
        animator.Play(Animator.StringToHash("empty running"), upperLayer, normalizedTime);
    }

    string GetCurrentBlendDirection(Vector2 dir)
    {
        if (dir.magnitude < 0.1f)
            return "Idle";

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? "Right" : "Left";
        }
        else
        {
            return dir.y > 0 ? "Forward" : "Back";
        }
    }
}
