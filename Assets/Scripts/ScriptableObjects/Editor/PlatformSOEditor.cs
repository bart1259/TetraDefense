#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

[CustomEditor(typeof(PlatformSO))]
public class PlatformSOEditor : Editor
{
    [System.Serializable]
    private struct PlatformJSON
    {
        public string PlatformName;
        public string PlatformShape;
        public string PlatformColor;
        public int Price;
    }

    [System.Serializable]
    private struct PlatformListJSON {
        public List<PlatformJSON> Platforms;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Draw button for importing all platforms from JSON
        if (GUILayout.Button("Import Platforms from JSON"))
        {
            // Open Platforms.json
            TextAsset platformsJSON = Resources.Load<TextAsset>("Data/Platforms");

            if (platformsJSON != null)
            {

                List<PlatformJSON> platforms = JsonUtility.FromJson<PlatformListJSON>(platformsJSON.text).Platforms;
                Debug.Log("Importing " + platforms.Count + " platforms from JSON...");
                foreach (PlatformJSON platform in platforms)
                {
                    Debug.Log(platform);
                    // Find associated SO file
                    string filter = $"t:PlatformSO {platform.PlatformName}";
                    string[] guids = AssetDatabase.FindAssets(filter);
                    if (guids.Length > 0)
                    {   
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        PlatformSO platformasset = AssetDatabase.LoadAssetAtPath<PlatformSO>(assetPath);
                        Color col;
                        ColorUtility.TryParseHtmlString("#" + platform.PlatformColor, out col);
                        platformasset.PlatformColor = col;
                        platformasset.PlatformShape = platform.PlatformShape;
                        platformasset.Price = platform.Price;
                        EditorUtility.SetDirty(platformasset);
                    } else
                    {
                        // Make the SO file
                        PlatformSO newPlatform = ScriptableObject.CreateInstance<PlatformSO>();
                        Color col;
                        ColorUtility.TryParseHtmlString("#" + platform.PlatformColor, out col);
                        newPlatform.PlatformColor = col;
                        newPlatform.PlatformShape = platform.PlatformShape;
                        newPlatform.Price = platform.Price;
                        newPlatform.PlatformName = platform.PlatformName;

                        AssetDatabase.CreateAsset(newPlatform, $"Assets/Resources/ScriptableObjects/Platforms/{newPlatform.PlatformName}.asset");
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(newPlatform);
                    }
                }
            } else {
                Debug.LogError("Platforms.json not found in Resources/Data/");
            }
        }
    }
}

#endif