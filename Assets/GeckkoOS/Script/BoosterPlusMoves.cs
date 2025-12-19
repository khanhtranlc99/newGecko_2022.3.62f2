using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
public class BoosterPlusMoves : MonoBehaviour
{
  public Button PlusMoves_Btn;
    public Text tvNum;
    public GameObject objNum;
    public GameObject parentTvCoin;
    public GameObject lockIcon; 
    public bool wasUseTNT_Booster;

    public GameObject vfxMoveBooster;
    public Transform canvas;
    public Transform target;



    
    public void Init(PlayerContain param)
    {
        
        wasUseTNT_Booster = false;
        if (UseProfile.CurrentLevel >= 0)//9
        {

            //unLockIcon.gameObject.SetActive(true);
            lockIcon.gameObject.SetActive(false);
            HandleUnlock();


        }
        else
        {
            //unLockIcon.gameObject.SetActive(false);
            lockIcon.gameObject.SetActive(true);
            objNum.SetActive(false);
            HandleLock();

        }


        void HandleUnlock()
        {
            PlusMoves_Btn.onClick.AddListener(HandleAtom_Booster);
            if (UseProfile.Roket_Booster > 0)
            {
                objNum.SetActive(true);
                tvNum.text = UseProfile.Roket_Booster.ToString();
                parentTvCoin.SetActive(false);
            }
            else
            {
                objNum.SetActive(false);
                tvNum.gameObject.SetActive(false);
                parentTvCoin.SetActive(true);
            }
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_ROCKET_BOOSTER, ChangeText);
        }
        void HandleLock()
        {


            PlusMoves_Btn.onClick.AddListener(HandleLockBtn);
        }
    }

    public void HandleLockBtn()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                              (
                              PlusMoves_Btn.transform.position,
                              "Unlock at level 9",
                              Color.white,
                              isSpawnItemPlayer: true
                              );
    }





    public void HandleAtom_Booster()
    {
        //GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.Roket_Booster >= 1)
        {

       
            UseProfile.Roket_Booster -= 1;
             var temp = SimplePool2.Spawn(vfxMoveBooster);
             temp.transform.SetParent(canvas);
             temp.transform.position = PlusMoves_Btn.transform.position;
             temp.transform.localScale = Vector3.zero;
             temp.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack).OnComplete(() => {
       
         
             temp.transform.DOMove(target.position, 1f).OnComplete(() => {
                GamePlayController.Instance.gameScene.PlusNumMove(3);
                SimplePool2.Despawn(temp.gameObject);
             });
             });
         
        }
        else
        {
            SuggetBox.Setup(GiftType.TNT_Booster).Show();
        }

    }


    

    public void ChangeText(object param)
    {
        tvNum.text = UseProfile.Roket_Booster.ToString();
        if (UseProfile.Roket_Booster > 0)
        {
            objNum.SetActive(true);
            tvNum.gameObject.SetActive(true);
            tvNum.text = UseProfile.Roket_Booster.ToString();
            parentTvCoin.SetActive(false);
        }
        else
        {
            objNum.SetActive(false);
            tvNum.gameObject.SetActive(false);
            parentTvCoin.SetActive(true);
        }
    }
    public void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_ROCKET_BOOSTER, ChangeText);
    }
}
