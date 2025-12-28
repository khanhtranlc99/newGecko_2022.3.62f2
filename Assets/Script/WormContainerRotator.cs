using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WormContainerRotator : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed = 10;
    public Transform centerPoint;

    [Header("Performance - Rotate Mode")]
    public bool disableSplineMeshWhileRotating = true;
    public bool freezeWormVisualWhileRotating = true;
    [Range(1, 100)] public int restoreBatchPerFrame = 25; // 20~40 cho 200-300 worm

    [Header("Tap vs Drag")]
    public float tapTimeThreshold = 0.25f;
    public float tapDistanceThreshold = 10f;

    // input
    private bool dragging;
    private Vector3 lastMousePos;
    private Vector3 mouseDownPosition;
    private float mouseDownTime;
    private WormController clickedWorm;

    // late-apply rotation
    private Vector3 pendingDelta;
    private bool hasRotateDelta;

    // orbit rotation state
    private Quaternion currentRotation;
    private Vector3 initialOffsetFromCenter;

    // cache (build once)
    private readonly List<WormController> cachedWorms = new();

    // rotate mode state
    private bool inRotateMode = false;
    private bool splineMeshDisabledByDrag = false;
    private Coroutine restoreRoutine;

    // refs
    private LevelEdit level;
    private Camera cam;

    void Awake()
    {
        level = GetComponent<LevelEdit>();
        cam = Camera.main;
    }

    void Start()
    {
        currentRotation = transform.rotation;
        if (centerPoint != null)
            initialOffsetFromCenter = transform.position - centerPoint.position;

        BuildCache(); // quan trọng: cache trước
    }

    // Gọi lại khi bạn spawn level mới / reload level
    public void BuildCache()
    {
        cachedWorms.Clear();

        if (level == null || level.lsWormsInGame == null) return;

        foreach (var go in level.lsWormsInGame)
        {
            if (!go) continue;

            var wc = go.GetComponent<WormController>();
            if (wc) cachedWorms.Add(wc);
        }
    }

    void Update()
    {
        // ===== POPUP BLOCK =====
        if (GamePlayController.Instance.playerContain.isPopupUp)
        {
            ExitRotateModeIfNeeded();
            CancelDragState();
            return;
        }

        // ===== PINCH CHECK =====
        var camCtrl = GamePlayController.Instance.cameraController;
        bool shouldIgnoreInput = camCtrl != null && (camCtrl.IsPinching || camCtrl.JustFinishedPinching || camCtrl.IsStartingPinch);
        if (shouldIgnoreInput)
        {
            ExitRotateModeIfNeeded();
            CancelDragState();

            if (camCtrl != null && camCtrl.IsStartingPinch)
                lastMousePos = Input.mousePosition;

            return;
        }

        // ===== MOUSE DOWN =====
        if (Input.GetMouseButtonDown(0))
        {
            clickedWorm = RaycastWorm();

            dragging = true;
            lastMousePos = Input.mousePosition;
            mouseDownPosition = Input.mousePosition;
            mouseDownTime = Time.time;

            // chỉ cho rotate ngay nếu không click vào worm
            if (clickedWorm == null)
                GamePlayController.Instance.playerContain.isRotateContainer = true;
        }

        // ===== MOUSE UP =====
        if (Input.GetMouseButtonUp(0))
        {
            float moveDistance = Vector3.Distance(Input.mousePosition, mouseDownPosition);

            // tap vào worm (không kéo)
            if (clickedWorm != null && moveDistance < tapDistanceThreshold)
            {
                clickedWorm.GetComponent<SplineMesh>().autoUpdate = true;
                clickedWorm.ActivateWorm();
                
            }

            ExitRotateModeIfNeeded();
            CancelDragState();
            return;
        }

        // ===== DRAGGING =====
        if (!dragging)
        {
            hasRotateDelta = false;
            return;
        }

        float dragDistance = Vector3.Distance(Input.mousePosition, mouseDownPosition);

        // Nếu click vào worm nhưng kéo đủ xa => chuyển sang rotate container
        if (clickedWorm != null && dragDistance >= tapDistanceThreshold)
            GamePlayController.Instance.playerContain.isRotateContainer = true;

        bool rotatingNow = GamePlayController.Instance.playerContain.isRotateContainer
                           && dragDistance >= tapDistanceThreshold;

        // vào rotate mode đúng thời điểm (1 lần)
        if (rotatingNow)
            EnterRotateModeIfNeeded();
        else
            ExitRotateModeIfNeeded(); // nếu user không kéo nữa / chưa đủ ngưỡng

        // lấy delta để LateUpdate apply (mượt + ổn định)
        pendingDelta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;
        hasRotateDelta = rotatingNow; // chỉ rotate khi đang rotate thật
    }

    void LateUpdate()
    {
        if (!hasRotateDelta || centerPoint == null)
            return;

        if (!GamePlayController.Instance.playerContain.isRotateContainer)
            return;

        float rotX = pendingDelta.y * rotateSpeed * Time.deltaTime;
        float rotY = -pendingDelta.x * rotateSpeed * Time.deltaTime;

        Quaternion yaw = Quaternion.AngleAxis(rotY, Vector3.up);
        Quaternion pitch = Quaternion.AngleAxis(rotX, Vector3.right);

        currentRotation = yaw * pitch * currentRotation;

        Vector3 rotatedOffset = currentRotation * initialOffsetFromCenter;
        transform.position = centerPoint.position + rotatedOffset;
        transform.rotation = currentRotation;
    }

    // =========================
    // Rotate Mode
    // =========================

    void EnterRotateModeIfNeeded()
    {
        if (inRotateMode) return;
        inRotateMode = true;

        // nếu đang restore dở thì stop (tránh giật)
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        if (disableSplineMeshWhileRotating && !splineMeshDisabledByDrag)
        {
            splineMeshDisabledByDrag = true;
        }

        if (freezeWormVisualWhileRotating)
        {
            for (int i = 0; i < cachedWorms.Count; i++)
            {
                var w = cachedWorms[i];
                if (!w) continue;
                // bạn tự quyết: có muốn skip worm đang autoMove không?
                if (w.autoMove) continue;
                w.SetRotateSuspend(true);
            }
        }
    }

    void ExitRotateModeIfNeeded()
    {
        if (!inRotateMode) return;
        inRotateMode = false;

        GamePlayController.Instance.playerContain.isRotateContainer = false;
        hasRotateDelta = false;

        // restore theo batch để không spike
        if (disableSplineMeshWhileRotating && splineMeshDisabledByDrag)
        {
            if (restoreRoutine == null)
                restoreRoutine = StartCoroutine(RestoreSmooth());
        }
        else
        {
            // nếu không tắt spline mesh thì vẫn cần unfreeze worm
            if (freezeWormVisualWhileRotating)
            {
                for (int i = 0; i < cachedWorms.Count; i++)
                    if (cachedWorms[i]) cachedWorms[i].SetRotateSuspend(false);
            }
        }
    }

    IEnumerator RestoreSmooth()
    {
        int batch = Mathf.Max(1, restoreBatchPerFrame);

        for (int i = 0; i < cachedWorms.Count; i++)
        {
            if (freezeWormVisualWhileRotating && i < cachedWorms.Count)
            {
                var w = cachedWorms[i];
                if (w) w.SetRotateSuspend(false);
            }

            if ((i + 1) % batch == 0) // (i+1) tránh yield ngay i=0
                yield return null;
        }

        splineMeshDisabledByDrag = false;
        restoreRoutine = null;
    }

    // =========================
    // Helpers
    // =========================

    WormController RaycastWorm()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return null;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.GetComponentInParent<WormController>();

        return null;
    }

    void CancelDragState()
    {
        dragging = false;
        clickedWorm = null;
        hasRotateDelta = false;
    }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        if(centerPoint==null)
        {
            var t = transform.Find("PositionsContainer_t");
            if (t != null) centerPoint = t;
        }

        EditorUtility.SetDirty(this);
    }
#endif
}
