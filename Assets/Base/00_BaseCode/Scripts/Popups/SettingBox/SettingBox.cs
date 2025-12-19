using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.SocialPlatforms.Impl;
public class SettingBox : BaseBox
{
    #region instance
    public static SettingBox instance;
    public static SettingBox Setup(bool isOffButton = true, bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<SettingBox>(PathPrefabs.SETTING_BOX));
            instance.Init();
        }

        instance.InitState(isOffButton);
        return instance;
    }
    #endregion
    #region Var

    [SerializeField] private Button btnClose;
  

    [SerializeField] private Button btnVibration;
    [SerializeField] private Button btnMusic;
    [SerializeField] private Button btnSound;
    [SerializeField] private Button btnHome;

    public RectTransform objMusic;
    public RectTransform objVibra;
    public RectTransform objSound;
    public Image imgMusic;
    public Image imgVibration;
    public Image imgSound;

    public Sprite spriteOn;
    public Sprite spriteOff;

 
  
   // public Button btnRestart;

    public bool isGameplay;

    public Vector3 postOn = new Vector3(90, -36, 0);
    public Vector3 postOff = new Vector3(35, -36, 0);

    #endregion
    private void Init()
    {
        btnClose.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); OnClickButtonClose(); }); 
        btnVibration.onClick.AddListener(delegate { OnClickBtnVibration(); });
        btnMusic.onClick.AddListener(delegate { OnClickBtnMusic(); });
        btnSound.onClick.AddListener(delegate { OnClickBtnSound(); });
        
        if(SceneName.GAME_PLAY == SceneManager.GetActiveScene().name && UseProfile.CurrentLevel>1)
        {
            btnHome.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); HandleBtnHome(); });
        }
        else
        {
            btnHome.gameObject.SetActive(false);
        }


        //  btnRestart.onClick.AddListener(delegate { HandleBtnRestart(); });


    }
  
    private void InitState(bool param)
    {
        isGameplay = param;
        
    
        SetUpBtn();
     //   GameController.Instance.admobAds.HandleShowMerec();
       
    }

    
    private void SetUpBtn()
    {
        if (GameController.Instance.useProfile.OnVibration)
        {
            imgVibration.sprite = spriteOn;
            objVibra.anchoredPosition = postOn;
        }
        else
        {
            imgVibration.sprite = spriteOff;
            objVibra.anchoredPosition = postOff;
        }

        if (GameController.Instance.useProfile.OnMusic)
        {
            imgMusic.sprite = spriteOn;
            objMusic.anchoredPosition = postOn;
        }
        else
        {
            imgMusic.sprite = spriteOff;
            objMusic.anchoredPosition = postOff;
        }

        if (GameController.Instance.useProfile.OnSound)
        {
            objSound.anchoredPosition = postOn;
            imgSound.sprite = spriteOn;
        }
        else
        {
            objSound.anchoredPosition = postOff;
            imgSound.sprite = spriteOff;
        }
      
    }

  
    private void OnClickBtnVibration()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (GameController.Instance.useProfile.OnVibration)
        {
            GameController.Instance.useProfile.OnVibration = false;
        }
        else
        {
            GameController.Instance.useProfile.OnVibration = true;
        }
        SetUpBtn();
    }

    private void OnClickBtnMusic()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (GameController.Instance.useProfile.OnMusic)
        {
            GameController.Instance.useProfile.OnMusic = false;
        }
        else
        {
            GameController.Instance.useProfile.OnMusic = true;
        }
        SetUpBtn();
    }
    private void OnClickBtnSound()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (GameController.Instance.useProfile.OnSound)
        {
            GameController.Instance.useProfile.OnSound = false;
        }
        else
        {
            GameController.Instance.useProfile.OnSound = true;
        }
        SetUpBtn();
    }


    private void OnClickButtonClose()
    {
        if(SceneName.GAME_PLAY == SceneManager.GetActiveScene().name)
            GamePlayController.Instance.playerContain.isPopupUp = false;
        GameController.Instance.admobAds.ShowInterstitial(false, actionIniterClose: () => { Next(); }, actionWatchLog: "BtnBackHomeSettingBox");

        void Next()
        {

            Close();


        }

    }


    public void HandleBtnHome()
    {

        GameController.Instance.admobAds.ShowInterstitial(false, actionIniterClose: () => { Next(); }, actionWatchLog: "BtnBackHomeSettingBox");

        void Next()
        {
            Initiate.Fade(SceneName.HOME_SCENE, Color.black, 2f);
            GameController.Instance.musicManager.PlayClickSound();
            Close();
        }
      



    }
 
    private void OnClickRestorePurchase()
    {
        GameController.Instance.iapController.RestorePurchases();
    }

    public override void Close()
    {
        base.Close();

     //   GameController.Instance.admobAds.HandleHideMerec();
    }

}
