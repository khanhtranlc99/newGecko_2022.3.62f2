using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject để lưu trữ shader references
/// Tránh shader bị strip khi build AssetBundle
/// </summary>
[CreateAssetMenu(fileName = "ShaderReferences", menuName = "Shader/Shader References")]
public class ShaderReferences : ScriptableObject
{
    [Header("Danh sách shader cần thiết")]
    public List<Shader> requiredShaders = new List<Shader>();
    
    [Header("Danh sách material mẫu")]
    public List<Material> requiredMaterials = new List<Material>();

    /// <summary>
    /// Lấy shader Spine SkeletonGraphic
    /// </summary>
    public Shader GetSpineShader()
    {
        foreach (Shader shader in requiredShaders)
        {
            if (shader != null && shader.name.Contains("Spine/SkeletonGraphic"))
            {
                return shader;
            }
        }
        return null;
    }

    /// <summary>
    /// Lấy material Spine SkeletonGraphic
    /// </summary>
    public Material GetSpineMaterial()
    {
        foreach (Material mat in requiredMaterials)
        {
            if (mat != null && mat.shader != null && mat.shader.name.Contains("Spine/SkeletonGraphic"))
            {
                return mat;
            }
        }
        return null;
    }

    /// <summary>
    /// Load ShaderReferences từ Resources
    /// </summary>
    public static ShaderReferences Load()
    {
        ShaderReferences references = Resources.Load<ShaderReferences>("ShaderReferences");
        if (references == null)
        {
            Debug.LogError("[ShaderReferences] Không tìm thấy ShaderReferences trong Resources!");
        }
        return references;
    }
}
