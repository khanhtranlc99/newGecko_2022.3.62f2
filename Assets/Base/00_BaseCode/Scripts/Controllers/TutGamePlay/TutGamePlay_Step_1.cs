using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutGamePlay_Step_1 : TutorialBase
{
  
    GameObject currentHand;
    [SerializeField] private Transform targetWorm;
    [SerializeField] private float handOffsetY = 1f;
    [SerializeField] private float handOffsetX = 1f;
    [SerializeField] private float handOffsetZ = 1f;
    [SerializeField] private float animationDistance = 0.5f;
    
    private Sequence pointingSequence;
    private bool isTracking = false;
    private Vector3 lastWormPosition;
    private Coroutine spawnHandCoroutine;
    private GameObject targetWormGameObject;
    private bool isWormClicked = false;

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel == 1 && base.IsCanShowTut()) return true;
        else return false;
    }
    public override bool IsCanEndTut()
    {
        if (!isWormClicked)
            return false;
            
        isTracking = false;
        if(pointingSequence != null)
        {
            pointingSequence.Kill();
            pointingSequence = null;
        }
        if(currentHand != null)
        {
            currentHand.transform.DOKill();
            Destroy(currentHand.gameObject);
        }

        return true; 
    }

    public override void StartTut()
    {
        if(UseProfile.LevelEggChest == 1)
        {
            spawnHandCoroutine = StartCoroutine(SpawnHand());
        }    
    }


    IEnumerator SpawnHand()
    {
        yield return new WaitForSeconds(0.5f);
        
        if (targetWorm == null)
        {
            targetWorm = GetTargetWorm();
        }
        
        if (targetWorm == null)
        {
            Debug.LogWarning("[TutGamePlay_Step_1] Không tìm thấy worm để chỉ tay!");
            yield break;
        }
        
        targetWormGameObject = null;
        WormController wormController = targetWorm.GetComponent<WormController>();
        if (wormController != null)
        {
            targetWormGameObject = targetWorm.gameObject;
        }
        else
        {
            Transform parent = targetWorm.parent;
            while (parent != null)
            {
                wormController = parent.GetComponent<WormController>();
                if (wormController != null)
                {
                    targetWormGameObject = parent.gameObject;
                    break;
                }
                parent = parent.parent;
            }
            
            if (targetWormGameObject == null)
            {
                targetWormGameObject = targetWorm.gameObject;
            }
        }
        
        if (handTut == null)
        {
            Debug.LogWarning("[TutGamePlay_Step_1] handTut prefab chưa được gán!");
            yield break;
        }
        
        Vector3 wormPosition = GetWormWorldPosition(targetWorm);
        Vector3 handStartPosition = wormPosition + Vector3.up * handOffsetY;
        
        currentHand = Instantiate(handTut, handStartPosition, handTut.transform.rotation);
        currentHand.SetActive(true);
        
        isTracking = true;
        lastWormPosition = wormPosition;
        StartHandPointingAnimation(wormPosition);
    }
    
    private Transform GetTargetWorm()
    {
        try
        {
            var levelData = GamePlayController.Instance?.playerContain?.levelController?.levelData;
            if (levelData != null && levelData.lsWormsInGame != null && levelData.lsWormsInGame.Count > 0)
            {
                GameObject firstWorm = levelData.lsWormsInGame[9];
                if (firstWorm != null)
                {
                    WormController wormController = firstWorm.GetComponent<WormController>();
                    if (wormController != null && wormController.head != null)
                    {
                        return wormController.head;
                    }
                    return firstWorm.transform;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TutGamePlay_Step_1] Lỗi khi lấy worm: {e.Message}");
        }
        
        return null;
    }
    private Vector3 GetWormWorldPosition(Transform wormTransform)
    {
        if (wormTransform == null) return Vector3.zero;
        
        return wormTransform.position;
    }
    private void StartHandPointingAnimation(Vector3 targetPosition)
    {
        if (currentHand == null) return;
        
        if (pointingSequence != null)
        {
            pointingSequence.Kill();
        }
        
        Vector3 startPos = targetPosition + Vector3.right * handOffsetX + Vector3.up * handOffsetY + Vector3.forward * handOffsetZ;
        Vector3 endPos = targetPosition + Vector3.right * (handOffsetX - animationDistance) + Vector3.up * handOffsetY + Vector3.forward * (handOffsetZ - animationDistance);
        
        currentHand.transform.position = startPos;
        
        pointingSequence = DOTween.Sequence();
        pointingSequence.Append(currentHand.transform.DOMove(endPos, 0.5f).SetEase(Ease.OutQuad));
        pointingSequence.Append(currentHand.transform.DOMove(startPos, 0.5f).SetEase(Ease.InQuad));
        pointingSequence.SetLoops(-1);
    }
    protected override void OnUpdate()
    {
        if (!isTracking || targetWormGameObject == null) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            WormController clickedWorm = RaycastWorm();
            if (clickedWorm != null && clickedWorm.gameObject == targetWormGameObject)
            {
                OnWormClicked();
            }
        }
    }
    
    private void LateUpdate()
    {
        if (!isTracking || currentHand == null || targetWorm == null) return;
        
        Vector3 wormPosition = GetWormWorldPosition(targetWorm);
        
        float distance = Vector3.Distance(wormPosition, lastWormPosition);
        if (distance > 0.01f)
        {
            lastWormPosition = wormPosition;
            StartHandPointingAnimation(wormPosition);
        }
    }
    
    private WormController RaycastWorm()
    {
        Camera cam = Camera.main;
        if (!cam) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<WormController>();
        }

        return null;
    }
    
    private void OnWormClicked()
    {
        isWormClicked = true;
        
        isTracking = false;
        
        if (spawnHandCoroutine != null)
        {
            StopCoroutine(spawnHandCoroutine);
            spawnHandCoroutine = null;
        }
        
        if (pointingSequence != null)
        {
            pointingSequence.Kill();
            pointingSequence = null;
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
        nameTut = "TutGamePlay_Step_1";
    }
    
    public override void OnEndTut()
    {
        base.OnEndTut();
        Destroy(gameObject);
    }
}
