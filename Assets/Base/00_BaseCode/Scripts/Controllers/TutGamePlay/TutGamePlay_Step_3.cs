using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutGamePlay_Step_3 : TutorialBase
{
    [SerializeField] private Vector3 centerPosition;
    [SerializeField] private float pinchDistance = 1f;
    [SerializeField] private float spreadDistance = 2f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private float delayBetweenObjects = 0.2f;
    [SerializeField] private float diagonalAngle = 45f;
    
    private GameObject currentObject1;
    private GameObject currentObject2;
    private Sequence zoomSequence1;
    private Sequence zoomSequence2;
    private Coroutine spawnObjectsCoroutine;
    private bool isZoomed = false;

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel == 5 && base.IsCanShowTut()) return true;
        else return false;
    }
    

    public override bool IsCanEndTut()
    {
        if (!isZoomed)
            return false;
            
        if (zoomSequence1 != null)
        {
            zoomSequence1.Kill();
            zoomSequence1 = null;
        }
        
        if (zoomSequence2 != null)
        {
            zoomSequence2.Kill();
            zoomSequence2 = null;
        }
        
        if (spawnObjectsCoroutine != null)
        {
            StopCoroutine(spawnObjectsCoroutine);
            spawnObjectsCoroutine = null;
        }
        
        if (currentObject1 != null)
        {
            currentObject1.transform.DOKill();
            Destroy(currentObject1);
        }
        
        if (currentObject2 != null)
        {
            currentObject2.transform.DOKill();
            Destroy(currentObject2);
        }
        
        return true;
    }

    public override void StartTut()
    {
        centerPosition = transform.position;
        
        if (GamePlayController.Instance != null && 
            GamePlayController.Instance.cameraController != null)
        {
            var camCtrl = GamePlayController.Instance.cameraController;
            
            if (camCtrl.IsPinching || camCtrl.IsStartingPinch)
            {
                OnPlayerZoomed();
                return;
            }
        }
        
        spawnObjectsCoroutine = StartCoroutine(SpawnObjects());
    }
    
    IEnumerator SpawnObjects()
    {
        if (isZoomed) yield break;
        
        yield return new WaitForSeconds(0.5f);
        
        if (isZoomed) yield break;
        
        if (handTut == null)
        {
            Debug.LogWarning("[TutGamePlay_Step_3] handTut prefab chưa được gán!");
            yield break;
        }
        
        float angleRad = diagonalAngle * Mathf.Deg2Rad;
        Vector3 diagonalDir1 = new Vector3(-Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0);
        Vector3 diagonalDir2 = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
        
        Vector3 object1StartPos = centerPosition + diagonalDir1 * (pinchDistance / 2f);
        object1StartPos.z = centerPosition.z;
        Vector3 object2StartPos = centerPosition + diagonalDir2 * (pinchDistance / 2f);
        object2StartPos.z = centerPosition.z;
        
        currentObject1 = Instantiate(handTut, object1StartPos, handTut.transform.rotation);
        currentObject1.SetActive(true);
        currentObject1.transform.localScale = Vector3.one;
        
        yield return new WaitForSeconds(delayBetweenObjects);
        
        if (isZoomed) yield break;
        
        currentObject2 = Instantiate(handTut, object2StartPos, handTut.transform.rotation);
        currentObject2.SetActive(true);
        currentObject2.transform.localScale = Vector3.one;
        
        StartPinchAnimation();
    }
    private void StartPinchAnimation()
    {
        if (currentObject1 == null || currentObject2 == null) return;
        
        float angleRad = diagonalAngle * Mathf.Deg2Rad;
        Vector3 diagonalDir1 = new Vector3(-Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0);
        Vector3 diagonalDir2 = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);
        
        Vector3 object1SpreadPos = centerPosition + diagonalDir1 * (spreadDistance / 2f);
        object1SpreadPos.z = centerPosition.z;
        Vector3 object2SpreadPos = centerPosition + diagonalDir2 * (spreadDistance / 2f);
        object2SpreadPos.z = centerPosition.z;

        Vector3 object1PinchPos = centerPosition + diagonalDir1 * (pinchDistance / 2f);
        object1PinchPos.z = centerPosition.z;
        Vector3 object2PinchPos = centerPosition + diagonalDir2 * (pinchDistance / 2f);
        object2PinchPos.z = centerPosition.z;

        zoomSequence1 = DOTween.Sequence();
        zoomSequence1.Append(currentObject1.transform.DOMove(object1SpreadPos, zoomDuration).SetEase(Ease.Linear)); 
        zoomSequence1.Append(currentObject1.transform.DOMove(object1PinchPos, zoomDuration).SetEase(Ease.Linear)); 
        zoomSequence1.SetLoops(-1); 
        
        zoomSequence2 = DOTween.Sequence();
        zoomSequence2.Append(currentObject2.transform.DOMove(object2SpreadPos, zoomDuration).SetEase(Ease.Linear)); 
        zoomSequence2.Append(currentObject2.transform.DOMove(object2PinchPos, zoomDuration).SetEase(Ease.Linear));
        zoomSequence2.SetLoops(-1); 
    }

    protected override void OnUpdate()
    {
        if (isZoomed) return;
        
        if (GamePlayController.Instance != null && 
            GamePlayController.Instance.cameraController != null)
        {
            var camCtrl = GamePlayController.Instance.cameraController;
            
            if (camCtrl.IsPinching || camCtrl.IsStartingPinch)
            {
                OnPlayerZoomed();
            }
        }
    }
    
    private void OnPlayerZoomed()
    {
        isZoomed = true;
        
        if (zoomSequence1 != null)
        {
            zoomSequence1.Kill();
            zoomSequence1 = null;
        }
        
        if (zoomSequence2 != null)
        {
            zoomSequence2.Kill();
            zoomSequence2 = null;
        }
        
        if (currentObject1 != null)
        {
            currentObject1.transform.DOKill();
            Destroy(currentObject1);
            currentObject1 = null;
        }
        
        if (currentObject2 != null)
        {
            currentObject2.transform.DOKill();
            Destroy(currentObject2);
            currentObject2 = null;
        }
        
        controller?.NextTut();
    }

    protected override void SetNameTut()
    {
        nameTut = "TutGamePlay_Step_3";
    }
}
