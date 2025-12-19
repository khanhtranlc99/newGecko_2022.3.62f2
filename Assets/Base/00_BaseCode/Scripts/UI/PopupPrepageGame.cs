using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
public class PopupPrepageGame : MonoBehaviour
{
    public Text tvLevel;
    public Text tvTotoScore;
    public CanvasGroup canvasGroup;
    public GameObject parent;
    public AudioClip tingTing;
    public void Init(Action callBack)
    {

        canvasGroup.alpha = 0;

        canvasGroup.DOFade(1, 0.5f).OnComplete(delegate
        {

            tvLevel.text = "LEVEL " + UseProfile.CurrentLevel;

            parent.transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
            {


                HandleOff(callBack);

            });
        });
     

  
   
    }
    public void Init(Action callBack, bool noCanvas)
    {
        canvasGroup.alpha = 1;
    
            tvLevel.text = "LEVEL " + UseProfile.CurrentLevel;

            parent.transform.DOScale(new Vector3(1, 1, 1), 0.5f).SetEase(Ease.OutBack).OnComplete(delegate {


                HandleOff(callBack);

            });
    




    }
    public void HandleOff(Action callBack )
    {
       
      
        canvasGroup.DOFade(0, 0.5f).SetDelay(1.5f).OnComplete(delegate {
            callBack?.Invoke();
            this.gameObject.SetActive(false);

            GameController.Instance.musicManager.PlayOneShot(tingTing);
        });
    }


}
