using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ScaleInOut : MonoBehaviour
{
    public Vector3 scaleIn;
    public Vector3 scaleOut;
    public float speed;
    void OnEnable()
    {
        HandleScaleInOut();
    }
    private void HandleScaleInOut()
    {
        this.transform.DOScale(scaleIn, speed).OnComplete(delegate {

            this.transform.DOScale(scaleOut, speed).OnComplete(delegate {
                HandleScaleInOut();
            });
        });
    }
    private void OnDestroy()
    {
        this.transform.DOKill();
    }
    private void OnDisable()
    {
        this.transform.DOKill();
    }
  
  
}
