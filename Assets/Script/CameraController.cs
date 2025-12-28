using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public CameraScriptableobject CameraScriptableobject;
    public float zoomSpeed = 0.1f;
    public float minZoom = 35f;
    public float maxZoom = 65f;

    public float pinchThreshold = 10f;
    [SerializeField] private bool isPinching = false;
    public bool IsPinching => isPinching;
    
    private bool wasPinching = false; // Lưu trạng thái pinch của frame trước
    private float pinchEndTime = 0f;
    public float pinchCooldown = 0.3f; // Thời gian delay sau khi kết thúc pinch
    public bool JustFinishedPinching => !isPinching && wasPinching && (Time.time - pinchEndTime) < pinchCooldown;
    
    private int previousTouchCount = 0; // Số lượng touch của frame trước
    public bool IsStartingPinch => Input.touchCount == 2 && previousTouchCount < 2;

    private void Start()
    {
        for(int i=0;i<CameraScriptableobject.levelList.Count; i++)
        {
            if (CameraScriptableobject.levelList[i].level == UseProfile.CurrentLevel)
            {
                cam.fieldOfView = CameraScriptableobject.levelList[i].fov;
                break;
            }
            else if (CameraScriptableobject.levelList[i].level > UseProfile.CurrentLevel)
            {
                cam.fieldOfView = 60;
                break;
            }
        }
    }
    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        TestZoomOnPC();
#else
        MobileZoom();
#endif
    }
    void MobileZoom()
    {
        wasPinching = isPinching;
        
        int currentTouchCount = Input.touchCount;
        
        if (currentTouchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 t0Prev = t0.position - t0.deltaPosition;
            Vector2 t1Prev = t1.position - t1.deltaPosition;

            float oldDist = (t0Prev - t1Prev).magnitude;
            float newDist = (t0.position - t1.position).magnitude;

            float delta = newDist - oldDist;

            if (!isPinching)
            {
                if (Mathf.Abs(delta) > pinchThreshold)
                    isPinching = true;
                else
                    return;
            }

            float pinchAmount = delta * zoomSpeed * 0.01f;

            cam.fieldOfView -= pinchAmount;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);
        }
        else
        {
            if (isPinching && currentTouchCount < 2)
            {
                pinchEndTime = Time.time;
            }
            isPinching = false;
        }
        
        previousTouchCount = currentTouchCount;
    }
    void TestZoomOnPC()
    {
        float scroll = Input.mouseScrollDelta.y;
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            isPinching = true;
            float zoomAmount = scroll * 10f * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(
                cam.fieldOfView - zoomAmount,
                minZoom,
                maxZoom
            );
        }
        else
        {
            if (isPinching)
            {
                pinchEndTime = Time.time;
            }
            isPinching = false;
        }
    }
}
