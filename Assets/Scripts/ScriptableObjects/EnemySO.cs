using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Custom Scriptable Objects/Enemy", order=0)]
public class EnemySO : ScriptableObject
{
    public string EnemyName;
    public float Health;
    public float Speed;
    public int Reward;
    public bool IsBoss;

    public GameObject EnemyPrefab;
}