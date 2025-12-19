using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class HandHint : MonoBehaviour
{
    [SerializeField] Image img;
    public Transform post1;
    public Transform post2;
    
    public void Init(Transform paramPost_1, Transform paramPost_2    )
    {
            post1 = paramPost_1;
            post2 = paramPost_2;
            this.transform.position = paramPost_1.position;
            img.color = new Color32(255, 255, 255, 0);
            img.DOFade(1, 0.5f).OnComplete(delegate {

                this.transform.DOMove(paramPost_2.position, 1).SetEase(Ease.OutBack).OnComplete(delegate
                {
                    img.DOFade(0, 0.3f);
                    Init(post1, post2);
                });

            });
        
      


    }

    private void OnDestroy()
    {
        this.transform.DOKill();
        img.DOKill();
    }
    private void OnDisable()
    {
        this.transform.DOKill();
        img.DOKill();

    }
}
