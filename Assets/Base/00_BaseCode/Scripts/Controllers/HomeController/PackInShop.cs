using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class PackInShop : MonoBehaviour
{
    public TypePackIAP typePackIAP;
    public Button btnBuy;
    public Text tvBuy;
    public Text tvBuy_2;

    public virtual void Init()
    {
        //tvBuy.text = "" + ;
    
        tvBuy.text =  "" + GameController.Instance.iapController.GetPrice(this.typePackIAP);
        tvBuy_2.text = "" + GameController.Instance.iapController.GetPrice(this.typePackIAP);
        btnBuy.onClick.AddListener(delegate { ButtonOnClick(); });
   


    }

    public void ButtonOnClick()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.iapController.BuyProduct(typePackIAP);
    }
      
}
