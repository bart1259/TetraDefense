#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

[CustomEditor(typeof(TowerSO))]
public class TowerSOEditor : Editor
{
    [System.Serializable]
    private struct TowerJSON
    {
        public string TowerName;
        public string TowerShape;
        public string TowerDescription;
        public int Price;
        public float Range;
        public float FireTimer;
        public float BulletDamage;
    }

    [System.Serializable]
    private struct TowerListJSON {
        public List<TowerJSON> Towers;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Draw button for importing all platforms from JSON
        if (GUILayout.Button("Import Towers from JSON"))
        {
            // Open Towers.json
            TextAsset towersJSON = Resources.Load<TextAsset>("Data/Towers");

            if (towersJSON != null)
            {

                List<TowerJSON> towers = JsonUtility.FromJson<TowerListJSON>(towersJSON.text).Towers;
                Debug.Log("Importing " + towers.Count + " towers from JSON...");
                foreach (TowerJSON tower in towers)
                {
                    // Find associated SO file
                    string filter = $"t:TowerSO {tower.TowerName}";
                    string[] guids = AssetDatabase.FindAssets(filter);
                    if (guids.Length > 0)
                    {   
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        TowerSO towerasset = AssetDatabase.LoadAssetAtPath<TowerSO>(assetPath);
                        towerasset.TowerShape = tower.TowerShape;
                        towerasset.TowerDescription = tower.TowerDescription;
                        towerasset.Price = tower.Price;
                        towerasset.FireTimer = tower.FireTimer;
                        towerasset.Range = tower.Range;
                        towerasset.BulletDamage = tower.BulletDamage;
                        EditorUtility.SetDirty(towerasset);
                    } else
                    {
                        // Make the SO file
                        TowerSO newTower = ScriptableObject.CreateInstance<TowerSO>();
                        newTower.TowerShape = tower.TowerShape;
                        newTower.TowerDescription = tower.TowerDescription;
                        newTower.Price = tower.Price;
                        newTower.TowerName = tower.TowerName;
                        newTower.FireTimer = tower.FireTimer;
                        newTower.Range = tower.Range;
                        newTower.BulletDamage = tower.BulletDamage;

                        AssetDatabase.CreateAsset(newTower, $"Assets/Resources/ScriptableObjects/Towers/{newTower.TowerName}.asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(newTower);
                    }
                }
            } else {
                Debug.LogError("Towers.json not found in Resources/Data/");
            }
        }
    }
}

#endif