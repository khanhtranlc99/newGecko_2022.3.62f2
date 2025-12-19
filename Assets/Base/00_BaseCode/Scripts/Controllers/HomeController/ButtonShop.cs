using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum ButtonShopType
{
    Gift,
    Booster,
    Gold,
    None
}
public class ButtonShop : MonoBehaviour
{
    public ButtonShopController controller;
    public ButtonShopType buttonShopType;
    public Button btn;
    public Animator animator;
    public string strOn;
    public string strOff;
   
    
    public void Init ()
    {
       
        btn.onClick.AddListener(delegate { controller.HandleOnClick(buttonShopType); });
    }
 
    
    public void HandleOn()
    {
        
            animator.Play(strOn);
      
     
    }
    public void HandleOff()
    {
        animator.Play(strOff);
    }

  


}
