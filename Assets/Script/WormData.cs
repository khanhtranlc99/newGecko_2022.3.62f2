using UnityEngine;

/// <summary>
/// Dữ liệu worm đã parse từ text level (mảng points 3-10)
/// </summary>
[System.Serializable]
public class WormData
{
    public Vector3[] points;  // Mảng points 3-10
    public int materialIndex = -1;  // -1 = random

    public WormData(Vector3[] points, int materialIndex = -1)
    {
        this.points = points;
        this.materialIndex = materialIndex;
    }

    /// <summary>
    /// Kiểm tra dữ liệu hợp lệ (3-10 points)
    /// </summary>
    public bool IsValid()
    {
        return points != null && points.Length >= 3 && points.Length <= 10;
    }
}

