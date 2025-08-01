using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AttackAnimationData data;
    private float attackSpeed;

    

    public void Start()
    {
        AttackAnimationData[] allData = Resources.LoadAll<AttackAnimationData>("AttackAnimationDatas");
        foreach (var data in allData)
        {
            Debug.Log($"Loaded AttackAnimationData: {data.name}, Duration: {data.totalDuration}");
            attackSpeed = data.attackClip.length / data.totalDuration;
            if (data.name == "Prepare to throw")
            {
                animator.SetFloat("PrepareToThrow", attackSpeed);
                Debug.Log($"Set Attack Speed for {data.name}: {attackSpeed}");
            }
            if (data.name == "Throw")
            {
                animator.SetFloat("Throw", attackSpeed);
                Debug.Log($"Set Attack Speed for {data.name}: {attackSpeed}");
            }

        }

    }
}
