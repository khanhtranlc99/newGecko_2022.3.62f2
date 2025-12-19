using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;

public class GenerateLevel : MonoBehaviour
{
    [Header("Grid Settings")]
    public int sizeX = 5;
    public int sizeY = 5;
    public int sizeZ = 5;

    [Header("Worm Settings")]
    [Tooltip("Số worm tối đa trong 1 level")]
    public int maxWormCount = 10;

    [Tooltip("Số cell tối đa cho 1 worm (độ dài tối đa)")]
    public int maxCellsPerWorm = 20;

    [Tooltip("Số cell tối thiểu để 1 worm được chấp nhận")]
    public int minCellsPerWorm = 3;

    public string fileName = "worm_level_001";

    // ================== JSON STRUCT ==================

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

    // ================== ENTRY ==================

    [Button]
    [ContextMenu("Generate Random Level & Save JSON")]
    public void GenerateAndSave()
    {
        LevelData levelData = GenerateMultiWormLevel();

        if (levelData == null || levelData.worms.Count == 0)
        {
            Debug.LogWarning("[GenerateLevel] Không tạo được worm nào.");
            return;
        }

        string json = JsonUtility.ToJson(levelData, true);

#if UNITY_EDITOR
        string dir = Path.Combine(Application.dataPath, "Resources/Levels");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string fullPath = Path.Combine(dir, fileName + ".json");
#else
        string fullPath = Path.Combine(Application.persistentDataPath, fileName + ".json");
#endif

        File.WriteAllText(fullPath, json);
        Debug.Log($"[GenerateLevel] Đã generate level: {levelData.worms.Count} worm, lưu tại: {fullPath}");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // ================== CORE: MULTI-WORM BFS ==================

    private LevelData GenerateMultiWormLevel()
    {
        LevelData level = new LevelData();

        // ma trận cell đã bị chiếm bởi worm trước
        bool[,,] occupied = new bool[sizeX, sizeY, sizeZ];

        int wormsCreated = 0;

        // Vòng lặp chính: còn cell free là còn thử tạo worm
        while (true)
        {
            // 1) tìm 1 cell random chưa bị chiếm làm start
            if (!TryGetRandomFreeCell(occupied, out Vector3Int start))
            {
                // không còn cell trống nào -> dừng hẳn
                break;
            }

            // 2) Random walk cho worm mới, chỉ đi trên các cell chưa occupied
            WormPath worm = GenerateOneWormDFS(start, occupied);

            // 3) check độ dài tối thiểu
            if (worm.cells.Count >= minCellsPerWorm)
            {
                // chấp nhận worm này, đánh dấu occupied
                foreach (var c in worm.cells)
                {
                    occupied[c.x, c.y, c.z] = true;
                }

                level.worms.Add(worm);
                wormsCreated++;
            }
            else
            {
                // worm quá ngắn, bỏ qua, nhưng vẫn đánh dấu cell start là occupied
                // để vòng sau chọn cell khác
                occupied[start.x, start.y, start.z] = true;
            }
        }

        return level;
    }


    /// <summary>
    /// BFS 3D từ 1 cell start, chỉ đi 6 hướng (không chéo),
    /// không đi vào cell đã occupied, và giới hạn maxCellsPerWorm.
    /// </summary>
    private WormPath GenerateOneWormDFS(Vector3Int start, bool[,,] occupied)
    {
        WormPath worm = new WormPath();

        // visited của worm hiện tại
        bool[,,] visited = new bool[sizeX, sizeY, sizeZ];

        // 6 hướng di chuyển
        Vector3Int[] dirs = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0,-1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0,-1)
        };

        System.Random rng = new System.Random();

        // cell bắt đầu
        Vector3Int current = start;

        // đánh dấu và add vào worm
        worm.cells.Add(current);
        visited[current.x, current.y, current.z] = true;

        // Random walk cho đến khi hết đường hoặc đạt giới hạn
        while (worm.cells.Count < maxCellsPerWorm)
        {
            List<Vector3Int> candidates = new List<Vector3Int>();

            // tìm hàng xóm hợp lệ
            foreach (var d in dirs)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;
                int nz = current.z + d.z;

                // out of bounds
                if (nx < 0 || nx >= sizeX ||
                    ny < 0 || ny >= sizeY ||
                    nz < 0 || nz >= sizeZ)
                    continue;

                // cell đã thuộc worm khác
                if (occupied[nx, ny, nz]) continue;

                // cell đã visited trong worm này
                if (visited[nx, ny, nz]) continue;

                candidates.Add(new Vector3Int(nx, ny, nz));
            }

            // không còn hướng nào → stop
            if (candidates.Count == 0)
                break;

            // chọn random 1 cell từ ứng viên
            Vector3Int next = candidates[rng.Next(candidates.Count)];

            // cập nhật current → next
            current = next;
            visited[current.x, current.y, current.z] = true;
            worm.cells.Add(current);
        }

        return worm;
    }

    /// <summary>
    /// Tìm 1 cell free ngẫu nhiên (chưa occupied). 
    /// Nếu không còn cell free thì trả false.
    /// </summary>
    private bool TryGetRandomFreeCell(bool[,,] occupied, out Vector3Int cell)
    {
        List<Vector3Int> freeCells = new List<Vector3Int>();
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    if (!occupied[x, y, z])
                    {
                        freeCells.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        if (freeCells.Count == 0)
        {
            cell = default;
            return false;
        }

        int idx = Random.Range(0, freeCells.Count);
        cell = freeCells[idx];
        return true;
    }

    // ================== UTILS ==================

    private static void ShuffleArray<T>(T[] array, System.Random rng)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}
