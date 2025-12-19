using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AdjustPolygonCollider : MonoBehaviour
{
    private SpriteRenderer spriteRendererA;
    private List<SpriteRenderer> spriteRenderersB = new List<SpriteRenderer>(); // Danh sách các SpriteRenderer B
    private PolygonCollider2D polygonCollider;

    [Button]
    void Handletest()
    {
        spriteRendererA = GetComponent<SpriteRenderer>(); // Lấy spriteRenderer của đối tượng chứa script này
        spriteRenderersB.Clear(); // Xóa danh sách các SpriteRenderer B để tránh lặp lại

        // Lấy tất cả các SpriteRenderer con của đối tượng chứa script này
        foreach (Transform child in transform)
        {
            SpriteRenderer childSpriteRenderer = child.GetComponent<SpriteRenderer>();
            if (childSpriteRenderer != null && childSpriteRenderer != spriteRendererA)
            {
                spriteRenderersB.Add(childSpriteRenderer); // Thêm SpriteRenderer con vào danh sách SpriteRenderer B
            }
        }

        polygonCollider = GetComponent<PolygonCollider2D>(); // Lấy PolygonCollider2D của đối tượng chứa script này

        // Lấy các điểm của SpriteRenderer A
        List<Vector2> pointsA = new List<Vector2>();
        spriteRendererA.sprite.GetPhysicsShape(0, pointsA);

        // Kết hợp các điểm của SpriteRenderer A và tất cả các SpriteRenderer B mà không tạo ra các đường giữa
        List<Vector2> combinedPoints = new List<Vector2>(pointsA);

        foreach (var spriteRendererB in spriteRenderersB)
        {
            combinedPoints.AddRange(CalculateCirclePoints(spriteRendererB));
        }

        // Đặt hình dạng PolygonCollider2D
        polygonCollider.points = combinedPoints.ToArray();
    }

    private List<Vector2> CalculateCirclePoints(SpriteRenderer spriteRenderer)
    {
        // Lấy bán kính của hình tròn
        float radius = spriteRenderer.bounds.size.x / 2f;

        // Số lượng điểm để xấp xỉ hình tròn
        int numPoints = 16;
        List<Vector2> points = new List<Vector2>();

        // Tính toán tọa độ trung tâm của SpriteRenderer B trong không gian của SpriteRenderer A
        Vector3 centerBLocal = spriteRenderer.bounds.center - spriteRendererA.bounds.center;

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * (2f * Mathf.PI / numPoints);
            float x = Mathf.Cos(angle) * radius + centerBLocal.x;
            float y = Mathf.Sin(angle) * radius + centerBLocal.y;
            points.Add(new Vector2(x, y));
        }

        return points;
    }
}