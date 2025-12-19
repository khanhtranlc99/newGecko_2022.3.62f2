using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackInShopCoin : PackInShop
{

    public int coinGift;
    public GiftType currentGift;
    public int numGift = 1;
    public ButtonShopController buttonShopController;
    public override void Init()
    {
        btnBuy.onClick.AddListener(delegate { HandleBuy(); });
    }
    private void HandleBuy()
    {
        GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.Coin >= coinGift)
        {
            UseProfile.Coin -= coinGift;
            List<GiftRewardShow> giftRewardShows = new List<GiftRewardShow>();
            giftRewardShows.Add(new GiftRewardShow() { amount = numGift, type = currentGift });
            foreach (var item in giftRewardShows)
            {
                GameController.Instance.dataContain.giftDatabase.Claim(item.type, item.amount);
            }
            PopupRewardBase.Setup(false).Show(giftRewardShows, delegate {   });
        }
        else
        {
            buttonShopController.HandleOnClick(ButtonShopType.Gold);
            btnBuy.transform.SetAsLastSibling();
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                       (
                          btnBuy.transform
                          ,
                       btnBuy.transform.position,
                       "More Coin",
                       Color.white,
                       isSpawnItemPlayer: true
                       );
        }    
    }    

}
