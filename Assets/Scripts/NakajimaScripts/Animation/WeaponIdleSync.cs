
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
        bool isBlendTreeState = (animator.GetCurrentAnimatorStateInfo(0).tagHash == animator.GetCurrentAnimatorStateInfo(1).tagHash) && animator.GetCurrentAnimatorStateInfo(1).tagHash != 0;
        bool isTest = (animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f) - (animator.GetCurrentAnimatorStateInfo(1).normalizedTime % 1f) < 0.001;
        if (isBlendTreeState && !isTest)
        {
            ResetLayerState(upperLayer);
            ResetLayerState(lowerLayer);
        }
        //if (isBlendTreeState && !hasEnteredIdleState)
        //{
        //    Debug.Log("�㔼�g�E�����g�̍Đ��ʒu�����Z�b�g");

        //    ResetLayerState(upperLayer);
        //    ResetLayerState(lowerLayer);
        //    hasEnteredIdleState=true;
        //}
        //if (!isBlendTreeState && hasEnteredIdleState)
        //{
        //    hasEnteredIdleState = false;
        //}
    }
    void ResetLayerState(int layerIndex)
    {
        // ���݂̃X�e�[�g�����擾
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // �����X�e�[�g�ɍēx�J�ځinormalizedTime = 0f�j
        animator.Play(stateInfo.fullPathHash, layerIndex, 0f);
    }


}
