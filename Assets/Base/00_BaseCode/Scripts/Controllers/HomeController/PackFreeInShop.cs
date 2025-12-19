using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackFreeInShop : PackInShop
{
    public GiftType currentGift;
    public int numGift = 1;
    public GameObject panelText;
    public bool WasClaim
    {
        get
        {

            return PlayerPrefs.GetInt("WasClaim" + currentGift.ToString(), 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("WasClaim" + currentGift.ToString(), value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public override void Init()
    {
        btnBuy.onClick.AddListener(delegate { HandleBuy(); });
        if(!WasClaim)
        {
            panelText.SetActive(true);
        }    
        else
        {
            panelText.SetActive(false);
        }    
    }
    private void HandleBuy()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (!WasClaim )
        {
            WasClaim = true;
            List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
            giftRewardShows.Add(new GiftRewardShow() { amount = numGift, type = currentGift });
            foreach (var item in giftRewardShows)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(item.type, item.amount);
            }
            PopupRewardBase.Setup(false).Show(giftRewardShows, delegate { });
            panelText.SetActive(false);
        }
        else
        {
             
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                       (
                          btnBuy.transform
                          ,
                       btnBuy.transform.position,
                       "Wait More Time",
                       Color.white,
                       isSpawnItemPlayer: true
                       );
        }
    }

    public void HandleOn()
    {
        WasClaim = false;

        panelText.SetActive(true);
     
        
    }    

}
