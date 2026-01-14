#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

[CustomEditor(typeof(EnemySO))]
public class EnemySOEditor : Editor
{
    [System.Serializable]
    private struct EnemyJSON
    {
        public string EnemyName;
        public float Health;
        public float Speed;
        public int Reward;
        public bool IsBoss;
    }

    [System.Serializable]
    private struct EnemyListJSON {
        public List<EnemyJSON> Enemies;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Draw button for importing all platforms from JSON
        if (GUILayout.Button("Import Enemies from JSON"))
        {
            // Open Enemies.json
            TextAsset enemiesJSON = Resources.Load<TextAsset>("Data/Enemies");

            if (enemiesJSON != null)
            {

                List<EnemyJSON> enemies = JsonUtility.FromJson<EnemyListJSON>(enemiesJSON.text).Enemies;
                Debug.Log("Importing " + enemies.Count + " enemies from JSON...");
                foreach (EnemyJSON enemy in enemies)
                {
                    Debug.Log(enemy);
                    // Find associated SO file
                    string filter = $"t:EnemySO {enemy.EnemyName}";
                    string[] guids = AssetDatabase.FindAssets(filter);
                    if (guids.Length > 0)
                    {   
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        EnemySO enemyasset = AssetDatabase.LoadAssetAtPath<EnemySO>(assetPath);
                        enemyasset.EnemyName = enemy.EnemyName;
                        enemyasset.Health = enemy.Health;
                        enemyasset.Speed = enemy.Speed;
                        enemyasset.Reward = enemy.Reward;
                        enemyasset.IsBoss = enemy.IsBoss;
                        EditorUtility.SetDirty(enemyasset);
                    } else
                    {
                        // Make the SO file
                        EnemySO newEnemy = ScriptableObject.CreateInstance<EnemySO>();
                        newEnemy.EnemyName = enemy.EnemyName;
                        newEnemy.Health = enemy.Health;
                        newEnemy.Speed = enemy.Speed;
                        newEnemy.Reward = enemy.Reward;
                        newEnemy.IsBoss = enemy.IsBoss;

                        AssetDatabase.CreateAsset(newEnemy, $"Assets/Resources/ScriptableObjects/Enemies/{newEnemy.EnemyName}.asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(newEnemy);
                    }
                }
            } else {
                Debug.LogError("Enemies.json not found in Resources/Data/");
            }
        }
    }
}

#endif