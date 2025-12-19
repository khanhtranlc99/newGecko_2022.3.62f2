using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoseBox : BaseBox
{
    public static LoseBox _instance;
    public static LoseBox Setup()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<LoseBox>(PathPrefabs.LOSE_BOX));
            _instance.Init();
        }
        _instance.InitState();
        return _instance;
    }
 
    public Button btnRetry;
    public Button btnAds;
  

 

    public void Init()
    {
        btnRetry.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); HandleClose(); });
 
     
       
   
       
    }   
    public void InitState()
    {
        GameController.Instance.AnalyticsController.LoseLevel(UseProfile.CurrentLevel);
    }
     
    // public void HandleAdsRevive()
    // {
    //     GameController.Instance.musicManager.PlayClickSound();
    //     GameController.Instance.admobAds.ShowVideoReward(
    //                 actionReward: () =>
    //                 {
                       
    //                         Close();
                  
                  
    //                 },
    //                 actionNotLoadedVideo: () =>
    //                 {
    //                     GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
    //                      (btnAdsRevive.transform
    //                         ,
    //                      btnAdsRevive.transform.position,
    //                      "No video at the moment!",
    //                      Color.white,
    //                      isSpawnItemPlayer: true
    //                      );
    //                 },
    //                 actionClose: null,
    //                 ActionWatchVideo.ReviveFreeLoseBox,
    //                 UseProfile.CurrentLevel.ToString());



    // }
    public void HandleClose()
    {
      //  GameController.Instance.musicManager.PlayClickSound();
        //Close();
        GamePlayController.Instance.playerContain.isPopupUp = false;
        Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f);

    }

}
