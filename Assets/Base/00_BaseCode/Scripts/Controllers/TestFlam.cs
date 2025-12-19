using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class TestFlam : MonoBehaviour
{


    public Animator animator;
    public RuntimeAnimatorController animatorControllerParameter_1;
    public RuntimeAnimatorController animatorControllerParameter_2;
    public RuntimeAnimatorController animatorControllerParameter_3;


    [Button]
    private void Test_1()
    {
        animator.runtimeAnimatorController = animatorControllerParameter_1;
        //animator.Play("Flame_1");
    }
    [Button]
    private void Test_2()
    {
        animator.runtimeAnimatorController = animatorControllerParameter_2;
        //animator.Play("Flame_2");
    }

    [Button]
    private void Test_3()
    {
        animator.runtimeAnimatorController = animatorControllerParameter_3;
        //animator.Play("Flame_3");
    }


}
