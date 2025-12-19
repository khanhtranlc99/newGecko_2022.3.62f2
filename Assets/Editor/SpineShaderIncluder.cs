using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tự động thêm Spine shaders vào Always Included Shaders để tránh bị strip khi build
/// </summary>
[InitializeOnLoad]
public class SpineShaderIncluder
{
    static SpineShaderIncluder()
    {
        // Chạy khi Unity Editor khởi động
        EditorApplication.delayCall += EnsureSpineShadersIncluded;
    }

    [MenuItem("Tools/Spine/Add Shaders to Always Included Shaders")]
    public static void EnsureSpineShadersIncluded()
    {
        string[] spineShaderNames = {
            "Spine/SkeletonGraphic",
            "Spine/SkeletonGraphic (Premultiply Alpha)",
            "Spine/Skeleton",
            "Spine/Skeleton Tint",
            "UI/Default"
        };

        // Lấy GraphicsSettings hiện tại
        var graphicsSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        var serializedObject = new SerializedObject(graphicsSettings);
        var alwaysIncludedShaders = serializedObject.FindProperty("m_AlwaysIncludedShaders");

        if (alwaysIncludedShaders == null)
        {
            Debug.LogError("[SpineShaderIncluder] Không tìm thấy m_AlwaysIncludedShaders property!");
            return;
        }

        bool modified = false;
        List<Shader> currentShaders = new List<Shader>();

        // Lấy danh sách shader hiện có
        for (int i = 0; i < alwaysIncludedShaders.arraySize; i++)
        {
            var shader = alwaysIncludedShaders.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
            if (shader != null)
            {
                currentShaders.Add(shader);
            }
        }

        // Thêm các Spine shader nếu chưa có
        foreach (string shaderName in spineShaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            
            if (shader == null)
            {
                // Thử tìm bằng cách search trong Assets
                string[] guids = AssetDatabase.FindAssets(shaderName.Split('/').Last() + " t:Shader");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (shader != null && shader.name == shaderName)
                    {
                        break;
                    }
                }
            }

            if (shader != null && !currentShaders.Contains(shader))
            {
                alwaysIncludedShaders.InsertArrayElementAtIndex(alwaysIncludedShaders.arraySize);
                alwaysIncludedShaders.GetArrayElementAtIndex(alwaysIncludedShaders.arraySize - 1).objectReferenceValue = shader;
                Debug.Log($"[SpineShaderIncluder] ✓ Đã thêm shader: {shaderName}");
                modified = true;
            }
            else if (shader != null)
            {
                Debug.Log($"[SpineShaderIncluder] Shader đã tồn tại: {shaderName}");
            }
            else
            {
                Debug.LogWarning($"[SpineShaderIncluder] ✗ Không tìm thấy shader: {shaderName}");
            }
        }

        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("[SpineShaderIncluder] ✓ Đã cập nhật Always Included Shaders");
        }
        else
        {
            Debug.Log("[SpineShaderIncluder] Tất cả shader đã được include");
        }
    }

    [MenuItem("Tools/Spine/List All Spine Shaders")]
    public static void ListSpineShaders()
    {
        string[] guids = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/Spine" });
        
        Debug.Log($"[SpineShaderIncluder] Tìm thấy {guids.Length} Spine shaders:");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader != null)
            {
                Debug.Log($"  - {shader.name} (Path: {path})");
            }
        }
    }
}
