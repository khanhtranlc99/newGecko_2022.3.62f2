using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public abstract class PackBase : MonoBehaviour
{
    public Text tvPrice;
    public Button btnBuy;
    public TypePackIAP typePackIAP;
    public Transform postText;
    public abstract void Init();
    public abstract void HandleBuy();
}
