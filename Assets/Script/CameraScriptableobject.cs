
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "CameraSettings", menuName = "ScriptableObjects/CameraSettings", order = 1)]
public class CameraScriptableobject : ScriptableObject
{

    public List<CemeraSetting> levelList;
}

[System.Serializable]
public class CemeraSetting
{
    public int level;
    public float fov;
}


