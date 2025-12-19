using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class WinStreakBar : MonoBehaviour
{
    public Image image;
    public Transform postWinStreak;
   public void Init()
    {
        image.DOColor(new Color32(255, 255, 255, 255), 0.5f).OnComplete(delegate {

            image.DOColor(new Color32(80, 80, 80, 80), 0.5f).OnComplete(delegate {


                Init();
            });

        });
    }
    public void Scale(Action callBack)
    {
    
        image.DOColor(new Color32(255, 255, 255, 255), 0.3f).OnComplete(delegate {

            image.DOColor(new Color32(80, 80, 80, 80), 0.3f).OnComplete(delegate {

                image.DOColor(new Color32(255, 255, 255, 255), 0.3f).OnComplete(delegate {

                    image.DOColor(new Color32(80, 80, 80, 80), 0.3f).OnComplete(delegate {

                        image.DOColor(new Color32(255, 255, 255, 255), 0.5f).OnComplete(delegate {


                            callBack?.Invoke();
                        });

                    });

                });
            });

        });
    }    

    private void OnDestroy()
    {
        image.DOKill();
    }
}
