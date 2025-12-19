#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Dreamteck.Splines;

public static class DreamteckSplineNormalBatchFix
{
    // Bạn có thể đổi hướng "Look Up" ở đây nếu game dùng trục khác
    private static readonly Vector3 LookUpWorld = Vector3.up;
    private static readonly Vector3 FallbackAxis = Vector3.forward;

    [MenuItem("Tools/Worm/Fix Dreamteck Normals (Perpendicular + LookUp) - In Scene")]
    public static void FixAllInOpenScenes()
    {
        int fixedCount = 0;
        var splines = Object.FindObjectsOfType<SplineComputer>(true);

        Undo.RecordObjects(splines, "Fix Dreamteck Spline Normals");

        foreach (var spline in splines)
        {
            if (!spline) continue;
            FixNormalsParallelTransport(spline);
            EditorUtility.SetDirty(spline);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Dreamteck] Fixed normals for {fixedCount}/{splines.Length} SplineComputer(s).");
    }

    [MenuItem("Tools/Worm/Fix Dreamteck Normals (Perpendicular + LookUp) - Selection Only")]
    public static void FixSelectionOnly()
    {
        var selectedRoots = Selection.gameObjects;
        int fixedCount = 0;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Fix Dreamteck Spline Normals (Selection)");

        foreach (var root in selectedRoots)
        {
            if (!root) continue;
            var splines = root.GetComponentsInChildren<SplineComputer>(true);
            Undo.RecordObjects(splines, "Fix Dreamteck Spline Normals");

            foreach (var spline in splines)
            {
                if (!spline) continue;
                //if (FixOneSplineNormals(spline, LookUpWorld, FallbackAxis))
                //{
                //    fixedCount++;
                //    EditorUtility.SetDirty(spline);
                //}
                FixNormalsParallelTransport(spline);
                EditorUtility.SetDirty(spline);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Dreamteck] Fixed normals for {fixedCount} SplineComputer(s) in Selection.");
    }

    // ---- Core: "Perpendicular to Spline" + "Look Up" bằng toán ----
    // Perpendicular: normal ⟂ tangent
    // LookUp: chọn hướng gần với worldUp nhất
    private static bool FixOneSplineNormals(SplineComputer spline, Vector3 worldUp, Vector3 fallbackAxis)
    {
        var points = spline.GetPoints(); // GetPoints/SetPoints là API chuẩn :contentReference[oaicite:2]{index=2}
        if (points == null || points.Length < 2) return false;

        bool changed = false;

        for (int i = 0; i < points.Length; i++)
        {
            // percent theo index (đủ tốt cho việc lấy tangent theo hình dạng hiện tại)
            double t = (points.Length <= 1) ? 0.0 : (double)i / (points.Length - 1);

            SplineSample sample = new SplineSample();
            spline.Evaluate(t, ref sample); // Evaluate trả về forward/up/right :contentReference[oaicite:3]{index=3}

            Vector3 tangent = sample.forward;
            if (tangent.sqrMagnitude < 1e-8f) tangent = Vector3.forward;
            tangent.Normalize();

            // Project worldUp lên mặt phẳng vuông góc tangent => normal vừa ⟂ spline vừa "hướng Up"
            Vector3 n = worldUp - Vector3.Dot(worldUp, tangent) * tangent;

            // nếu tangent gần song song worldUp => n ~ 0 => dùng fallbackAxis
            if (n.sqrMagnitude < 1e-8f)
            {
                Vector3 fb = fallbackAxis.sqrMagnitude < 1e-8f ? Vector3.right : fallbackAxis;
                fb.Normalize();
                n = fb - Vector3.Dot(fb, tangent) * tangent;
            }

            n.Normalize();

            // Ghi normal vào point (manual cũng demo points[i].normal = Vector3.up :contentReference[oaicite:4]{index=4})
            if ((points[i].normal - n).sqrMagnitude > 1e-10f)
            {
                points[i].normal = n;
                changed = true;
            }
        }

        if (!changed) return false;

        spline.SetPoints(points);     // ghi lại toàn bộ array :contentReference[oaicite:5]{index=5}
        spline.RebuildImmediate();    // để mesh user cập nhật ngay (tránh “click mới hiện”)
        return true;
    }
    public static void FixNormalsParallelTransport(SplineComputer spline)
{
    var points = spline.GetPoints();
    if (points.Length < 2) return;

    // lấy sample đầu
    SplineSample prevSample = new SplineSample();
    spline.Evaluate(0.0, ref prevSample);

    Vector3 prevTangent = prevSample.forward.normalized;
    Vector3 normal = Vector3.up;

    // đảm bảo normal ⟂ tangent ban đầu
    normal = Vector3.Cross(prevTangent, Vector3.Cross(normal, prevTangent)).normalized;

    points[0].normal = normal;

    for (int i = 1; i < points.Length; i++)
    {
        double t = (double)i / (points.Length - 1);
        SplineSample curSample = new SplineSample();
        spline.Evaluate(t, ref curSample);

        Vector3 curTangent = curSample.forward.normalized;

        // xoay normal theo sự thay đổi của tangent
        Quaternion q = Quaternion.FromToRotation(prevTangent, curTangent);
        normal = (q * normal).normalized;

        points[i].normal = normal;

        prevTangent = curTangent;
    }

    spline.SetPoints(points);
    spline.RebuildImmediate();
}
}
#endif
