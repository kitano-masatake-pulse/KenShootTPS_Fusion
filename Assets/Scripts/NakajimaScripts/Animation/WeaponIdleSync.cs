
using UnityEngine;

public class WeaponIdleSync : MonoBehaviour
{
    public Animator animator;

    // ���C���[�̃C���f�b�N�X
    private int upperLayer = 1; // �㔼�g
    private int lowerLayer = 0; // �����g

    private int GrenadeTagHash = Animator.StringToHash("IdleGrenade"); // �㔼�gBlendTree���i���m�Ɂj
    private int lowerIdleHash = Animator.StringToHash("GrenadeIdle");

    private bool lowerSynced = false;



    void Update()
    {
        //�����A�O���l�[�h��BlendTree�Ɉړ�������
        if (animator.GetCurrentAnimatorStateInfo(1).tagHash == GrenadeTagHash)
        {
            SyncUpperWithLower();
        }
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
