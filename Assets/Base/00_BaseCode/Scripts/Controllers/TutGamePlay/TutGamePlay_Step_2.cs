using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutGamePlay_Step_2 : TutorialBase
{
    GameObject currentHand;
    [SerializeField] private float infinityRadius = 1f;
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private Vector3 centerPosition;
    
    private Sequence infinitySequence;
    private Coroutine spawnHandCoroutine;
    private bool isDragged = false;

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel == 1 && base.IsCanShowTut()) return true;
        else return false;
    }
    public override bool IsCanEndTut()
    {
        if (!isDragged)
            return false;
            
        if (infinitySequence != null)
        {
            infinitySequence.Kill();
            infinitySequence = null;
        }
        
        if (spawnHandCoroutine != null)
        {
            StopCoroutine(spawnHandCoroutine);
            spawnHandCoroutine = null;
        }
        
        if (currentHand != null)
        {
            currentHand.transform.DOKill();
            Destroy(currentHand.gameObject);
        }
        
        return true;
    }

    public override void StartTut()
    {
        centerPosition = transform.position;
        
        if (GamePlayController.Instance != null && 
            GamePlayController.Instance.playerContain != null &&
            GamePlayController.Instance.playerContain.isRotateContainer)
        {
            OnPlayerDragged();
            return;
        }
        
        spawnHandCoroutine = StartCoroutine(SpawnHand());
    }
    
    IEnumerator SpawnHand()
    {
        if (isDragged) yield break;
        
        yield return new WaitForSeconds(0.5f);
        
        if (isDragged) yield break;
        
        if (handTut == null)
        {
            Debug.LogWarning("[TutGamePlay_Step_2] handTut prefab chưa được gán!");
            yield break;
        }
        
        Vector3 startPos = new Vector3(centerPosition.x, centerPosition.y, centerPosition.z);
        currentHand = Instantiate(handTut, startPos, handTut.transform.rotation);
        currentHand.SetActive(true);
        
        StartInfinityAnimation();
    }
    
    private void StartInfinityAnimation()
    {
        if (currentHand == null) return;
        
        if (infinitySequence != null)
        {
            infinitySequence.Kill();
        }
        
        infinitySequence = DOTween.Sequence();
        
        int segments = 50;
        Vector3[] path = new Vector3[segments + 1];
        
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments * Mathf.PI * 2f;
            float x = centerPosition.x + infinityRadius * Mathf.Sin(t);
            float y = centerPosition.y + infinityRadius * Mathf.Sin(2f * t) / 2f;
            float z = centerPosition.z;
            
            path[i] = new Vector3(x, y, z);
        }
        
        infinitySequence.Append(currentHand.transform.DOPath(path, animationDuration, PathType.CatmullRom)
            .SetEase(Ease.Linear));
        
        infinitySequence.SetLoops(-1);
    }

    protected override void OnUpdate()
    {
        if (isDragged) return;
        
        if (GamePlayController.Instance != null && 
            GamePlayController.Instance.playerContain != null &&
            GamePlayController.Instance.playerContain.isRotateContainer)
        {
            OnPlayerDragged();
        }
    }

    private void OnPlayerDragged()
    {
        isDragged = true;
        
        if (infinitySequence != null)
        {
            infinitySequence.Kill();
            infinitySequence = null;
        }
        
        if (currentHand != null)
        {
            currentHand.transform.DOKill();
            Destroy(currentHand.gameObject);
            currentHand = null;
        }
        
        controller?.NextTut();
    }

    protected override void SetNameTut()
    {
        nameTut = "TutGamePlay_Step_2";
    }
}
