using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class PackInShopAds : PackInShop
{
  
    public GiftType currentGift;
    ActionWatchVideo actionWatchVideo;
    public GameObject decorBtn;
    public int numGift = 1;
 
   
    //public  bool WasWatch
    //{
    //    get
    //    {

    //        return PlayerPrefs.GetInt("WasWatchPackInShopAds"   + currentGift.ToString(), 0) == 1;
    //    }
    //    set
    //    {
    //        PlayerPrefs.SetInt("WasWatchPackInShopAds" + currentGift.ToString(), value ? 1 : 0);
    //        PlayerPrefs.Save();
    //    }
    //}
    public override void Init()
    {
        btnBuy.onClick.AddListener(HandleOnClick);
        ShowCount();
    }
    private void ShowCount()
    {
        switch (currentGift)
        {
            case GiftType.TNT_Booster:

                tvBuy.text = UseProfile.NumbWatchAdsTNT.ToString() + "/3";
                tvBuy_2.text = UseProfile.NumbWatchAdsTNT.ToString() + "/3";
                break;
            case GiftType.Rocket_Booster:
                tvBuy.text = UseProfile.NumbWatchAdsRocket.ToString() + "/3";
                tvBuy_2.text = UseProfile.NumbWatchAdsRocket.ToString() + "/3";

                break;
            case GiftType.Heart:
                tvBuy.text = UseProfile.NumbWatchAdsHeart.ToString() + "/3";
                tvBuy_2.text = UseProfile.NumbWatchAdsHeart.ToString() + "/3";

                break;
            case GiftType.Coin:

                tvBuy.text = UseProfile.NumbWatchAdsCoin.ToString() + "/3";
                tvBuy_2.text = UseProfile.NumbWatchAdsCoin.ToString() + "/3";
                break;
        }
    }    
    private void HandleAfterWatchVideo()
    {
        switch (currentGift)
        {
            case GiftType.TNT_Booster:
                UseProfile.NumbWatchAdsTNT -= 1;
                if (UseProfile.NumbWatchAdsTNT <= 0)
                {
                    Claim(delegate { UseProfile.NumbWatchAdsTNT = 3; ShowCount();   });
                }    


                break;
            case GiftType.Rocket_Booster:
                UseProfile.NumbWatchAdsRocket -= 1;
                if (UseProfile.NumbWatchAdsRocket <= 0)
                {
                    Claim(delegate { UseProfile.NumbWatchAdsRocket = 3; ShowCount(); });
                }

                break;
            case GiftType.Heart:
                UseProfile.NumbWatchAdsHeart -= 1;
                if (UseProfile.NumbWatchAdsHeart <= 0)
                {
                    Claim(delegate { UseProfile.NumbWatchAdsHeart = 3; ShowCount(); });
                }

                break;
            case GiftType.Coin:

                UseProfile.NumbWatchAdsCoin -= 1;
                if (UseProfile.NumbWatchAdsCoin <= 0)
                {
                    Claim(delegate { UseProfile.NumbWatchAdsCoin = 3; ShowCount(); });
                }

                break;
        }
        ShowCount();
    }    

    private void HandleOnClick()
    {
        switch (currentGift)
        {
            case GiftType.TNT_Booster:

                actionWatchVideo = ActionWatchVideo.TNT_Booster;
                break;
            case GiftType.Rocket_Booster:

                actionWatchVideo = ActionWatchVideo.Rocket_Booster;
                break;
            case GiftType.Freeze_Booster:

                actionWatchVideo = ActionWatchVideo.Freeze_Booster;
                break;
            case GiftType.Atom_Booster:

                actionWatchVideo = ActionWatchVideo.Atom_Booste;
                break;
        }
        GameController.Instance.musicManager.PlayClickSound();

        GameController.Instance.admobAds.ShowVideoReward(
                actionReward: () =>
                {


                    HandleAfterWatchVideo();

                },
                actionNotLoadedVideo: () =>
                {
                    btnBuy.transform.SetAsLastSibling();
                    GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                     (
                        btnBuy.transform
                        ,
                     btnBuy.transform.position,
                     "No video",
                     Color.white,
                     isSpawnItemPlayer: true
                     );
                },
                actionClose: null,
                actionWatchVideo,
                UseProfile.CurrentLevel.ToString());
    }
      


        void Claim(Action callBack)
        {
        
            List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
            giftRewardShows.Add(new GiftRewardShow() { amount = numGift, type = currentGift });
            foreach(var item in giftRewardShows)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(item.type, item.amount);
            }    
            PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { callBack?.Invoke(); });
      
          
        }



     
    }    

 