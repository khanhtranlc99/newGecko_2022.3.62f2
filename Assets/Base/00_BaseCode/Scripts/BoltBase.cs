using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine.UI;
public class BoltBase : MonoBehaviour
{
    public Animator animator;
    public string name;
    public void Start()
    {
        animator.Play(name);
    }
}
