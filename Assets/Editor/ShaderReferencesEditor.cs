using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool để setup ShaderReferences asset
/// </summary>
public class ShaderReferencesEditor
{
    [MenuItem("Tools/Shader/Setup ShaderReferences Asset")]
    public static void SetupShaderReferences()
    {
        // Load hoặc tạo ShaderReferences asset
        string assetPath = "Assets/Resources/ShaderReferences.asset";
        ShaderReferences references = AssetDatabase.LoadAssetAtPath<ShaderReferences>(assetPath);
        
        if (references == null)
        {
            references = ScriptableObject.CreateInstance<ShaderReferences>();
            AssetDatabase.CreateAsset(references, assetPath);
            Debug.Log("[ShaderReferencesEditor] Đã tạo ShaderReferences.asset mới");
        }
        
        // Clear danh sách hiện tại
        references.requiredShaders.Clear();
        references.requiredMaterials.Clear();
        
        // Thêm các Spine shader
        string[] spineShaderPaths = {
            "Assets/Spine/Runtime/spine-unity/Shaders/SkeletonGraphic/Spine-SkeletonGraphic.shader",
            "Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton.shader",
            "Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton-Tint.shader"
        };
        
        foreach (string path in spineShaderPaths)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader != null)
            {
                references.requiredShaders.Add(shader);
                Debug.Log($"[ShaderReferencesEditor] ✓ Đã thêm shader: {shader.name}");
            }
            else
            {
                Debug.LogWarning($"[ShaderReferencesEditor] ✗ Không tìm thấy shader: {path}");
            }
        }
        
        // Tìm và thêm UI/Default shader
        Shader uiShader = Shader.Find("UI/Default");
        if (uiShader != null && !references.requiredShaders.Contains(uiShader))
        {
            references.requiredShaders.Add(uiShader);
            Debug.Log("[ShaderReferencesEditor] ✓ Đã thêm shader: UI/Default");
        }
        
        // Thêm Spine material mặc định
        Material spineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Spine/Runtime/spine-unity/Materials/SkeletonGraphicDefault.mat");
        if (spineMat != null)
        {
            references.requiredMaterials.Add(spineMat);
            Debug.Log($"[ShaderReferencesEditor] ✓ Đã thêm material: {spineMat.name}");
        }
        
        // Lưu asset
        EditorUtility.SetDirty(references);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[ShaderReferencesEditor] ✓ Hoàn tất! {references.requiredShaders.Count} shaders, {references.requiredMaterials.Count} materials");
        
        // Highlight asset trong Project window
        Selection.activeObject = references;
        EditorGUIUtility.PingObject(references);
    }

    [MenuItem("Tools/Shader/Print ShaderReferences Info")]
    public static void PrintShaderReferencesInfo()
    {
        ShaderReferences references = Resources.Load<ShaderReferences>("ShaderReferences");
        
        if (references == null)
        {
            Debug.LogError("[ShaderReferencesEditor] Không tìm thấy ShaderReferences trong Resources!");
            return;
        }
        
        Debug.Log($"[ShaderReferencesEditor] === ShaderReferences Info ===");
        Debug.Log($"Shaders ({references.requiredShaders.Count}):");
        foreach (Shader shader in references.requiredShaders)
        {
            if (shader != null)
            {
                Debug.Log($"  - {shader.name}");
            }
        }
        
        Debug.Log($"Materials ({references.requiredMaterials.Count}):");
        foreach (Material mat in references.requiredMaterials)
        {
            if (mat != null)
            {
                Debug.Log($"  - {mat.name} (Shader: {mat.shader?.name ?? "NULL"})");
            }
        }
    }
}
