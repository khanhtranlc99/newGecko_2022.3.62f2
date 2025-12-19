
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WormMeshSkin", menuName = "Skin/WormMeshSkin", order = 1)]
public class WormMeshSkin : ScriptableObject
{
    public List<WormHeadMesh> headMeshes = new();
    public List<WormBodyMesh> bodyMeshes = new();

    public WormHeadMesh GetHeadMesh(int index)
    {
        if (index < 0 || index >= headMeshes.Count)
            return null;
        return headMeshes[index];
    }
    public WormBodyMesh GetBodyMesh(int index)
    {
        if (index < 0 || index >= bodyMeshes.Count)
            return null;
        return bodyMeshes[index];
    }
}

[System.Serializable]
public class WormBodyMesh
{
    public Mesh skin;
    public Material bodyMaterial;
}

[System.Serializable]
public class WormHeadMesh
{
    public Mesh skin;
    public Material headMaterial;
}
