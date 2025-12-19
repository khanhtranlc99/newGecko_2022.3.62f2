using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class BoosterHandKeep : MonoBehaviour
{
    public Button HandKeep_Btn;
    public Text tvNum;
    public GameObject objNum;
    public GameObject parentTvCoin;
    public GameObject lockIcon; 
    public bool wasUseTNT_Booster;

    public GameObject hand;

    public GameObject handPrefab;
    public Transform handPos;

    public GameObject currentWorm;


    
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
            HandKeep_Btn.onClick.AddListener(HandleAtom_Booster);
            if (UseProfile.TNT_Booster > 0)
            {
                objNum.SetActive(true);
                tvNum.text = UseProfile.TNT_Booster.ToString();
                parentTvCoin.SetActive(false);
            }
            else
            {
                objNum.SetActive(false);
                tvNum.gameObject.SetActive(false);
                parentTvCoin.SetActive(true);
            }
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_TNT_BOOSTER, ChangeText);
        }
        void HandleLock()
        {


            HandKeep_Btn.onClick.AddListener(HandleLockBtn);
        }
    }

    public void HandleLockBtn()
    {
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                              (
                              HandKeep_Btn.transform.position,
                              "Unlock at level 9",
                              Color.white,
                              isSpawnItemPlayer: true
                              );
    }





    public void HandleAtom_Booster()
    {
     //   GameController.Instance.musicManager.PlayClickSound();
        if (UseProfile.TNT_Booster >= 1)
        {

       
            UseProfile.TNT_Booster -= 1;
             hand = Instantiate(handPrefab, handPos);
             hand.SetActive(true);
             wasUseTNT_Booster = true;
         
        }
        else
        {
            SuggetBox.Setup(GiftType.TNT_Booster).Show();
        }

    }


    

    public void ChangeText(object param)
    {
        tvNum.text = UseProfile.TNT_Booster.ToString();
        if (UseProfile.TNT_Booster > 0)
        {
            objNum.SetActive(true);
            tvNum.gameObject.SetActive(true);
            tvNum.text = UseProfile.TNT_Booster.ToString();
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
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_TNT_BOOSTER, ChangeText);
    }




    private void Update()
    {
        if(wasUseTNT_Booster)
        {

            if(Input.GetMouseButtonDown(0))
            {
            // Bắn raycast từ camera đến vị trí chuột trên màn hình
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Kiểm tra raycast có hit collider không
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Nếu có collider được hit
                Collider hitCollider = hit.collider;
                if (hitCollider != null)
                {
                  if(hitCollider.gameObject.GetComponent<WormController>() != null)
                  {
                       if(currentWorm == null)
                       {
                        currentWorm = hitCollider.gameObject;
                             hand.transform.DOMove(hit.point, 0.1f).OnComplete(() => {
                        
                               hitCollider.gameObject.transform.parent = hand.transform;
                               hand.transform.DOMove(handPos.position, 0.1f).OnComplete(() => {
                                wasUseTNT_Booster = false;
                                hand.SetActive(false);
                                Destroy(currentWorm.gameObject);
                                  GamePlayController.Instance.playerContain.levelController.levelData.lsWormsInGame.Remove(currentWorm.gameObject);
                                  GamePlayController.Instance.gameScene.HandleCheckWin();
                                currentWorm = null;
                               });
                        });
                       }
                      



                   
                  }
                }
            }
            }
        }
    }







}
