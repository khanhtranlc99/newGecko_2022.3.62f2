using Dreamteck.Splines;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WormTest : MonoBehaviour
{
   public SplineComputer spline;
   public List<GameObject> wormsPost;
    private SplinePoint[] bufferFrom;
    public GameObject head; 
    public GameObject trail;
    public GameObject leg;
    [SerializeField] float bodyRadius = 0.25f;   // bán kính thân sâu
    [SerializeField] float heightOffset = 2f;    // nếu pivot chân không ở giữa thì chỉnh thêm
    [SerializeField] Vector3 legEulerOffset;

    public List<Material> lsMaterials;
    
    [Button]
   public void Spawn()
   {


        SplineComputer spline = GetComponent<SplineComputer>();
        //spline.space = SplineComputer.Space.Local;


        //int count = wormsPost.Count;
        //SplinePoint[] pts = new SplinePoint[count];

        //for (int i = 0; i < count; i++)
        //{
        //    Vector3 cell = wormsPost[i].transform.position;

        //    // Vector3 worldPos = originWorld + ((Vector3)cell - gridCenter) * cellSize;

        //    SplinePoint p = new SplinePoint();
        //    p.position = cell;
        //    p.size = 1f;
        //    p.color = Color.white;

        //    pts[i] = p;
        //}

        //spline.SetPoints(pts, SplineComputer.Space.Local);

        //spline.RebuildImmediate();

        //// FIX: Rebuild SplineMesh sau khi SplineComputer đã có points mới
        //var splineMesh = GetComponent<Dreamteck.Splines.SplineMesh>();
        //if (splineMesh != null)
        //{
        //    splineMesh.RebuildImmediate();
        //}

        bufferFrom = spline.GetPoints(SplineComputer.Space.Local);
        var tempHead = Instantiate(head);
        tempHead.transform.position = bufferFrom[0].position;
        tempHead.transform.parent = this.transform;
        // Xoay đầu theo hướng từ point 0 đến point 1
        

        var tempTrail = Instantiate(trail);
        tempTrail.transform.position = bufferFrom[bufferFrom.Length - 1].position;
        tempTrail.transform.parent = this.transform;
        // Xoay đuôi theo hướng từ point gần cuối về point cuối (ngược lại để mặt rỗng quay vào trong)
        if (bufferFrom.Length > 1)
        {
            int lastIndex = bufferFrom.Length - 1;
            Vector3 trailDirection = bufferFrom[lastIndex - 1].position - bufferFrom[lastIndex].position;
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

        Vector3 tangentB = -sBack.forward;
        Vector3 upB = sBack.up.normalized;

        Vector3 frontPos = sFront.position;
        Vector3 backPos = sBack.position;

        // Local +Z của leg sẽ // với dirFront / dirBack
        Quaternion baseRotFront = Quaternion.LookRotation(tangentF, upF);
        Quaternion baseRotBack = Quaternion.LookRotation(tangentB, upB);
        tempHead.transform.rotation = Quaternion.LookRotation(tangentF, upF);
            

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!spline) spline = GetComponent<SplineComputer>();
        wormsPost = new List<GameObject>();
        var allTransforms = transform.GetComponentsInChildren<Transform>();
        for (int i = 1; i < allTransforms.Length; i++) // Bỏ qua phần tử đầu tiên (chính nó)
        {
            wormsPost.Add(allTransforms[i].gameObject);
        }
    }
#endif

    [Button]
   public void Delete()
   {
        for(int i = wormsPost.Count - 1; i >= 0; i--)
        {
            var obj = wormsPost[i];
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
            wormsPost.Remove(obj);
        }





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
