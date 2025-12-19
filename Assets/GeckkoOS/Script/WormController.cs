using System.Collections;
using Dreamteck.Splines;
using MoreMountains.NiceVibrations;
using UnityEngine;

public class WormController : MonoBehaviour
{
    [Header("Refs")]
    public SplineComputer spline;  // spline thân sâu
    public Transform root;         // container (WormContainerRotator)
    public Transform head;         // đầu sâu (visual) ở point 0
    public Transform tail;
    public Transform frontLeg;  
    public Transform backLeg;
    private Material currentMaterial;

    [Header("Move Settings")]
    public float cellSize = 1f;        // 1 bước dài bao nhiêu
    public float stepDuration = 0.12f; // thời gian 1 bước
    public bool loopMove = false;      // nếu true thì cứ lặp bước
    public bool isBlock = false;

    [Header("Visual Offsets")]
    public float headSinkDistance = 0.2f;
    public float tailSinkDistance = 0.05f;
    public float legMidT = 0.5f; 
    public float legSinkDistance = 0.1f;

    [Header("Debug")]
    public bool debugLog = false;
    public bool checkMeshTwist = false; // Bật để kiểm tra mesh bị xoắn
    public float twistThreshold = 45f; // Góc quay tối đa (độ) giữa 2 normals để coi là xoắn
    public bool drawNormals = false; // Vẽ normals trong Scene view để debug
    public float normalDrawLength = 0.5f; // Độ dài của line vẽ normal

    // Hướng di chuyển trên GRID (±X, ±Y, ±Z)
    private Vector3Int moveDirGrid;

    private bool isStepping = false;
    private bool isMoving = false;
    public bool autoMove = false;
    public float headCheckRadius = 0.3f;
    private bool hasCollided = false;
    private Renderer headRenderer;
    private FaceAnimation faceAnim;
    private Animator frontLegAnim;
    private Animator backLegAnim;
    public System.Action OnStepFinished;

    // ====== Bộ nhớ spline ======
    /// <summary>Trạng thái spline hiện tại (authoritative) – KHÔNG GetPoints mỗi step nữa.</summary>
    private SplinePoint[] statePoints;

    /// <summary>Backup spline trước khi ACTIVE (để quay về nếu bị block).</summary>
    private SplinePoint[] activationBackupPoints;

    /// <summary>Backup spline ban đầu khi spawn (nếu cần dùng sau).</summary>
    private SplinePoint[] spawnInitialPoints;

    // Reuse buffer để giảm GC trong mỗi bước
    private SplinePoint[] bufferFrom;
    private SplinePoint[] bufferTo;
    private SplinePoint[] bufferWork;

    private bool isReturningToStart = false;
    private int stepCount = 0;
    private Coroutine destroyCoroutine;

    void Start()
    {
        if (!spline) spline = GetComponent<SplineComputer>();
        if (!root) root = GetComponentInParent<WormContainerRotator>()?.transform;
        stepDuration = 0.19f;
        tailSinkDistance = 0.08f;
        // Làm việc trong LOCAL space của con worm
        spline.space = SplineComputer.Space.Local;

        // Lấy spline 1 lần duy nhất khi start, sau đó tự quản bên trong
        var pts = spline.GetPoints(SplineComputer.Space.Local);
        if (pts == null || pts.Length == 0)
        {
            if (debugLog) Debug.LogWarning($"[WormController] {gameObject.name}: Không có spline points khi Start!");
            return;
        }

        int n = pts.Length;
        statePoints = new SplinePoint[n];
        System.Array.Copy(pts, statePoints, n);

        spawnInitialPoints = new SplinePoint[n];
        System.Array.Copy(pts, spawnInitialPoints, n);

        EnsureBuffers(n);

        headRenderer = head ? head.GetComponentInChildren<Renderer>(true) : null;
        faceAnim = head ? head.GetComponentInChildren<FaceAnimation>(true) : null;
        frontLegAnim = frontLeg ? frontLeg.GetComponentInChildren<Animator>(true) : null;
        backLegAnim = backLeg ? backLeg.GetComponentInChildren<Animator>(true) : null;

        currentMaterial = headRenderer ? headRenderer.sharedMaterial : null;
        spline.SetPoints(statePoints, SplineComputer.Space.Local);
        this.GetComponent<SplineMesh>().autoUpdate = false;
        //SyncEndsToSpline();
        faceAnim.StartAnim();
    }
    public void ActivateWorm()
    {
        
        if (isMoving)
        {
            GameController.Instance.musicManager.PlayClickFailSound();
            return;
        }
        // Đang dùng TNT booster thì sâu không được chạy
        if (GamePlayController.Instance.playerContain.boosterHandKeep.wasUseTNT_Booster)
        {
            GameController.Instance.musicManager.PlayClickFailSound();
            return;
        }
        
        if (!head || !spline || statePoints == null || statePoints.Length == 0) return;
        if (!root) root = GetComponentInParent<WormContainerRotator>()?.transform;

        // 1) Lấy hướng grid trước để kiểm tra va chạm ngay lập tức
        moveDirGrid = GetDirByRotation();

        // 2) Kiểm tra va chạm TRƯỚC khi làm các tác vụ nặng (như copy array)
        if (IsBlockedAhead())
        {
            autoMove = false;
            if (!isBlock) GamePlayController.Instance.gameScene.SubNumMove();
            isBlock = true;
            isMoving = false; // Giải phóng trạng thái để có thể click lại
            return;
        }

        // 3) Nếu KHÔNG bị chặn, phát âm thanh thành công NGAY LẬP TỨC
        GameController.Instance.musicManager.PlayClickSuccessSound();

        // 4) Sau đó mới thực hiện các bước chuẩn bị di chuyển
        isMoving = true;
        int n = statePoints.Length;

        // Lưu lại hình dạng spline hiện tại (statePoints) để có thể quay về nếu bị chặn
        if (activationBackupPoints == null || activationBackupPoints.Length != n)
            activationBackupPoints = new SplinePoint[n];

        System.Array.Copy(statePoints, activationBackupPoints, n);

        autoMove = true;
        isReturningToStart = false;

        // Không cho nhiều StepLoop chạy chồng
        if (!isStepping)
        {
            isStepping = true;
            PlayLegAnim();
            StartCoroutine(StepLoop());
        }
    }

    IEnumerator StepLoop()
    {

        // Delay 1 frame để tránh dồn nặng đúng frame tap
        yield return null;

        while (autoMove || loopMove)
        {
            yield return MoveOneStep();
            OnStepFinished?.Invoke();
        }

        isStepping = false;
        isMoving = false;
        if (debugLog) Debug.Log($"[WormController] {gameObject.name}: StepLoop kết thúc - autoMove: {autoMove}, loopMove: {loopMove}");
    }

    IEnumerator MoveOneStep(System.Action onDone = null)
    {
        if (statePoints == null || statePoints.Length == 0)
        {
            if (debugLog) Debug.LogWarning($"[WormController] {gameObject.name}: KHÔNG DI CHUYỂN ĐƯỢC - statePoints rỗng!");
            yield break;
        }
        if (!isMoving)
            yield break;
        int n = statePoints.Length;
        EnsureBuffers(n);

        // Kiểm tra chướng ngại phía trước trước khi di chuyển
       
        headRenderer.sharedMaterial = currentMaterial;
        // from = state hiện tại
        System.Array.Copy(statePoints, bufferFrom, n);

        var from = bufferFrom;
        var to = bufferTo;
        var work = bufferWork;

        Transform segParent = transform;

        // 2) Tính headTargetLocal dựa trên moveDirGrid (hướng grid trong root)
        Vector3 headCurrentLocal = from[0].position;

        // dirWorld = hướng WORLD tương ứng với moveDirGrid
        Vector3 dirWorld = root.TransformDirection((Vector3)moveDirGrid);

        // Đưa về LOCAL của worm
        Vector3 dirLocal = segParent.InverseTransformDirection(dirWorld).normalized;
        if (dirLocal.sqrMagnitude < 0.0001f)
        {
            if (debugLog) Debug.LogWarning($"[WormController] {gameObject.name}: KHÔNG DI CHUYỂN ĐƯỢC - Hướng di chuyển không hợp lệ (dirLocal: {dirLocal})");
            yield break;
        }

        Vector3 headTargetLocal = headCurrentLocal + dirLocal * cellSize;

        // 3) Xây mảng to: headTarget + các point chạy về vị trí point trước
        to[0] = from[0];
        to[0].position = headTargetLocal;

        for (int i = 1; i < n; i++)
        {
            to[i] = from[i];
            to[i].position = from[i - 1].position;
        }

        // 4) Lerp LOCAL from -> to trong stepDuration
        float t = 0f;
        float dur = Mathf.Max(stepDuration, 0.0001f);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float a = Mathf.Clamp01(t);

            for (int i = 0; i < n; i++)
            {
                work[i] = from[i];
                work[i].position = Vector3.Lerp(from[i].position, to[i].position, a);
            }

            spline.SetPoints(work, SplineComputer.Space.Local);

            // cập nhật head/tail theo spline đang lerp
            UpdateEndTransform(head, 0, 1, work, segParent, root, headSinkDistance, false);
            UpdateEndTransform(tail, n - 1, n - 2, work, segParent, root, tailSinkDistance, true);
            UpdateLegTransform(frontLeg, 0, 1, legMidT, work, segParent, head, legSinkDistance);
            // backLeg giữa segment 1-2 (nếu worm dài đủ)
            if (n > 2)
            {
                UpdateLegTransform(backLeg, 1, 2, legMidT, work, segParent, head, legSinkDistance);
            }
            else UpdateLegTransform(backLeg, 1, 1, legMidT, work, segParent, head, legSinkDistance);

            yield return null;   // mượt theo frame
        }

        // 5) Kiểm tra mesh bị xoắn, chỉ fix normals nếu cần thiết (tối ưu performance)
        bool needsFix = CheckMeshTwist(to, spline);
        if (needsFix)
        {
            FixNormalsParallelTransport(to, spline);
            
            // Log nếu bật debug
            if (checkMeshTwist && debugLog)
            {
                Debug.Log($"[WormController] {gameObject.name}: Đã fix normals do phát hiện mesh bị xoắn");
            }
        }
        else if (checkMeshTwist && debugLog)
        {
            // Log khi không phát hiện xoắn (để biết method có chạy không)
            Debug.Log($"[WormController] {gameObject.name}: Không phát hiện mesh bị xoắn (OK)");
        }
        
        // 6) Kết thúc step: set to + cập nhật statePoints
        spline.SetPoints(to, SplineComputer.Space.Local);
        System.Array.Copy(to, statePoints, n);

        UpdateEndTransform(head, 0, 1, to, segParent, root, headSinkDistance, false);
        UpdateEndTransform(tail, n - 1, n - 2, to, segParent, root, tailSinkDistance, true);
        UpdateLegTransform(frontLeg, 0, 1, legMidT, work, segParent, head, legSinkDistance);
        // backLeg giữa segment 1-2 (nếu worm dài đủ)
        if (n > 2)
        {
            UpdateLegTransform(backLeg, 1, 2, legMidT, work, segParent, head, legSinkDistance);
        }
        else UpdateLegTransform(backLeg, 1, 1, legMidT, work, segParent, head, legSinkDistance);

        // Kiểm tra lại vật cản sau khi di chuyển xong
        if (IsBlockedAhead())
        {
            autoMove = false;
            if (!isBlock) GamePlayController.Instance.gameScene.SubNumMove();
            isBlock = true;
            if (activationBackupPoints != null && activationBackupPoints.Length == n)
                yield return ReturnToStartPosition();
        }
        else
        {
            stepCount++;
            if (stepCount >= 5 && destroyCoroutine == null)
            {
                destroyCoroutine=StartCoroutine(DestroyAfterSeconds(2));
            }
        }

        onDone?.Invoke();
    }

    IEnumerator DestroyAfterSeconds(float delay)
    {
        if (debugLog) Debug.Log("DestroyAfterSeconds: " + delay);
        yield return new WaitForSeconds(delay);
        GamePlayController.Instance.playerContain.levelController.levelData.lsWormsInGame.Remove(this.gameObject);
        faceAnim.StopAnim();
        GamePlayController.Instance.gameScene.HandleCheckWin();
        Destroy(gameObject);
    }

    private void UpdateEndTransform(
        Transform end,
        int idxThis,
        int idxOther,
        SplinePoint[] ptsLocal,
        Transform segParent,
        Transform rootTransform,
        float sinkDistance,
        bool reverseDirection = false)
    {
        if (!end) return;
        int n = ptsLocal.Length;
        if (n == 0) return;

        Vector3 worldThis;
        Vector3 worldOther;
        
        // FIX: Tail dùng spline sample ở percent cuối (1.0) thay vì point cuối
        // để đảm bảo tail luôn khớp với cuối mesh extrude
        if (reverseDirection && end == tail && spline != null)
        {
            // Tail: dùng spline.Evaluate(1.0) để lấy vị trí cuối của mesh
            SplineSample tailSample = spline.Evaluate(1.0);
            worldThis = tailSample.position;
            
            // Tính direction từ sample gần cuối
            if (n > 1)
            {
                SplineSample prevSample = spline.Evaluate(0.95); // Sample gần cuối
                worldOther = prevSample.position;
            }
            else
            {
                worldOther = segParent.TransformPoint(ptsLocal[idxOther].position);
            }
        }
        else
        {
            // Head: dùng point như bình thường
            worldThis = segParent.TransformPoint(ptsLocal[idxThis].position);
            if (n <= 1) return;
            worldOther = segParent.TransformPoint(ptsLocal[idxOther].position);
        }

        end.position = worldThis;

        if (n <= 1) return;

        Vector3 d = reverseDirection ? (worldOther - worldThis) : (worldThis - worldOther);
        if (d.sqrMagnitude < 0.00001f) return;

        if (reverseDirection)
        {
            Vector3 forward = d.normalized;
            Vector3 up = rootTransform ? rootTransform.up : Vector3.up;
            end.rotation = Quaternion.LookRotation(forward, up);
        }
        // sink: dịch dọc theo forward để chui vào thân
        if (Mathf.Abs(sinkDistance) > 0.0001f)
        {
            end.Translate(Vector3.forward * sinkDistance, Space.Self);
        }
    }

    private void EnsureBuffers(int n)
    {
        if (bufferFrom == null || bufferFrom.Length != n)
        {
            bufferFrom = new SplinePoint[n];
            bufferTo = new SplinePoint[n];
            bufferWork = new SplinePoint[n];
        }
    }

    private bool IsBlockedAhead()
    {
        if (!head) return false;
        if (!root) root = transform;

        Vector3 dirWorld = root.TransformDirection((Vector3)moveDirGrid).normalized;
        if (dirWorld.sqrMagnitude < 0.0001f)
            return false;

        Vector3 origin = head.position + dirWorld * headCheckRadius;

        if (Physics.SphereCast(origin, headCheckRadius, dirWorld, out RaycastHit hit, cellSize))
        {
            // Nếu đụng object KHÔNG PHẢI là chính worm
            if (!hit.collider.transform.IsChildOf(head.transform))
            {
                // Ưu tiên phát âm thanh và rung ngay lập tức
                GameController.Instance.musicManager.PlayClickFailSound();
                if (GameController.Instance.useProfile.OnVibration) MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

                PlayIdleAnim();
                headRenderer.sharedMaterial = GamePlayController.Instance.playerContain.wrongMaterial;
                hasCollided = true;
                return true;
            }
        }

        return false;
    }

    private Vector3Int GetDirByRotation()
    {
        if (!head) return Vector3Int.forward;
        if (!root) root = transform;

        Vector3 dirWorld = head.transform.TransformDirection(Vector3.forward);
        Vector3 dirLocal = root.InverseTransformDirection(dirWorld);
        dirLocal.Normalize();

        float ax = Mathf.Abs(dirLocal.x);
        float ay = Mathf.Abs(dirLocal.y);
        float az = Mathf.Abs(dirLocal.z);

        if (ax >= ay && ax >= az)
            return dirLocal.x > 0 ? Vector3Int.right : Vector3Int.left;

        if (ay >= az)
            return dirLocal.y > 0 ? Vector3Int.up : Vector3Int.down;

        return dirLocal.z > 0 ? Vector3Int.forward : Vector3Int.back;
    }

    /// <summary>
    /// Di chuyển worm về vị trí trước khi Activate (activationBackupPoints)
    /// </summary>
    IEnumerator ReturnToStartPosition()
    {
        if (isReturningToStart || activationBackupPoints == null || activationBackupPoints.Length == 0)
            yield break;

        if (statePoints == null || statePoints.Length == 0)
            yield break;

        int n = statePoints.Length;
        if (activationBackupPoints.Length != n)
            yield break;

        isReturningToStart = true;
        if (debugLog) Debug.Log($"[WormController] {gameObject.name}: BẮT ĐẦU QUAY VỀ VỊ TRÍ BACKUP");
        headRenderer.sharedMaterial = GamePlayController.Instance.playerContain.wrongMaterial;

        EnsureBuffers(n);
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }

        System.Array.Copy(statePoints, bufferFrom, n);
        System.Array.Copy(activationBackupPoints, bufferTo, n);

        Transform segParent = transform;

        float t = 0f;
        float dur = Mathf.Max(stepDuration, 0.0001f);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float a = Mathf.Clamp01(t);

            for (int i = 0; i < n; i++)
            {
                bufferWork[i] = bufferFrom[i];
                bufferWork[i].position = Vector3.Lerp(bufferFrom[i].position, bufferTo[i].position, a);
            }

            spline.SetPoints(bufferWork, SplineComputer.Space.Local);

            UpdateEndTransform(head, 0, 1, bufferWork, segParent, root, headSinkDistance, false);
            UpdateEndTransform(tail, n - 1, n - 2, bufferWork, segParent, root, tailSinkDistance, true);
            UpdateLegTransform(frontLeg, 0, 1, legMidT, bufferWork, segParent, head, legSinkDistance);
            // backLeg giữa segment 1-2 (nếu worm dài đủ)
            if (n > 2)
            {
                UpdateLegTransform(backLeg, 1, 2, legMidT, bufferWork, segParent, head, legSinkDistance);
            }
            else UpdateLegTransform(backLeg, 1, 1, legMidT, bufferWork, segParent, head, legSinkDistance);

            yield return null;
        }
        FixNormalsParallelTransport(bufferWork, spline);
        // Set về đúng backup + cập nhật statePoints
        spline.SetPoints(activationBackupPoints, SplineComputer.Space.Local);
        System.Array.Copy(activationBackupPoints, statePoints, n);

        UpdateEndTransform(head, 0, 1, statePoints, segParent, root, 0, false);
        UpdateEndTransform(tail, n - 1, n - 2, statePoints, segParent, root, tailSinkDistance, true);
        UpdateLegTransform(frontLeg, 0, 1, legMidT, bufferWork, segParent, head, legSinkDistance);
        // backLeg giữa segment 1-2 (nếu worm dài đủ)
        if (n > 2)
        {
            UpdateLegTransform(backLeg, 1, 2, legMidT, bufferWork, segParent, head, legSinkDistance);
        }
        else UpdateLegTransform(backLeg, 1, 1, legMidT, bufferWork, segParent, head, legSinkDistance);

        isReturningToStart = false;
        isMoving = false; // Đảm bảo reset isMoving sau khi quay lại
        PlayIdleAnim();
        if (debugLog) Debug.Log($"[WormController] {gameObject.name}: ĐÃ QUAY VỀ VỊ TRÍ BACKUP");
    }
    private void UpdateLegTransform(
    Transform leg,
    int idxA,
    int idxB,
    float tMid,
    SplinePoint[] ptsLocal,
    Transform segParent,
    Transform rootTransform,
    float sinkDistance)
    {
        if (!leg) return;
        int n = ptsLocal.Length;
        if (n < 2) return;

        // ✅ GIỮ nguyên position như hiện tại
        tMid = Mathf.Clamp01(tMid);
        Vector3 localMid = Vector3.Lerp(ptsLocal[idxA].position, ptsLocal[idxB].position, tMid);
        leg.position = segParent.TransformPoint(localMid);

        // ✅ Rotation giống head: lấy forward theo idxA/idxB + up của root
        Quaternion rot = GetRotationLikeHead(idxA, idxB, ptsLocal, segParent, rootTransform, reverseDirection: false);
        if (rot != Quaternion.identity)
            leg.rotation = rot;

        // Sink: nếu bạn muốn “chui vào thân” giống head thì dịch theo forward local
        if (Mathf.Abs(sinkDistance) > 0.0001f)
            leg.Translate(Vector3.forward * sinkDistance, Space.Self);
    }

    private Quaternion GetRotationLikeHead(
    int idxThis,
    int idxOther,
    SplinePoint[] ptsLocal,
    Transform segParent,
    Transform rootTransform,
    bool reverseDirection = false)
    {
        Vector3 worldThis = segParent.TransformPoint(ptsLocal[idxThis].position);
        Vector3 worldOther = segParent.TransformPoint(ptsLocal[idxOther].position);

        Vector3 d = reverseDirection ? (worldOther - worldThis) : (worldThis - worldOther);
        if (d.sqrMagnitude < 1e-8f) return Quaternion.identity;

        Vector3 forward = d.normalized;
        Vector3 up = rootTransform ? rootTransform.up : Vector3.up;

        // chống trường hợp forward gần song song up -> nhảy roll
        up = up - Vector3.Dot(up, forward) * forward;
        if (up.sqrMagnitude < 1e-6f)
        {
            Vector3 fb = rootTransform ? rootTransform.right : Vector3.right;
            up = fb - Vector3.Dot(fb, forward) * forward;
        }
        up.Normalize();

        return Quaternion.LookRotation(forward, up);
    }

    /// <summary>
    /// Fix normals cho spline points bằng Parallel Transport để tránh mesh bị xoắn
    /// Tối ưu: Tính toán trực tiếp từ positions, không cần Evaluate hoặc Rebuild
    /// </summary>
    private void FixNormalsParallelTransport(SplinePoint[] points, SplineComputer spline)
    {
        if (points == null || points.Length < 2) return;

        // Tính tangent từ positions (không cần Evaluate)
        Vector3 prevTangent;
        if (points.Length >= 2)
        {
            Vector3 dir = (points[1].position - points[0].position);
            if (dir.sqrMagnitude < 1e-8f)
            {
                prevTangent = Vector3.forward;
            }
            else
            {
                prevTangent = dir.normalized;
            }
        }
        else
        {
            prevTangent = Vector3.forward;
        }

        // Normal ban đầu: vuông góc với tangent và gần với Vector3.up nhất
        Vector3 normal = Vector3.up;
        normal = normal - Vector3.Dot(normal, prevTangent) * prevTangent;
        if (normal.sqrMagnitude < 1e-8f)
        {
            normal = Vector3.right - Vector3.Dot(Vector3.right, prevTangent) * prevTangent;
        }
        if (normal.sqrMagnitude < 1e-8f)
        {
            normal = Vector3.forward - Vector3.Dot(Vector3.forward, prevTangent) * prevTangent;
        }
        normal.Normalize();

        points[0].normal = normal;

        // Fix normals cho các points còn lại bằng Parallel Transport
        for (int i = 1; i < points.Length; i++)
        {
            // Tính tangent từ positions (không cần Evaluate)
            Vector3 curTangent;
            if (i < points.Length - 1)
            {
                // Dùng điểm trước và sau để tính tangent mượt hơn
                Vector3 dir = (points[i + 1].position - points[i - 1].position);
                if (dir.sqrMagnitude < 1e-8f)
                {
                    dir = (points[i].position - points[i - 1].position);
                }
                if (dir.sqrMagnitude < 1e-8f)
                {
                    curTangent = prevTangent;
                }
                else
                {
                    curTangent = dir.normalized;
                }
            }
            else
            {
                // Point cuối: dùng điểm trước
                Vector3 dir = (points[i].position - points[i - 1].position);
                if (dir.sqrMagnitude < 1e-8f)
                {
                    curTangent = prevTangent;
                }
                else
                {
                    curTangent = dir.normalized;
                }
            }

            // Xoay normal theo sự thay đổi của tangent (Parallel Transport)
            if (Vector3.Dot(prevTangent, curTangent) < 0.9999f) // Tránh quay khi tangent gần như không đổi
            {
                Quaternion q = Quaternion.FromToRotation(prevTangent, curTangent);
                normal = (q * normal).normalized;
            }

            // Đảm bảo normal vẫn ⟂ tangent (re-orthogonalize)
            normal = normal - Vector3.Dot(normal, curTangent) * curTangent;
            if (normal.sqrMagnitude < 1e-8f)
            {
                normal = Vector3.up - Vector3.Dot(Vector3.up, curTangent) * curTangent;
            }
            if (normal.sqrMagnitude < 1e-8f)
            {
                normal = Vector3.right - Vector3.Dot(Vector3.right, curTangent) * curTangent;
            }
            normal.Normalize();

            points[i].normal = normal;
            prevTangent = curTangent;
        }
    }

    public void PlayLegAnim()
    {
        frontLegAnim.Play("Armature|run", 0, 0f);

        backLegAnim.Play("Armature|run", 0, 0f);
    }
    public void PlayIdleAnim()
    {
        frontLegAnim.Play("Armature|idle", 0, 0f);

        backLegAnim.Play("Armature|idle", 0, 0f);
    }
    public void SetRotateSuspend(bool suspend)
    {
        if (backLegAnim) backLegAnim.enabled = !suspend;
        if (frontLegAnim) frontLegAnim.enabled = !suspend;  
        if (faceAnim) faceAnim.enabled = !suspend;

    }
    /// <summary>
    /// Kiểm tra mesh có bị xoắn hay không bằng cách kiểm tra sự thay đổi đột ngột của normals
    /// Tối ưu: Return sớm khi phát hiện xoắn để tiết kiệm performance
    /// </summary>
    /// <returns>True nếu phát hiện mesh bị xoắn, False nếu không</returns>
    public bool CheckMeshTwist(SplinePoint[] points, SplineComputer spline)
    {
        if (points == null || points.Length < 2) return false;

        float maxTwistAngle = 0f;
        int twistPointIndex = -1;
        float maxDot = 0f;
        int maxDotIndex = -1;

        // Kiểm tra góc quay giữa các normals liên tiếp (return sớm nếu phát hiện)
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 prevNormal = points[i - 1].normal;
            Vector3 curNormal = points[i].normal;

            // Tính góc quay giữa 2 normals
            float angle = Vector3.Angle(prevNormal, curNormal);

            // Nếu góc quay quá lớn → phát hiện xoắn, return ngay (tối ưu)
            if (angle > twistThreshold)
            {
                if (checkMeshTwist && debugLog)
                {
                    Debug.LogError($"[WormController] {gameObject.name}: PHÁT HIỆN MESH BỊ XOẮN! " +
                        $"Góc quay: {angle:F1}° (threshold: {twistThreshold}°) tại point {i}");
                }
                return true; // Return sớm để tiết kiệm performance
            }

            // Track góc quay lớn nhất để log (nếu cần)
            if (angle > maxTwistAngle)
            {
                maxTwistAngle = angle;
                twistPointIndex = i;
            }
        }

        // Kiểm tra thêm: Normal có vuông góc với tangent không? (return sớm nếu phát hiện)
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 tangent;
            if (i < points.Length - 1)
            {
                tangent = (points[i + 1].position - points[i].position).normalized;
            }
            else
            {
                tangent = (points[i].position - points[i - 1].position).normalized;
            }

            Vector3 normal = points[i].normal;
            float dot = Mathf.Abs(Vector3.Dot(normal, tangent));

            // Track dot lớn nhất để log
            if (dot > maxDot)
            {
                maxDot = dot;
                maxDotIndex = i;
            }

            // Normal phải vuông góc với tangent (dot ≈ 0)
            // Nếu dot > 0.1 → normal không vuông góc → phát hiện xoắn
            if (dot > 0.1f)
            {
                if (checkMeshTwist && debugLog)
                {
                    Debug.LogError($"[WormController] {gameObject.name}: PHÁT HIỆN MESH BỊ XOẮN! " +
                        $"Normal không vuông góc với tangent (dot: {dot:F3}, threshold: 0.1) tại point {i}");
                }
                return true; // Return sớm để tiết kiệm performance
            }
        }

        // Log thông tin chi tiết nếu bật debug (ngay cả khi không phát hiện xoắn)
        if (checkMeshTwist && debugLog)
        {
            Debug.Log($"[WormController] {gameObject.name}: CheckMeshTwist - " +
                $"Max angle: {maxTwistAngle:F1}° tại point {twistPointIndex}, " +
                $"Max dot: {maxDot:F3} tại point {maxDotIndex}, " +
                $"Threshold: {twistThreshold}°");
        }

        // Không phát hiện xoắn
        return false;
    }

    /// <summary>
    /// Kiểm tra mesh bị xoắn từ statePoints hiện tại (dùng để debug trong Inspector)
    /// </summary>
    [ContextMenu("Check Mesh Twist")]
    public void CheckMeshTwistCurrent()
    {
        if (statePoints == null || statePoints.Length < 2)
        {
            Debug.LogWarning($"[WormController] {gameObject.name}: Không có statePoints để kiểm tra!");
            return;
        }

        // Tạm thời bật debug để xem chi tiết
        bool originalCheckTwist = checkMeshTwist;
        bool originalDebugLog = debugLog;
        checkMeshTwist = true;
        debugLog = true;

        bool hasTwist = CheckMeshTwist(statePoints, spline);
        
        // Khôi phục
        checkMeshTwist = originalCheckTwist;
        debugLog = originalDebugLog;

        if (hasTwist)
        {
            Debug.LogError($"[WormController] {gameObject.name}: MESH ĐANG BỊ XOẮN!");
        }
        else
        {
            Debug.Log($"[WormController] {gameObject.name}: Mesh không bị xoắn ✓");
        }
    }

    /// <summary>
    /// Log thông tin chi tiết về normals (dùng để debug)
    /// </summary>
    [ContextMenu("Log Normals Info")]
    public void LogNormalsInfo()
    {
        if (statePoints == null || statePoints.Length < 2)
        {
            Debug.LogWarning($"[WormController] {gameObject.name}: Không có statePoints!");
            return;
        }

        Debug.Log($"[WormController] {gameObject.name}: === NORMALS INFO ===");
        for (int i = 0; i < statePoints.Length; i++)
        {
            Vector3 normal = statePoints[i].normal;
            Vector3 tangent;
            if (i < statePoints.Length - 1)
            {
                tangent = (statePoints[i + 1].position - statePoints[i].position).normalized;
            }
            else
            {
                tangent = (statePoints[i].position - statePoints[i - 1].position).normalized;
            }
            
            float dot = Mathf.Abs(Vector3.Dot(normal, tangent));
            float angle = i > 0 ? Vector3.Angle(statePoints[i - 1].normal, normal) : 0f;
            
            Debug.Log($"Point {i}: normal={normal}, tangent={tangent}, dot={dot:F3}, angle={angle:F1}°");
        }
    }
}

