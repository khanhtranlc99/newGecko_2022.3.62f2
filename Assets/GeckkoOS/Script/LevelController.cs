using System.Collections;
using UnityEngine;

public class LevelController : MonoBehaviour
{
     
    public LevelEdit levelData;
    private bool isLoadingLevel = false;
    
    public void Init()
    {
        StartCoroutine(LoadLevelAsync());
    }

    private IEnumerator LoadLevelAsync()
    {
        isLoadingLevel = true;
        string pathLevel = StringHelper.PATH_CONFIG_LEVEL;
        string fullPath = string.Format(pathLevel, UseProfile.CurrentLevel);
        
        // Load level bất đồng bộ để tránh lag
        ResourceRequest request = Resources.LoadAsync<LevelEdit>(fullPath);
        
        // Đợi đến khi load xong
        yield return request;
        
        // Kiểm tra xem có load thành công không
        if (request.asset != null)
        {
            LevelEdit loadedLevel = request.asset as LevelEdit;
            if (loadedLevel != null)
            {
                levelData = Instantiate(loadedLevel);
            }
            else
            {
                Debug.LogError($"[LevelController] Không thể load level tại path: {fullPath}");
            }
        }
        else
        {
            Debug.LogError($"[LevelController] Không tìm thấy level tại path: {fullPath}");
        }
        
        isLoadingLevel = false;
    }

    // Kiểm tra xem level đã load xong chưa
    public bool IsLevelLoaded()
    {
        return !isLoadingLevel && levelData != null;
    }

}
