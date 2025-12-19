using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SuggetBox : BaseBox
{
    public static SuggetBox _instance;
    public static SuggetBox Setup(GiftType giftType, bool isBoosterTut = false)
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<SuggetBox>(PathPrefabs.SUGGET_BOX));
            _instance.Init();
        }
        _instance.InitState( giftType, isBoosterTut);
        return _instance;
    }

    public Button btnClose;
    public Button payByCoinBtn;
    public Button payByAdsBtn;
    public Text tvTitler;
    public Text tvContent;
    public Text tvPrive;
    public int price;
    GiftType currentGift;
    ActionWatchVideo actionWatchVideo;
    public Image iconDecor;
    public Text tvNum;
    public Text tvCountNumbAds;
    public GameObject iconAds;
    public void Init()
    {
        btnClose.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); Close(); });
   
        payByCoinBtn.onClick.AddListener(delegate { HandlePayByCoin(); });
    }

    public void InitState(GiftType giftType, bool isTut)
    {
        currentGift = giftType;
        switch (giftType)
        {
            case GiftType.TNT_Booster:
                tvTitler.text = "TNT BOOM";
                tvContent.text = "Create a 3x3 wide explosion";
                price = 150;
                tvPrive.text = price.ToString();
                actionWatchVideo = ActionWatchVideo.TNT_Booster;
                payByAdsBtn.onClick.RemoveAllListeners();
                payByAdsBtn.onClick.AddListener(delegate { HandlePayByAds(); });
                iconAds.SetActive(true);
                tvCountNumbAds.text = UseProfile.NumbWatchAdsTNT.ToString() + "/3";
                break;
            case GiftType.Rocket_Booster:
                tvTitler.text = "Rocket";
                tvContent.text = "Shoots 1 random slime";
                price = 200;
                tvPrive.text = price.ToString();
                actionWatchVideo = ActionWatchVideo.Rocket_Booster;
                payByAdsBtn.onClick.RemoveAllListeners();
                payByAdsBtn.onClick.AddListener(delegate { HandlePayByAds(); });
                iconAds.SetActive(true);
                tvCountNumbAds.text = UseProfile.NumbWatchAdsRocket.ToString() + "/3";
                break;
            case GiftType.Freeze_Booster:
                tvTitler.text = "Freeze";
                tvContent.text = "Freeze all slimes";
                price = 300;
                tvPrive.text = price.ToString();
                actionWatchVideo = ActionWatchVideo.Freeze_Booster;
                payByAdsBtn.onClick.RemoveAllListeners();
                payByAdsBtn.onClick.AddListener(delegate { ShopBox.Setup(ButtonShopType.Gold).Show(); });
                iconAds.SetActive(false);
                tvCountNumbAds.text =  "Shop";
                break;
            case GiftType.Atom_Booster:
                tvTitler.text = "Atom";
                tvContent.text = "Create a Big explosion";
                price = 700;
                tvPrive.text = price.ToString();
                actionWatchVideo = ActionWatchVideo.Atom_Booste;
                payByAdsBtn.onClick.RemoveAllListeners();
                payByAdsBtn.onClick.AddListener(delegate { ShopBox.Setup(ButtonShopType.Gold).Show(); });
                iconAds.SetActive(false);
                tvCountNumbAds.text = "Shop";
                break;
        }
        iconDecor.sprite = GameController.Instance.dataContain.giftDatabase.GetIconItem(giftType);
        iconDecor.SetNativeSize();
        if (isTut)
        {
            payByAdsBtn.gameObject.SetActive(false);
            payByCoinBtn.gameObject.SetActive(false);
            tvNum.gameObject.SetActive(false);
        }    
        else
        {
            payByAdsBtn.gameObject.SetActive(true);
            payByCoinBtn.gameObject.SetActive(true);
            tvNum.gameObject.SetActive(true);

        }
     }


    public void HandlePayByAds()
    {

        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowVideoReward(
                     actionReward: () =>
                     {
                         switch (currentGift)
                         {
                             case GiftType.TNT_Booster:
                                 UseProfile.NumbWatchAdsTNT -= 1;
                                 if (UseProfile.NumbWatchAdsTNT <= 0)
                                 {
                                     UseProfile.NumbWatchAdsTNT = 3;
                                     HandleClaimGiftX1();
                                   
                                 }
                                 tvCountNumbAds.text = UseProfile.NumbWatchAdsTNT.ToString() + "/3";
                                 break;
                             case GiftType.Rocket_Booster:

                                 UseProfile.NumbWatchAdsRocket -= 1;
                                 if (UseProfile.NumbWatchAdsRocket <= 0)
                                 {
                                     UseProfile.NumbWatchAdsRocket = 3;
                                     HandleClaimGiftX1();

                                 }
                                 tvCountNumbAds.text = UseProfile.NumbWatchAdsRocket.ToString() + "/3";
                                 break;
                         }
                            


                     },
                     actionNotLoadedVideo: () =>
                     {
                         GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                          (
                             payByAdsBtn.transform
                             ,
                          payByAdsBtn.transform.position,
                          "No video at the moment!",
                          Color.white,
                          isSpawnItemPlayer: true
                          );
                     },
                     actionClose: null,
                     actionWatchVideo,
                     UseProfile.CurrentLevel.ToString());
    }   
    
    public void HandlePayByCoin()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.Coin >= price)
        {
            UseProfile.Coin -= price;      
            HandleClaimGift();
        }
        else
        {
            ShopBox.Setup(ButtonShopType.Gold).Show();
        }    


    }  
    

    public void HandleClaimGift()
    {
   
         Close();
        GameController.Instance.dataContain.giftDatabase.Claim(currentGift, 1);
        List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
        giftRewardShows.Add(new GiftRewardShow() { amount = 1, type = currentGift });
        PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });

    }
    public void HandleClaimGiftX1()
    {

    
        Close();
        GameController.Instance.dataContain.giftDatabase.Claim(currentGift, 1);
        List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
        giftRewardShows.Add(new GiftRewardShow() { amount = 1, type = currentGift });
        PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });

    }
}
