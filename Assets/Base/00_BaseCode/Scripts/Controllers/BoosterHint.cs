using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Org.BouncyCastle.Math.Field;

public class BoosterHint : MonoBehaviour
{
    public Button btnHint_Booster;
    public Text tvNum;
    public GameObject objNum;
    public GameObject objAds;
 
    public bool wasUseHint_Booster;

    public HandHint handTut;

    public Transform postCanvas;

    public HandHint currentHandHint;




    public void Init()
    {
        
        wasUseHint_Booster = false;   
       
            HandleUnlock();     
    


        void HandleUnlock()
        {
            btnHint_Booster.onClick.AddListener(HandleHint_Booster);
            if (UseProfile.Hint_Booster > 0)
            {
                objNum.SetActive(true);
                tvNum.text = UseProfile.Hint_Booster.ToString();
                objAds.SetActive(false);
            }
            else
            {
                objNum.SetActive(false);
                tvNum.gameObject.SetActive(false);
                objAds.SetActive(true);
         
            }
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHECK_HAND_BOOSTER, ChangeCurrentHand);
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_HINT_BOOSTER, ChangeText);

        }
        void HandleLock()
        {
          
            
            btnHint_Booster.onClick.AddListener(HandleLockBtn);
        }
    }

    public void HandleLockBtn()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                              (
                              btnHint_Booster.transform.position,
                              "Unlock at level 3",
                              Color.white,
                              isSpawnItemPlayer: true
                              );
    }


    public void HandleHint_Booster()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if(currentHandHint != null)
        {
            return;
        }

        if (UseProfile.Hint_Booster >= 1)
        {      
            UseProfile.Hint_Booster -= 1;         
            wasUseHint_Booster = true;
            btnHint_Booster.interactable = false;
            HandleHint();
          //  GamePlayController.Instance.tutHintBooster.NextTut();
        }
        else
        {
         GameController.Instance.admobAds.ShowVideoReward(
                     actionReward: () =>
                     {
                         UseProfile.Hint_Booster += 3;                        
                         List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
                         giftRewardShows.Add(new GiftRewardShow() { amount = 3, type = GiftType.Hint_Booster });
                         PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });
                     },
                     actionNotLoadedVideo: () =>
                     {
                         GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                          (
                         
                          btnHint_Booster.transform.position,
                          "No video at the moment!",
                          Color.white,
                          isSpawnItemPlayer: true
                          );
                     },
                     actionClose: null,
                       ActionWatchVideo.Hint,
                     UseProfile.CurrentLevel.ToString());
        }


    }

    public void HandlteTut()
    {
        wasUseHint_Booster = true;
        btnHint_Booster.interactable = false;
        HandleHint();
    }    




 public void HandleHint()
{
    
 
}

 
    public void ChangeText(object param)
    {
      
        tvNum.text = UseProfile.Hint_Booster.ToString();
        if (UseProfile.Hint_Booster > 0)
        {
            objNum.SetActive(true);
            tvNum.gameObject.SetActive(true);
            tvNum.text = UseProfile.Hint_Booster.ToString();
            objAds.SetActive(false);
        }
        else
        {
            objNum.SetActive(false);
            tvNum.gameObject.SetActive(false);
            objAds.SetActive(true);
 
        }
      
    }

    public void ChangeCurrentHand(object param)
    {
        if (currentHandHint != null)
        {
            btnHint_Booster.interactable = true;
            SimplePool2.Despawn(currentHandHint.gameObject);
            currentHandHint = null;
        }
       
    }    


    public void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHECK_HAND_BOOSTER, ChangeCurrentHand);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_HINT_BOOSTER, ChangeText);
    }

 
}
