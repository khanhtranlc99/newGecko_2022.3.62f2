using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ButtonShopController : MonoBehaviour
{
    public List<ButtonShop> lsButtonShops;
    public RectTransform rectTransform;
    private ButtonShop btnShopTemp;
    ButtonShopType currentButtonShopType = ButtonShopType.None;
    public ScrollRect scroller;
    public bool move;
    public ButtonShop GetButtonShop(ButtonShopType param)
    {
       foreach(var item in lsButtonShops)
        {
            if(item.buttonShopType == param)
            {
                return item;
            }
        }
        return null;
    }

    public void Init (ButtonShopType param)
    {
        foreach(var item in lsButtonShops)
        {
            item.Init();
        }
        scroller.onValueChanged.AddListener(OnScrollValueChanged);
        HandleOnClick(param);


    }
    public void HandleOnClick(ButtonShopType buttonShopType  )
    {
   
        if(currentButtonShopType != buttonShopType)
        {
            move = false;
            currentButtonShopType = buttonShopType;
            foreach (var item in lsButtonShops)
            {
                item.HandleOff();
            }
            btnShopTemp = GetButtonShop(buttonShopType);
            btnShopTemp.HandleOn();
            btnShopTemp.transform.SetAsLastSibling();
            switch (buttonShopType)
            {
                case ButtonShopType.Gift:
                
                        rectTransform.DOAnchorPosY(0, 0.5f).OnComplete(delegate { move = true; });
               
                    break;
                case ButtonShopType.Booster:
                
                        rectTransform.DOAnchorPosY(960, 0.5f).OnComplete(delegate { move = true; });

                    break;
                case ButtonShopType.Gold:
               
                        rectTransform.DOAnchorPosY(1472, 0.5f).OnComplete(delegate { move = true; });

                    break;
            }
        
        }
    

 
        
    }

    private void OnScrollValueChanged(Vector2 position)
    {
        if(move == true)
        {
            if (position.y <= 1 && position.y > 0.6f)
            {

                if (currentButtonShopType != ButtonShopType.Gift)
                {
                    currentButtonShopType = ButtonShopType.Gift;
                    foreach (var item in lsButtonShops)
                    {
                        item.HandleOff();
                    }
                    btnShopTemp = GetButtonShop(ButtonShopType.Gift);
                    btnShopTemp.HandleOn();
                    btnShopTemp.transform.SetAsLastSibling();
                }
            }
            if (position.y <= 0.6f && position.y > 0.1f)
            {
                if (currentButtonShopType != ButtonShopType.Booster)
                {
                    currentButtonShopType = ButtonShopType.Booster;
                    foreach (var item in lsButtonShops)
                    {
                        item.HandleOff();
                    }
                    btnShopTemp = GetButtonShop(ButtonShopType.Booster);
                    btnShopTemp.HandleOn();
                    btnShopTemp.transform.SetAsLastSibling();
                }
            }
            if (position.y <= 0.1f)
            {
                if (currentButtonShopType != ButtonShopType.Gold)
                {
                    currentButtonShopType = ButtonShopType.Gold;
                    foreach (var item in lsButtonShops)
                    {
                        item.HandleOff();
                    }
                    btnShopTemp = GetButtonShop(ButtonShopType.Gold);
                    btnShopTemp.HandleOn();
                    btnShopTemp.transform.SetAsLastSibling();
                }
            }
            
        }    
 
    }    
}
