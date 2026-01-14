using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "New Platform", menuName = "Custom Scriptable Objects/Platform", order=0)]
[System.Serializable]
public class PlatformSO : ScriptableObject
{
    public string PlatformName;
    [TextArea(5, 10)]
    public string PlatformShape;
    public Color PlatformColor = Color.white;
    public int Price = 10;

    public override string ToString()
    {
        return $"PlatformName: {PlatformName}, PlatformShape: {PlatformShape}, PlatformColor: {PlatformColor}, Price: {Price}";
    }
}