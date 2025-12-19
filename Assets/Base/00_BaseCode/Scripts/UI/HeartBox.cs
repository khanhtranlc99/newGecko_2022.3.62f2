using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeartBox : BaseBox
{
    private static HeartBox instance;
    public static HeartBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<HeartBox>(PathPrefabs.HEART_BOX));
            instance.Init();
        }

        instance.InitState();
        return instance;
    }
    [SerializeField] Button closeBtn;
    [SerializeField] Sprite onHeart;
    [SerializeField] List<Image> lsImgHeart;
    [SerializeField] Text txtTime;
    [SerializeField] Text txtCoinBuy;
    [SerializeField] Button btnBuy;
    [SerializeField] Button btnADS;
    [SerializeField] Text tvCountHeart;
    public void Init()
    {
        checkHeart();
        txtCoinBuy.text = 15.ToString();
        closeBtn.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound();  Close(); });
        btnADS.onClick.AddListener(ClickByAdsHeart);
        if (UseProfile.Heart < 5)
        {
          
            btnBuy.onClick.AddListener(OnclickBtnBuy);
        }
        else
        {
            btnBuy.interactable = false;
            btnADS.interactable = false;
            //txtTime.text = "FULL";
        }
    }
    public void InitState()
    {
        if (UseProfile.Heart >= 5)
        {
            btnBuy.interactable = false;
            btnADS.interactable = false;
        }
        tvCountHeart.text =  UseProfile.NumbWatchAdsHeart.ToString() + "/3";
    }
    private void checkHeart()
    {
        var temp = UseProfile.Heart;
        if(temp > lsImgHeart.Count )
        {
            temp = lsImgHeart.Count;
        }
        Debug.LogError("temp_" + temp);
        for (int i = 0; i < temp; i++)
        {
            lsImgHeart[i].sprite = onHeart;
        }
    }
    public void ClickByAdsHeart()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowVideoReward(
                     actionReward: () =>
                     {
                         

                         UseProfile.NumbWatchAdsHeart -= 1;
                         tvCountHeart.text = UseProfile.NumbWatchAdsHeart.ToString() + "/3";
                         if (UseProfile.NumbWatchAdsHeart <= 0)
                         {
                             UseProfile.NumbWatchAdsHeart = 3;
                             UseProfile.Heart++;
                             checkHeart();
                             InitState();
                             List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
                             giftRewardShows.Add(new GiftRewardShow() { amount = 1, type = GiftType.Heart });
                             PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });
                         }

                     },
                     actionNotLoadedVideo: () =>
                     {
                         GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                          (btnADS.transform,
                          btnADS.transform.position,
                          "No video at the moment!",
                          Color.white,
                          isSpawnItemPlayer: true
                          );
                     },
                     actionClose: null,
                     ActionWatchVideo.HeartInHearPopup,
                     UseProfile.CurrentLevel.ToString());
    }
    private void OnclickBtnBuy()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.Coin < 215)
        {
            ShopBox.Setup(ButtonShopType.Gold).Show();
            return;
        }
        UseProfile.Coin -= 15;
      
        UseProfile.Heart++;
        checkHeart();
        InitState();

        List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
        giftRewardShows.Add(new GiftRewardShow() { amount = 1, type = GiftType.Heart });
        PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });

    }

    private void Update()
    {
        if (UseProfile.Heart < 5)
        {
            txtTime.text = TimeManager.ShowTime2((long)GameController.Instance.heartGame.currentCoolDown);
        }
        else
        {
         
            txtTime.text = "FULL";
        }
    }
}
