using MoreMountains.NiceVibrations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HomeScene : BaseScene
{

    public Button btnSetting;
    public Button btnContinue;
    public TMP_Text currentLevel;
    public Image imageStatus;
    public List<Sprite> status;
 
 
    public void ShowGift()
    {
        

    }
    public int NumberPage(ButtonType buttonType)
    {
        switch (buttonType)
        {
            case ButtonType.ShopButton:
                return 0;
                break;

            case ButtonType.HomeButton:
                return 1;
                break;

            case ButtonType.RankButton:
                return 2;
                break;

        }
        return 0;
    }


    public void Init()
    {
        currentLevel.text = "Level " + UseProfile.CurrentLevel.ToString();
        if (UseProfile.CurrentLevel %5==0)
        {
            imageStatus.sprite = status[0];
        }
        else
        {
            imageStatus.sprite = status[1];
        }

        btnContinue.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f); });

        btnSetting.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); OnSettingClick(); });

  

        
       
   
    }
    //private void Update()
    //{

    //       // OnScreenChange();


    //}





    public override void OnEscapeWhenStackBoxEmpty()
    {
        //Hiển thị popup bạn có muốn thoát game ko?
    }
    private void OnSettingClick()
    {
        SettingBox.Setup(true).Show();
        //MMVibrationManager.Haptic(HapticTypes.MediumImpact);
    }

    


}
