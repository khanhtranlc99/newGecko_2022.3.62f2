using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class PlayerContain : MonoBehaviour
{
    public LevelController levelController;
    public BoosterHandKeep boosterHandKeep;
    public BoosterPlusMoves boosterPlusMoves;
    public Material wrongMaterial;

    [Header("Btn UI")]
    public Button btnRetry;
    public Button btnSetting;
    public bool isPopupUp = false;

    public bool isRotateContainer = false;
    public void Init()
    {
        levelController.Init();
        boosterHandKeep.Init(this);
        boosterPlusMoves.Init(this);
        btnRetry.onClick.AddListener(delegate { HandleRetry(); });
        btnSetting.onClick.AddListener(delegate { HandleSetting(); });
    }

    void HandleRetry()
    {
        GameController.Instance.musicManager.PlayClickSound();
        Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f);
    }
    void HandleSetting()
    {
        isPopupUp = true;
        GameController.Instance.musicManager.PlayClickSound();
        SettingBox.Setup(true).Show();
    }
}
