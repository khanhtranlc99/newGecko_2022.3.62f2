using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading;

public class AdmobAdsGoogle : MonoBehaviour
{
    private const string AppID = "ca-app-pub-1848319716441980~1498427537";
    private const string InterstitialAdUnitId = "ca-app-pub-1848319716441980/7609733250";
    private const string RewardedAdUnitId = "ca-app-pub-1848319716441980/6359542408";
    private const string BanerAdUnitId = "ca-app-pub-1848319716441980/9548307943";
    private const string AppOpenId = "ca-app-pub-1848319716441980/9356736256";
 
 
    BannerView bannerView;
    InterstitialAd _interstitialAd;
 
    public bool bannerAvailable { get; private set; } = false;
    public float countdownAds;
    public bool IsLoadedInterstitial()
    {
        if (_interstitialAd == null)
        {
            return false;
        }
        return _interstitialAd.CanShowAd();
    }
    public bool IsLoadedAOA()
    {
        if(appOpenAd == null )
        {
            return false;
        }    
        return appOpenAd.CanShowAd();
    }
    public void Init()
    {
        countdownAds = 10000;
        MobileAds.RaiseAdEventsOnUnityMainThread = false;
        MobileAds.Initialize(initStatus =>
        {

          
                InitializeBannerAds();
 
                InitInterstitial();
                InitializeRewardedAds();
         
        });
       
    }
     
 

    public void HandleLoadAdsInGame()
    {
        InitializeBannerAds();
        InitializeOpenAppAds();
        InitInterstitial();
        InitializeRewardedAds();
    }    
  

    private void  Update()
    {
        countdownAds += Time.unscaledDeltaTime;
    }

    #region Banner
    bool noCollapsible = false;
    public void InitializeBannerAds()
    {
        DestroyBannerView();
        bannerView = new BannerView(BanerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        ListenToBannerEvent(bannerView);
        AdRequest request = new AdRequest();

        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
         
    }
  
    private void RemoveListenBannerEvent(BannerView _banner)
    {
        if (_banner == null) return;
        // Raised when an ad is loaded into the banner view.
        _banner.OnBannerAdLoaded -= () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                      + _banner.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        _banner.OnBannerAdLoadFailed -= (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                           + error);
        };
        // Raised when the ad is estimated to have earned money.
        _banner.OnAdPaid -= (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _banner.OnAdImpressionRecorded -= () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _banner.OnAdClicked -= () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _banner.OnAdFullScreenContentOpened -= () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _banner.OnAdFullScreenContentClosed -= () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    private void ListenToBannerEvent(BannerView _banner)
    {
        if (_banner == null) return;
        // Raised when an ad is loaded into the banner view.
        _banner.OnBannerAdLoaded += () =>
        {
            bannerAvailable = true;
            ShowBanner();
            Debug.LogError("BannerNative_____ok");
          
        };
        // Raised when an ad fails to load into the banner view.
        _banner.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("BannerNativeFalse" + error);
            StartCoroutine(HandleOnBannerAdLoadFailed());
        };
        // Raised when the ad is estimated to have earned money.
        _banner.OnAdPaid += (AdValue adValue) =>
        {
         
            OnAdRevenuePaidEvent(BanerAdUnitId, "COLLAPSIBLE_BANNER", _banner.GetResponseInfo(), adValue);
        };
        // Raised when an impression is recorded for an ad.
        _banner.OnAdImpressionRecorded += () =>
        {
           
        };
        // Raised when a click is recorded for an ad.
        _banner.OnAdClicked += () =>
        {
          
        };
        // Raised when an ad opened full screen content.
        _banner.OnAdFullScreenContentOpened += () =>
        {
        
        };
        // Raised when the ad closed full screen content.
        _banner.OnAdFullScreenContentClosed += () =>
        {
       
        };
    }
    private IEnumerator HandleOnBannerAdLoadFailed()
    {
        yield return new WaitForEndOfFrame();

        bannerAvailable = false;

        Invoke(nameof(InitializeBannerAds), 3);
    }

    public void ShowBanner()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            return;
        }

        if(bannerView != null)
        {
  
            bannerView.Show();

        }    
      
    }
    private void DestroyBannerView()
    {
        if (bannerView != null)
        {
            Debug.Log($"Destroy Admob banner");
            bannerAvailable = false;
            RemoveListenBannerEvent(bannerView);
            bannerView.Hide();
            bannerView.Destroy();
            bannerView = null;
        }
    }

    public void HideBanner()
    {
     
        if (bannerView != null)
        {
            bannerView.Hide();
        }

    }

    #endregion

    #region Interstitial
    Action evtInterDone;
    bool showingInter = false;
    private void InitInterstitial()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        showingInter = false;


        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(InterstitialAdUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                ListenToInterstitialEvent(_interstitialAd);
          
            });

    }
    private void ListenToInterstitialEvent(InterstitialAd interstitialAd)
    {
   
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
        
            OnAdRevenuePaidEvent(InterstitialAdUnitId, "ADMOB_INTER", interstitialAd.GetResponseInfo(), adValue);
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
          
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
     
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
         
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            
        };
  
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
       
            StartCoroutine(HandleActionInter());
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            StartCoroutine(OnAdFullInterScreenContentFailed());
            // Reload the ad so that we can show another as soon as possible.

        };
    }
    public void ShowInterstitialAd(Action actionIniterClose)
    {
        Debug.LogError("ShowInterstitialAd");
        evtInterDone = actionIniterClose;
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            evtInterDone?.Invoke();
            Debug.LogError("ShowInterstitialAd000000000000000");
            return;
        }
        Debug.LogError("ShowInterstitialAd11111111111111");

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.LogError("LEVEL_START_SHOW_INITSTIALL" + RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_INITSTIALL, 1));
            if (UseProfile.CurrentLevel >= RemoteConfigController.GetFloatConfig(FirebaseConfig.LEVEL_START_SHOW_INITSTIALL, 1))
            {
             
                if (countdownAds > RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL, 90))
                {
                    showingInter = true;
                    _interstitialAd.Show();
                }
                else
                {
                    evtInterDone?.Invoke();
                }     
            }
            else
            {
                evtInterDone?.Invoke();
            }
        }
        else
        {
            evtInterDone?.Invoke();
        }
    }

    private IEnumerator HandleActionInter()
    {
        yield return new WaitForEndOfFrame();
        evtInterDone?.Invoke();
        showingInter = false;
        Invoke(nameof(InitInterstitial), 3);
    }
    private IEnumerator OnAdFullInterScreenContentFailed()
    {
        yield return new WaitForEndOfFrame();
        Invoke(nameof(InitInterstitial), 3);
    }

    #endregion

    #region RewardedAd
    private RewardedAd _rewardedAd;
    Action evtRewardedClose;
    bool showingReward = false;
    public void InitializeRewardedAds()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
        showingReward = false;
        Debug.Log("Loading the rewarded ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(RewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("Rewarded ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : "
                          + ad.GetResponseInfo());

                _rewardedAd = ad;
                RegisterRewardedEventHandlers(_rewardedAd);
            });
    }
    private void RegisterRewardedEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
            OnAdRevenuePaidEvent(RewardedAdUnitId, "ADMOB_REWARDED", ad.GetResponseInfo(), adValue);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            StartCoroutine(HandleClaimReward());
         
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {

            StartCoroutine(OnAdFullScreenContentFailed());
        };
    }

    private IEnumerator HandleClaimReward()
    {
        yield return new WaitForEndOfFrame();
        evtRewardedClose?.Invoke();
        Invoke(nameof(InitializeRewardedAds), 10);
        showingReward = false;
    }   
    private IEnumerator OnAdFullScreenContentFailed()
    {
        yield return new WaitForEndOfFrame();
        Invoke(nameof(InitializeRewardedAds), 10);
    }

    public void ShowRewardedAd(Action actionReward, UnityAction actionNotLoadedVideo, ActionWatchVideo actionType)
    {
        evtRewardedClose = actionReward;
  

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            showingReward = true;
            _rewardedAd.Show((Reward reward) =>
            {

          
            });
        }
        else
        {
            ShowInterstitialAd(actionReward);
        }
    }
    #endregion

    #region AppOpenAd

    public bool noAdsToShowAOA
    {
        get 
        {
            if(showingInter || showingReward)
            {
                return false;
            }
            if (showingInter && showingReward)
            {
                return false;
            }
            return true;
        }
    }

    private AppOpenAd appOpenAd = null;
    Action evtAppOpenAdDone;
    public void InitializeOpenAppAds()
    {
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(AppOpenId, adRequest, (AppOpenAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                return;
            }
            else
            {
                appOpenAd = ad;
                RegisterAOAEventHandlers(appOpenAd);
       
            }
        });

    }
    private void RegisterAOAEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
       
            OnAdRevenuePaidEvent(AppOpenId, "ADMOB_AOA", ad.GetResponseInfo(), adValue);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
      
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
   
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
          
        
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            StartCoroutine(HandleOnAdFullScreenContentClosed());
         
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            StartCoroutine(HandleOnAdAOAFullScreenContentFailed());

        };
    }
    public void ShowOpenAppAdsReady(Action actionIniterClose)
    {
        evtAppOpenAdDone = actionIniterClose;
     

        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            return;
        }

 
                if (appOpenAd != null && appOpenAd.CanShowAd() && noAdsToShowAOA)
                {
                    appOpenAd.Show();
                    Debug.LogError("SHOW_OPEN_ADS");
                }
 


    }
    private IEnumerator HandleOnAdFullScreenContentClosed()
    {
        yield return new WaitForEndOfFrame();
        evtAppOpenAdDone?.Invoke();
    }

    private IEnumerator HandleOnAdAOAFullScreenContentFailed()
    {
        yield return new WaitForEndOfFrame();
        Invoke(nameof(InitializeOpenAppAds), 3);
    }
    #endregion





    private void OnAdRevenuePaidEvent(string adUnitId, string type, ResponseInfo info, AdValue value)
    {
       
    }
}
