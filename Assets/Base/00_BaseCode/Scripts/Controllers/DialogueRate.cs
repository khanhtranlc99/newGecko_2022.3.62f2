using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class DialogueRate : BaseBox
{

    private static DialogueRate instance;
    public static DialogueRate Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<DialogueRate>(PathPrefabs.RATE_GAME_BOX));
            instance.Init();
        }
        //ChickenDataManager.CountTillShowRate = 0;
        instance.gameObject.SetActive(true);
        return instance;
    }


  //  [SerializeField] MyReviewManager myReviewManager;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private List<Sprite> lstSprStar;
    [SerializeField] private List<Button> lstBtnStar;
    [SerializeField] private List<Image> lstImgStar;
    private int countStar;

    public void Init()
    {
        btnConfirm.onClick.AddListener(RateAction);
        btnClose.onClick.AddListener(CloseAction);


        btnConfirm.interactable = false;

    }
    public void InitState()
    {
        for (int i = 0; i < lstBtnStar.Count; i++)
        {
            int index = i + 1;
         
            lstImgStar[i].sprite = lstSprStar[0];
        }
        countStar = 0;
    
    }
    public void ClickStar(int index)
    {
        countStar = index;
        for (int i = 0; i < lstImgStar.Count; i++)
        {
            lstImgStar[i].sprite = lstSprStar[0];
        }
        for (int i = 0; i < index; i++)
        {
            lstImgStar[i].sprite = lstSprStar[1];
        }
        btnConfirm.interactable = true;
        //GameController.Instance.musicManager.Pla();
    }
    public void RateAction()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (countStar <= 0)
            return;
        if (countStar == 5)
        {
            UseProfile.CanShowRate = false;

            //myReviewManager.ShowReview(delegate { CloseAction(); });

        
        }
        else
        {
            ShowTextThankRate();
            CloseAction();
        }
    }
    public void CloseAction()
    {
        GameController.Instance.musicManager.PlayClickSound();

        UseProfile.CurrentLevel += 1;
        if (UseProfile.CurrentLevel >= 84)
        {
            UseProfile.CurrentLevel = 84;
        }
        UseProfile.WinStreak += 1;
     
        Initiate.Fade("GamePlay", Color.black, 2f);

        Close();


    }

    public void ShowTextThankRate()
    {
        //StartCoroutine(Helper.StartAction(() =>
        //{
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
    (
       btnConfirm.transform.position,
      "THANK FOR REVIEW",
        Color.white,
        isSpawnItemPlayer: true
    );
        // }, 0.5f));
    }

    
}