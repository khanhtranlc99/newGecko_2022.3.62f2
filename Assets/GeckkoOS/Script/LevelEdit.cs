using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(WormContainerRotator))]
public class LevelEdit : MonoBehaviour
{
    public int numMove = 3;
     public List<GameObject> lsWorms;
     public List<GameObject> lsWormsInGame;
     public WormTest prefabWorm;

    [Header("Editor Source (Transforms only)")]
    public string wormNamePrefix = "Worm_";
    public string prefabName = "WormPrefab";

    [Button]

     public void SpawnWorm()
     {
        foreach(var worm in lsWorms)
        {
            var tempWorm = Instantiate(prefabWorm);
            tempWorm.transform.position = worm.transform.position;
            tempWorm.wormsPost = new List<GameObject>();
            var allTransforms = worm.transform.GetComponentsInChildren<Transform>();
            for(int i = 1; i < allTransforms.Length; i++) // Bỏ qua phần tử đầu tiên (chính nó)
            {
                tempWorm.wormsPost.Add(allTransforms[i].gameObject);
            }
            tempWorm.Spawn();
            tempWorm.transform.parent = this.transform;
            lsWormsInGame.Add(tempWorm.gameObject);
        }
     }
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying) return;
        if (prefabWorm == null)
        {
            // 1) tìm theo tên prefab
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefabWorm = AssetDatabase.LoadAssetAtPath<WormTest>(path);
                EditorUtility.SetDirty(this);
            }
        }
            if (lsWorms == null) lsWorms = new List<GameObject>();
        lsWorms.Clear();

            // 2) Không có container -> scan toàn bộ con cháu và lấy ROOT worm
            // Root worm = object có prefix, và KHÔNG nằm dưới một worm khác (tránh ăn point con)
            var all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null || t == transform) continue;

                if (!string.IsNullOrEmpty(wormNamePrefix) && !t.name.StartsWith(wormNamePrefix))
                    continue;

                // Nếu parent phía trên cũng là Worm_ thì đây là con/point -> bỏ
                if (HasAncestorWithPrefix(t, wormNamePrefix))
                    continue;

                lsWorms.Add(t.gameObject);
            }
        

        // Sort theo số Worm_12 nếu có
        lsWorms.Sort((a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));

        EditorUtility.SetDirty(this);
    }

    static bool HasAncestorWithPrefix(Transform t, string prefix)
    {
        Transform p = t.parent;
        while (p != null)
        {
            if (!string.IsNullOrEmpty(prefix) && p.name.StartsWith(prefix))
                return true;
            p = p.parent;
        }
        return false;
    }

    static int ExtractIndex(string name)
    {
        int underscore = name.LastIndexOf('_');
        if (underscore < 0 || underscore >= name.Length - 1) return int.MaxValue;
        return int.TryParse(name.Substring(underscore + 1), out int v) ? v : int.MaxValue;
    }
#endif
}
