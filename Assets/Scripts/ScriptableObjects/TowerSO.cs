using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Tower", menuName = "Custom Scriptable Objects/Tower", order=0)]
public class TowerSO : ScriptableObject
{
    public string TowerName;
    [TextArea(5, 10)]
    public string TowerShape;
    public string TowerDescription;
    public int Price = 10;
    public float Range = 5.0f;
    public float FireTimer = 5.0f;
    public float BulletDamage = 5.0f;
    public GameObject TowerCursorPrefab;
    public GameObject TowerPrefab;

}