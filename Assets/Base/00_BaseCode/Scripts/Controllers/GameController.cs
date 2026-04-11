using MoreMountains.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Security.Cryptography;
using System.Text;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif



public class GameController : MonoBehaviour
{
    
    public static GameController Instance;

    public MoneyEffectController moneyEffectController;
    public UseProfile useProfile;
    public DataContain dataContain;
    public MusicManagerGameBase musicManager;
    public AdmobAdsGoogle admobAds;

    public AnalyticsController AnalyticsController;
    public IapController iapController;
    public HeartGame heartGame;
    [HideInInspector] public SceneType currentScene;
 
    public StartLoading startLoading;

    protected void Awake()
    {
        Instance = this;
        Init();

        DontDestroyOnLoad(this);

       // GameController.Instance.useProfile.IsRemoveAds = true;


#if UNITY_IOS

    if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
    ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
    {

        ATTrackingStatusBinding.RequestAuthorizationTracking();

    }

#endif

    }

    private void Start()
    {
        //   musicManager.PlayBGMusic();

    }

    public void Init()
    {
        Application.targetFrameRate = 60;
        SetUp();
    }

    public void SetUp()
    {
       
        musicManager.Init();
        iapController.Init();
        MMVibrationManager.SetHapticsActive(useProfile.OnVibration);
        admobAds.Init();
        startLoading.Init();
        heartGame.Init();
 
    }

    public void LoadScene(string sceneName)
    {
        Initiate.Fade(sceneName.ToString(), Color.black, 2f);
    }

    /// <summary>
    /// Từ sau level 3, cứ mỗi 2 level (4, 6, 8...) show Interstitial trước khi vào game.
    /// Gọi khi chuẩn bị Fade sang GAME_PLAY; nếu đúng level thì show inter rồi chạy onDone, không thì chạy onDone luôn.
    /// </summary>
    public void TryShowInterBeforeNextLevel(UnityAction onDone)
    {
        if (admobAds == null || useProfile.IsRemoveAds)
        {
            onDone?.Invoke();
            return;
        }
      
            admobAds.ShowInterstitialAd(  delegate { onDone?.Invoke(); });
      
    }

}
public enum SceneType
{
    StartLoading = 0,
    MainHome = 1,
    GamePlay = 2
}