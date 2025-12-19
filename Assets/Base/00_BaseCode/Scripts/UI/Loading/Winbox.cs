using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class Winbox : BaseBox
{
    public static Winbox _instance;
    public static Winbox Setup()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<Winbox>(PathPrefabs.WIN_BOX));
            _instance.Init();
        }
        _instance.InitState();
        return _instance;
    }

    public Button nextButton;
 
    public void Init()
    {
        nextButton.onClick.AddListener(delegate { HandleNext();    });
        UseProfile.CurrentLevel += 1;
        if(UseProfile.CurrentLevel >= 150)
        {
            UseProfile.CurrentLevel = 150;
        }
    }   
    public void InitState()
    {

         

     
    }    
    private void HandleNext()
    {
        GameController.Instance.musicManager.PlayClickSound();
       Close();
            Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f);
     
       
    }
    private void HandleReward()
    {
        //GameController.Instance.musicManager.PlayClickSound();
        //GameController.Instance.admobAds.ShowVideoReward(
        //           actionReward: () =>
        //           {
        //               Close();
        //               //GameController.Instance.admobAds.HandleHideMerec();
                    
        //               List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
        //               giftRewardShows.Add(new GiftRewardShow() { amount = 1, type = GiftType.Coin });
        //               PopupRewardBase.Setup(false).Show(giftRewardShows, delegate {
        //                   PopupRewardBase.Setup(false).Close();
        //                   Initiate.Fade("GamePlay", Color.black, 2f);
        //               });

        //           },
        //           actionNotLoadedVideo: () =>
        //           {
        //               GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
        //                (rewardButton.transform,
        //                rewardButton.transform.position,
        //                "No video at the moment!",
        //                Color.white,
        //                isSpawnItemPlayer: true
        //                );
        //           },
        //           actionClose: null,
        //           ActionWatchVideo.WinBox_Claim_Coin,
        //           UseProfile.CurrentLevel.ToString());
    }
    private void OnDestroy()
    {
        
    }
}
