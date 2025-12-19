using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

/// <summary>
/// Đọc level từ JSON (danh sách worms, mỗi worm là list Vector3Int cell)
/// và build SplineComputer cho từng con sâu.
/// </summary>
public class WormPathLoader : MonoBehaviour
{
    [Header("JSON")]
    [Tooltip("Tên file JSON trong Resources (không kèm .json). Ví dụ: worm_level_001")]
    public string jsonResourceName = "worm_level_001";

    [Header("Grid -> World")]
    [Tooltip("Kích thước 1 ô grid trong world (phải trùng với GenerateLevel.cellSize).")]
    public float cellSize = 1f;

    [Tooltip("Điểm gốc để đặt nguyên cả level (center worms quanh vị trí này).")]
    public Vector3 originWorld = Vector3.zero;

    [Header("Spline Prefab & Parent")]
    [Tooltip("Prefab chứa SplineComputer mà mỗi worm sẽ instance ra.")]
    public GameObject splinePrefab;

    [Tooltip("Parent để chứa tất cả spline của worms. Nếu trống sẽ dùng chính GameObject này.")]
    public Transform splineParent;


    [System.Serializable]
    public class WormPath
    {
        public List<Vector3Int> cells = new List<Vector3Int>();
    }

    [System.Serializable]
    public class LevelData
    {
        public List<WormPath> worms = new List<WormPath>();
    }


    void Start()
    {
        LoadLevel();
    }

    [ContextMenu("Load Level Now")]
    public void LoadLevel()
    {
        if (splinePrefab == null)
        {
            Debug.LogError("[WormPathLoader] Chưa gán splinePrefab!");
            return;
        }

        if (!splineParent) splineParent = transform;

        ClearChildren(splineParent);

        TextAsset jsonAsset = Resources.Load<TextAsset>("Levels/"+jsonResourceName);
        if (jsonAsset == null)
        {
            return;
        }

        LevelData level = JsonUtility.FromJson<LevelData>(jsonAsset.text);
        if (level == null || level.worms == null || level.worms.Count == 0)
        {
            Debug.LogWarning("[WormPathLoader] LevelData rỗng hoặc không có worm nào.");
            return;
        }

        bool hasAnyCell = false;
        Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        foreach (var worm in level.worms)
        {
            if (worm.cells == null) continue;

            foreach (var c in worm.cells)
            {
                hasAnyCell = true;

                if (c.x < min.x) min.x = c.x;
                if (c.y < min.y) min.y = c.y;
                if (c.z < min.z) min.z = c.z;

                if (c.x > max.x) max.x = c.x;
                if (c.y > max.y) max.y = c.y;
                if (c.z > max.z) max.z = c.z;
            }
        }

        if (!hasAnyCell)
        {
            Debug.LogWarning("[WormPathLoader] Không có cell nào trong worms.");
            return;
        }

        Vector3 gridCenter = ((Vector3)min + (Vector3)max) / 2f;

        for (int i = 0; i < level.worms.Count; i++)
        {
            WormPath worm = level.worms[i];
            if (worm == null || worm.cells == null || worm.cells.Count < 2)
                continue;

            CreateSplineForWorm(worm, i, gridCenter);
        }

        Debug.Log($"[WormPathLoader] Đã load level: {level.worms.Count} worm.");
    }


    private void CreateSplineForWorm(WormPath worm, int wormIndex, Vector3 gridCenter)
    {
        GameObject go = Instantiate(splinePrefab, splineParent);
        go.name = $"WormSpline_{wormIndex}";

        SplineComputer spline = go.GetComponent<SplineComputer>();
        if (!spline)
        {
            Debug.LogError("[WormPathLoader] Prefab không có SplineComputer!");
            return;
        }

        List<Vector3Int> cells = worm.cells;
        int count = cells.Count;
        if (count < 2) return;

        spline.space = SplineComputer.Space.World;

        SplinePoint[] pts = new SplinePoint[count];

        for (int i = 0; i < count; i++)
        {
            Vector3Int cell = cells[i];

            Vector3 worldPos = originWorld + ((Vector3)cell - gridCenter) * cellSize;

            SplinePoint p = new SplinePoint();
            p.position = worldPos;
            p.size = 1f;
            p.color = Color.white;

            pts[i] = p;
        }

        spline.SetPoints(pts, SplineComputer.Space.World);

        spline.RebuildImmediate();
    }

    /// <summary>
    /// Xóa hết con của 1 parent.
    /// </summary>
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }
}
