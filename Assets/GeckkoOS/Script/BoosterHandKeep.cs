using DG.Tweening;
using EventDispatcher;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private Camera anchorCamera;
    [SerializeField] private Vector2 viewportAnchor = new Vector2(0.5f, 0.1f);
    [SerializeField] private float anchorDepthFromCamera = 45f;
    [SerializeField] private float referenceFieldOfView = 40f;
    [SerializeField] private float referenceOrthographicSize = 5f;
    [SerializeField] private Vector3 handScaleMultiplier = Vector3.one;
    [SerializeField] private bool matchCameraRotation = true;

    public GameObject currentWorm;

    private Transform _runtimeHandAnchor;
    private bool _handFollowsAnchor;

    private void Awake()
    {
        if (handPos == null)
        {
            var go = new GameObject("HandAnchor_Runtime");
            go.transform.SetParent(transform, false);
            _runtimeHandAnchor = go.transform;
            handPos = _runtimeHandAnchor;
        }
    }

    private Camera GetAnchorCamera()
    {
        if (anchorCamera != null)
            return anchorCamera;
        return Camera.main;
    }

    private float GetZoomScreenSizeRatio(Camera cam)
    {
        if (cam == null)
            return 1f;
        if (cam.orthographic)
        {
            if (referenceOrthographicSize < 0.0001f)
                return 1f;
            return cam.orthographicSize / referenceOrthographicSize;
        }

        float refTan = Mathf.Tan(referenceFieldOfView * 0.5f * Mathf.Deg2Rad);
        if (refTan < 0.0001f)
            return 1f;
        float curTan = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return curTan / refTan;
    }

    private void RefreshHandAnchorTransform()
    {
        if (handPos == null)
            return;

        Camera cam = GetAnchorCamera();
        if (cam == null)
            return;

        Vector3 worldPoint = cam.ViewportToWorldPoint(
            new Vector3(viewportAnchor.x, viewportAnchor.y, anchorDepthFromCamera));

        handPos.position = worldPoint;

        if (matchCameraRotation)
            handPos.rotation = cam.transform.rotation;
        else
            handPos.rotation = Quaternion.identity;

        float ratio = GetZoomScreenSizeRatio(cam);
        handPos.localScale = Vector3.Scale(handScaleMultiplier, Vector3.one * ratio);
    }

    private void SyncHandToAnchor()
    {
        if (hand == null || handPos == null || handPrefab == null || !_handFollowsAnchor)
            return;

        hand.transform.SetPositionAndRotation(handPos.position, handPos.rotation);
        hand.transform.localScale = Vector3.Scale(handPrefab.transform.localScale, handPos.lossyScale);
    }

    public void Init(PlayerContain param)
    {
        
        wasUseTNT_Booster = false;
        if (UseProfile.CurrentLevel >= 0)//9
        {
            HandleUnlock();
            HandKeep_Btn.onClick.AddListener(HandleAtom_Booster);

        }
        else
        {
            HandleLock();

        }
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CHANGE_TNT_BOOSTER, ChangeText);


        void HandleLock()
        {


            HandKeep_Btn.onClick.AddListener(HandleLockBtn);
        }
    }
    public void HandleUnlock()
    {
        if (UseProfile.TNT_Booster > 0)
        {
            objNum.SetActive(true);
            tvNum.text = UseProfile.TNT_Booster.ToString();
            lockIcon.SetActive(false);
        }
        else
        {
            objNum.SetActive(false);
            lockIcon.SetActive(true);
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

            RefreshHandAnchorTransform();
            hand = Instantiate(handPrefab, handPos.position, handPos.rotation, null);
            hand.transform.localScale = Vector3.Scale(handPrefab.transform.localScale, handPos.lossyScale);
            hand.SetActive(true);
            _handFollowsAnchor = true;
            wasUseTNT_Booster = true;
        }
        else
        {
            GameController.Instance.admobAds.ShowRewardedAd(
                     actionReward: () =>
                     {
                         UseProfile.TNT_Booster += 3; 
                     },
                     actionNotLoadedVideo: () =>
                     {

                     },
                       ActionWatchVideo.TNT_Booster
                    );
        }

    }


    

    public void ChangeText(object param)
    {
        tvNum.text = UseProfile.TNT_Booster.ToString();
        if (UseProfile.TNT_Booster > 0)
        {
            objNum.SetActive(true);
            tvNum.text = UseProfile.TNT_Booster.ToString();
            lockIcon.SetActive(false);
        }
        else
        {
            objNum.SetActive(false);
            lockIcon.SetActive(true);
        }
    }
    public void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CHANGE_TNT_BOOSTER, ChangeText);
    }

    private void LateUpdate()
    {
        if (!wasUseTNT_Booster && hand == null)
            return;

        RefreshHandAnchorTransform();
        SyncHandToAnchor();
    }

    private void Update()
    {
        if (!wasUseTNT_Booster || hand == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Camera cam = GetAnchorCamera();
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            return;

        WormController worm = hit.collider.GetComponentInParent<WormController>();
        if (worm == null || currentWorm != null)
            return;

        currentWorm = worm.gameObject;
        _handFollowsAnchor = false;
        hand.transform.DOKill(false);

        Transform wormTf = worm.transform;
        hand.transform.DOMove(hit.point, 0.5f).OnComplete(() =>
        {
            if (hand == null || wormTf == null)
            {
                currentWorm = null;
                return;
            }

            wormTf.SetParent(hand.transform, worldPositionStays: true);

            RefreshHandAnchorTransform();
            hand.transform.DOMove(handPos.position, 0.5f).OnComplete(() =>
            {
                wasUseTNT_Booster = false;
                _handFollowsAnchor = false;

                if (hand != null)
                    hand.SetActive(false);

                if (currentWorm != null)
                {
                    var levelData = GamePlayController.Instance?.playerContain?.levelController?.levelData;
                    levelData?.lsWormsInGame?.Remove(currentWorm);
                    Destroy(currentWorm);
                }

                currentWorm = null;
                GamePlayController.Instance?.gameScene?.HandleCheckWin();
            });
        });
    }







}
