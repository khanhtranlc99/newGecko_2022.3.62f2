using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutGamePlay_Step_1 : TutorialBase
{
  
    GameObject currentHand;
    public Button btnHome;
 
    public override bool IsCanEndTut()
    {
        if(currentHand != null)
        {
            currentHand.transform.DOKill();
            Destroy(currentHand.gameObject);
        }

     return true; 
    }

 

    public override void StartTut()
    {
        if(UseProfile.LevelEggChest == 1)
        {
            btnHome.gameObject.SetActive(false);
            StartCoroutine(SpawnHand());
        }    
        

        
       
    }


    IEnumerator  SpawnHand()
    {
        yield return new WaitForSeconds(0.5f);
      
    }    
 



   
    protected override void SetNameTut()
    {
     
    }
    public override void OnEndTut()
    {
      
    }
}
