using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System.Collections;
using JetBrains.Annotations;
public class WormTest : MonoBehaviour
{
   public SplineComputer spline;
   public List<GameObject> wormsPost;
      public GameObject head; 
    public GameObject trail;
    public GameObject leg;
    [SerializeField] float bodyRadius = 0.25f;   // bán kính thân sâu
    [SerializeField] float heightOffset = 2f;    // nếu pivot chân không ở giữa thì chỉnh thêm
    [SerializeField] Vector3 legEulerOffset;

    public List<Material> lsMaterials;
    
    [Button("Spawn Single")]
   public void Spawn()
   {


      SplineComputer spline = GetComponent<SplineComputer>();
      spline.space = SplineComputer.Space.World;


        int count = wormsPost.Count;
        SplinePoint[] pts = new SplinePoint[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 cell = wormsPost[i].transform.position;

            // Vector3 worldPos = originWorld + ((Vector3)cell - gridCenter) * cellSize;

            SplinePoint p = new SplinePoint();
            p.position = cell;
            p.size = 1f;
            p.color = Color.white;
      
            pts[i] = p;
        }

        spline.SetPoints(pts, SplineComputer.Space.World);

        spline.RebuildImmediate();
        
        // FIX: Rebuild SplineMesh sau khi SplineComputer đã có points mới
        var splineMesh = GetComponent<Dreamteck.Splines.SplineMesh>();
        if (splineMesh != null)
        {
            splineMesh.RebuildImmediate();
        }


        var tempHead = Instantiate(head);
        tempHead.transform.position = wormsPost[0].transform.position;  
        tempHead.transform.parent = this.transform;
        // Xoay đầu theo hướng từ point 0 đến point 1
        if (wormsPost.Count > 1)
        {
            Vector3 headDirection = wormsPost[0].transform.position - wormsPost[1].transform.position;
            if (headDirection.sqrMagnitude > 0.0001f)
            {
                tempHead.transform.rotation = Quaternion.LookRotation(headDirection.normalized);
            }
        }

        var tempTrail = Instantiate(trail);
        tempTrail.transform.position = wormsPost[wormsPost.Count - 1].transform.position;
                  tempTrail.transform.parent = this.transform;
        // Xoay đuôi theo hướng từ point gần cuối về point cuối (ngược lại để mặt rỗng quay vào trong)
        if (wormsPost.Count > 1)
        {
            int lastIndex = wormsPost.Count - 1;
            Vector3 trailDirection = wormsPost[lastIndex - 1].transform.position - wormsPost[lastIndex].transform.position;
            if (trailDirection.sqrMagnitude > 0.0001f)
            {
                tempTrail.transform.rotation = Quaternion.LookRotation(trailDirection.normalized);
            }
        }
        double t0 = spline.GetPointPercent(0); // point 0 (đầu)
        double t1 = spline.GetPointPercent(1); // point 1

        // Trung điểm trên spline giữa 0-1 và 1-2
        double tFront = t0;   // hoặc DMath.Lerp(t0, t1, 0.5)
        double tBack = t1;   // hoặc DMath.Lerp(t1, t2, 0.5)

        // Lấy sample (vị trí + frame) trên spline
        SplineSample sFront = spline.Evaluate(tFront);
        SplineSample sBack = spline.Evaluate(tBack);

        // Frame của spline
        Vector3 tangentF = -sFront.forward;       // hướng “đầu → đuôi” của worm
        Vector3 upF = sFront.up.normalized;
        Vector3 rightF = sFront.right.normalized;   // nếu Dreamteck có sFront.right, dùng luôn

        Vector3 tangentB = -sBack.forward;
        Vector3 upB = sBack.up.normalized;
        Vector3 rightB = sBack.right.normalized;

        // Vị trí ở mép thân (bên phải thân)
        Vector3 sideOffsetF = rightF * bodyRadius;
        Vector3 sideOffsetB = rightB * bodyRadius;

        // chỉnh lên/xuống giữa thân nếu pivot chân bị lệch
        Vector3 heightOffF = upF * heightOffset;
        Vector3 heightOffB = upB * heightOffset;

        Vector3 frontPos = sFront.position + sideOffsetF + heightOffF;
        Vector3 backPos = sBack.position + sideOffsetB + heightOffB;

        // Local +Z của leg sẽ // với dirFront / dirBack
        Quaternion baseRotFront = Quaternion.LookRotation(tangentF, upF);
        Quaternion baseRotBack = Quaternion.LookRotation(tangentB, upB);

        // offset thêm để chỉnh cho đúng hướng chân (set trong inspector)
        Quaternion legRotOffset = Quaternion.Euler(legEulerOffset);

        Quaternion rotFront = baseRotFront * legRotOffset;
        Quaternion rotBack = baseRotBack * legRotOffset;

        // Spawn
        var legFront = Instantiate(leg, frontPos, rotFront, this.transform);
        var legBack = Instantiate(leg, backPos, rotBack, this.transform);
        

        var material = lsMaterials[Random.Range(0, lsMaterials.Count)];
      this.GetComponent<Renderer>().sharedMaterial = material;

        // Gán material cho trail
        tempHead.GetComponentInChildren<Renderer>().sharedMaterial = material;
      tempTrail.GetComponentInChildren<Renderer>().sharedMaterial = material;
        legFront.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = material;
        legBack.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = material;

        this.GetComponent<WormController>().head = tempHead.transform;
    this.GetComponent<WormController>().tail = tempTrail.transform;
    this.GetComponent<WormController>().root = this.transform;
    this.GetComponent<WormController>().frontLeg = legFront.transform;
    this.GetComponent<WormController>().backLeg = legBack.transform;
        // Tự động thêm MeshCollider nếu chưa có
        if (GetComponent<MeshCollider>() == null)
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            // Lấy mesh từ MeshFilter nếu có
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            }
        }
       
    }

    [Button("Spawn Batch (Optimized)")]
    public void SpawnBatch()
    {
        // Tối ưu: Setup trước khi spawn
        SplineComputer spline = GetComponent<SplineComputer>();
        spline.space = SplineComputer.Space.World;
        
        // Tắt rebuild on awake tạm thời để tránh rebuild không cần thiết
        var splineMesh = GetComponent<Dreamteck.Splines.SplineMesh>();
        bool originalBuildOnAwake = false;
        if (splineMesh != null)
        {
            originalBuildOnAwake = splineMesh.buildOnAwake;
            splineMesh.buildOnAwake = false; // Tắt tạm thời
        }
        
        // Setup spline points
        int count = wormsPost.Count;
        SplinePoint[] pts = new SplinePoint[count];
        
        for (int i = 0; i < count; i++)
        {
            Vector3 cell = wormsPost[i].transform.position;
            SplinePoint p = new SplinePoint();
            p.position = cell;
            p.size = 1f;
            p.color = Color.white;
            
            // Tính normal
            if (count >= 3)
            {
                Vector3 side1, side2;
                if (i == 0)
                {
                    side1 = cell - wormsPost[1].transform.position;
                    side2 = cell - wormsPost[2].transform.position;
                }
                else if (i == count - 1)
                {
                    side1 = cell - wormsPost[count - 2].transform.position;
                    side2 = cell - wormsPost[count - 3].transform.position;
                }
                else
                {
                    side1 = cell - wormsPost[i + 1].transform.position;
                    side2 = cell - wormsPost[i - 1].transform.position;
                }
                
                Vector3 normal = Vector3.Cross(side1.normalized, side2.normalized).normalized;
                if (normal.sqrMagnitude < 0.0001f)
                {
                    normal = Vector3.up;
                }
                p.normal = normal;
            }
            else
            {
                p.normal = Vector3.up;
            }
            
            pts[i] = p;
        }
        
        spline.SetPoints(pts, SplineComputer.Space.World);
        spline.RebuildImmediate();
        
        // Rebuild SplineMesh
        if (splineMesh != null)
        {
            splineMesh.RebuildImmediate();
        }
        
        // Setup head, trail, legs (giống Spawn() nhưng tối ưu hơn)
        var tempHead = Instantiate(head);
        tempHead.transform.position = wormsPost[0].transform.position;
        tempHead.transform.parent = this.transform;
        if (wormsPost.Count > 1)
        {
            Vector3 headDirection = wormsPost[0].transform.position - wormsPost[1].transform.position;
            if (headDirection.sqrMagnitude > 0.0001f)
            {
                tempHead.transform.rotation = Quaternion.LookRotation(headDirection.normalized);
            }
        }
        
        var tempTrail = Instantiate(trail);
        tempTrail.transform.position = wormsPost[wormsPost.Count - 1].transform.position;
        tempTrail.transform.parent = this.transform;
        if (wormsPost.Count > 1)
        {
            int lastIndex = wormsPost.Count - 1;
            Vector3 trailDirection = wormsPost[lastIndex - 1].transform.position - wormsPost[lastIndex].transform.position;
            if (trailDirection.sqrMagnitude > 0.0001f)
            {
                tempTrail.transform.rotation = Quaternion.LookRotation(trailDirection.normalized);
            }
        }
        
        double t0 = spline.GetPointPercent(0);
        double t1 = spline.GetPointPercent(1);
        double tFront = t0;
        double tBack = t1;
        
        SplineSample sFront = spline.Evaluate(tFront);
        SplineSample sBack = spline.Evaluate(tBack);
        
        Vector3 tangentF = -sFront.forward;
        Vector3 upF = sFront.up.normalized;
        Vector3 rightF = sFront.right.normalized;
        
        Vector3 tangentB = -sBack.forward;
        Vector3 upB = sBack.up.normalized;
        Vector3 rightB = sBack.right.normalized;
        
        Vector3 sideOffsetF = rightF * bodyRadius;
        Vector3 sideOffsetB = rightB * bodyRadius;
        Vector3 heightOffF = upF * heightOffset;
        Vector3 heightOffB = upB * heightOffset;
        
        Vector3 frontPos = sFront.position + sideOffsetF + heightOffF;
        Vector3 backPos = sBack.position + sideOffsetB + heightOffB;
        
        Quaternion baseRotFront = Quaternion.LookRotation(tangentF, upF);
        Quaternion baseRotBack = Quaternion.LookRotation(tangentB, upB);
        Quaternion legRotOffset = Quaternion.Euler(legEulerOffset);
        Quaternion rotFront = baseRotFront * legRotOffset;
        Quaternion rotBack = baseRotBack * legRotOffset;
        
        var legFront = Instantiate(leg, frontPos, rotFront, this.transform);
        var legBack = Instantiate(leg, backPos, rotBack, this.transform);
        
        var material = lsMaterials[Random.Range(0, lsMaterials.Count)];
        this.GetComponent<Renderer>().sharedMaterial = material;
        tempHead.GetComponentInChildren<Renderer>().sharedMaterial = material;
        tempTrail.GetComponentInChildren<Renderer>().sharedMaterial = material;
        legFront.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = material;
        legBack.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = material;
        
        this.GetComponent<WormController>().head = tempHead.transform;
        this.GetComponent<WormController>().tail = tempTrail.transform;
        this.GetComponent<WormController>().root = this.transform;
        this.GetComponent<WormController>().frontLeg = legFront.transform;
        this.GetComponent<WormController>().backLeg = legBack.transform;
        
        // Defer MeshCollider update - chỉ update sau khi spawn xong
        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        // Tạm thời disable collider để tránh update nặng
        meshCollider.enabled = false;
        
        // Khôi phục buildOnAwake
        if (splineMesh != null)
        {
            splineMesh.buildOnAwake = originalBuildOnAwake;
        }
        
        // Update collider sau 1 frame (dùng coroutine)
        StartCoroutine(UpdateColliderDelayed(meshCollider));
    }
    
    private IEnumerator UpdateColliderDelayed(MeshCollider meshCollider)
    {
        yield return null; // Đợi 1 frame
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
        meshCollider.enabled = true;
    }

    [Button]
   public void Move()
   {
        





   }

    //  IEnumerator MoveOneStep(System.Action onDone = null)
    // {
    //     if (!spline) yield break;

    //     // 1) Lấy spline points trong LOCAL của worm
    //     SplinePoint[] current = spline.GetPoints(SplineComputer.Space.Local);
    //     int n = current.Length;
    //     if (n == 0) yield break;

    //     if (IsBlockedAhead())
    //     {
    //         autoMove = false;
    //         yield break;
    //     }

    //     EnsureBuffers(n);

    //     // copy current -> bufferFrom
    //     System.Array.Copy(current, bufferFrom, n);

    //     var from = bufferFrom;
    //     var to = bufferTo;
    //     var work = bufferWork;

    //     Transform segParent = transform;

    //     // 2) Tính headTargetLocal dựa trên moveDirGrid (hướng grid trong root)
    //     Vector3 headCurrentLocal = from[0].position;

    //     // dirWorld = hướng WORLD tương ứng với moveDirGrid
    //     Vector3 dirWorld = root.TransformDirection((Vector3)moveDirGrid);

    //     // Đưa về LOCAL của worm
    //     Vector3 dirLocal = segParent.InverseTransformDirection(dirWorld).normalized;
    //     if (dirLocal.sqrMagnitude < 0.0001f) yield break;

    //     Vector3 headTargetLocal = headCurrentLocal + dirLocal * cellSize;

    //     // 3) Xây mảng to: headTarget + các point chạy về vị trí point trước
    //     to[0] = from[0];
    //     to[0].position = headTargetLocal;

    //     for (int i = 1; i < n; i++)
    //     {
    //         to[i] = from[i];
    //         to[i].position = from[i - 1].position;
    //     }

    //     // 4) Lerp LOCAL from -> to trong stepDuration
    //     float t = 0f;
    //     float dur = Mathf.Max(stepDuration, 0.0001f);

    //     while (t < 1f)
    //     {
    //         t += Time.deltaTime / dur;
    //         float a = Mathf.Clamp01(t);

    //         for (int i = 0; i < n; i++)
    //         {
    //             work[i] = from[i];
    //             work[i].position = Vector3.Lerp(from[i].position, to[i].position, a);
    //         }

    //         spline.SetPoints(work, SplineComputer.Space.Local);

    //         // Cập nhật head theo point 0
    //         if (head)
    //         {
    //             Vector3 worldP0 = segParent.TransformPoint(work[0].position);
    //             head.position = worldP0;

    //             if (n > 1)
    //             {
    //                 Vector3 worldP1 = segParent.TransformPoint(work[1].position);
    //                 Vector3 d = worldP0 - worldP1;
    //                 if (d.sqrMagnitude > 0.00001f)
    //                 {
    //                     Vector3 up = root ? root.up : Vector3.up;
    //                     head.rotation = Quaternion.LookRotation(d.normalized, up);
    //                 }
    //             }
    //         }

    //         yield return null;
    //     }

    //     // 5) Kết thúc step: set to
    //     spline.SetPoints(to, SplineComputer.Space.Local);

    //     if (head)
    //     {
    //         Vector3 worldP0 = segParent.TransformPoint(to[0].position);
    //         head.position = worldP0;

    //         if (n > 1)
    //         {
    //             Vector3 worldP1 = segParent.TransformPoint(to[1].position);
    //             Vector3 d = worldP0 - worldP1;
    //             if (d.sqrMagnitude > 0.00001f)
    //             {
    //                 Vector3 up = root ? root.up : Vector3.up;
    //                 head.rotation = Quaternion.LookRotation(d.normalized, up);
    //             }
    //         }
    //     }

    //     onDone?.Invoke();
    // }


    //     /// <summary>
    // /// Kiểm tra phía trước đầu sâu có vật cản (worm khác, tường, ...) hay không.
    // /// </summary>
    // private bool IsBlockedAhead()
    // {
    //     if (!head) return false;
    //     if (!root) root = transform;

    //     // Hướng world hiện tại dựa trên moveDirGrid
    //     Vector3 dirWorld = root.TransformDirection((Vector3)moveDirGrid).normalized;
    //     if (dirWorld.sqrMagnitude < 0.0001f) return false;

    //     Vector3 origin = head.position;

    //     // SphereCast ra phía trước
    //     if (Physics.SphereCast(origin, headCheckRadius, dirWorld, out RaycastHit hit, cellSize))
    //     {
    //         // Bỏ qua collider thuộc chính con worm này
    //         if (!hit.collider.transform.IsChildOf(this.transform))
    //         {
    //             // Va vào worm/obj khác
    //             hasCollided = true;
    //             return true;
    //         }

    //         // Đụng phải chính mình -> bỏ qua, không coi là blocked
    //         return false;
    //     }

    //     return false;
    // }

    // /// <summary>
    // /// Lấy hướng grid từ rotation hiện tại của đầu sâu (theo local của root).
    // /// </summary>
    // private Vector3Int GetDirByRotation()
    // {
    //     if (!head) return Vector3Int.forward;
    //     if (!root) root = transform;

    //     // Vector3.forward trong local của head -> WORLD
    //     Vector3 dirWorld = head.transform.TransformDirection(Vector3.forward);

    //     // WORLD -> LOCAL của root (container)
    //     Vector3 dirLocal = root.InverseTransformDirection(dirWorld);
    //     dirLocal.Normalize();

    //     float ax = Mathf.Abs(dirLocal.x);
    //     float ay = Mathf.Abs(dirLocal.y);
    //     float az = Mathf.Abs(dirLocal.z);

    //     if (ax >= ay && ax >= az)
    //         return dirLocal.x > 0 ? Vector3Int.right : Vector3Int.left;

    //     if (ay >= az)
    //         return dirLocal.y > 0 ? Vector3Int.up : Vector3Int.down;

    //     return dirLocal.z > 0 ? Vector3Int.forward : Vector3Int.back;
    // }

}
