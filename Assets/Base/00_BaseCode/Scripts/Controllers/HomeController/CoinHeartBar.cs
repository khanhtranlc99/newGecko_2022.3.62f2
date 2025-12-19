using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CoinHeartBar : MonoBehaviour
{
    public Text tvCoin;
    public Text tvHeart;
    public Button btnHeart;
    public Button btnCoin;
    public Text tvCoolDownHeart;
    public GameObject panelTime;
    public Image iconHeart;
    public Sprite normalHeart;
    public Sprite unlimitHeart;

    public void Init ()
    {
        tvCoin.text = UseProfile.Coin.ToString();
        tvHeart.text = UseProfile.Heart.ToString();
        btnCoin.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound(); ShopBox.Setup(ButtonShopType.Gold).Show(); });
        btnHeart.onClick.AddListener(delegate { GameController.Instance.musicManager.PlayClickSound();
            Debug.LogError("HeartBox");

            HeartBox.Setup().Show();
        });

        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_COIN, HandleChangeCoin);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_HEART, HandleChangeHeart);
    }

    public void HandleChangeCoin(object param)
    {
        tvCoin.text = UseProfile.Coin.ToString();
    }
    public void HandleChangeHeart(object param)
    {
        tvHeart.text = UseProfile.Heart.ToString();
    }

    public void OnDisable()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_COIN, HandleChangeCoin);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_HEART, HandleChangeHeart);

    }
    public void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_COIN, HandleChangeCoin);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_HEART, HandleChangeHeart);
    }
    private void Update()
    {
        if(!UseProfile.isUnlimitHeart)
        {
            if (UseProfile.Heart < 5)
            {
                tvCoolDownHeart.text = TimeManager.ShowTime2((long)GameController.Instance.heartGame.currentCoolDown);
                if (panelTime != null)
                {
                    panelTime.gameObject.SetActive(true);
                }
            }
            else
            {
                tvCoolDownHeart.text = "";
                tvHeart.text = "FULL";
                if (panelTime != null)
                {
                    panelTime.gameObject.SetActive(false);
                }
            }
            iconHeart.sprite = normalHeart;
        }
        else
        {

            tvHeart.text = "FULL";
            iconHeart.sprite = unlimitHeart;
            tvCoolDownHeart.text = TimeManager.ShowTime2((long)GameController.Instance.heartGame.timeLimit);
            if (panelTime != null)
            {
                panelTime.gameObject.SetActive(true);
            }
           
        }
    
      
    }

}
