using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets
{

    [System.Serializable]
    public class SerializableColor
    {
        public float r, g, b, a;
        public SerializableColor() { }
        public SerializableColor(Color c) { r = c.r; g = c.g; b = c.b; a = c.a; }
        public Color ToColor() => new Color(r, g, b, 1f);
    }

    [System.Serializable]
    public class SerializableVector2Int
    {
        public int x, y;
        public SerializableVector2Int() { }
        public SerializableVector2Int(Vector2Int v) { x = v.x; y = v.y; }
        public Vector2Int ToVector2Int() => new Vector2Int(x, y);
    }

    [System.Serializable]
    public class LevelData
    {
        public List<List<SerializableColor>> colorMatrix;
        public SerializableVector2Int gridSize;
    }

    public class LevelLoader
    {
        public string jsonFileName = "leveldata.json";
        public LevelData levelData = new LevelData();

        // Create a level with a specific pattern
        public void CreateSampleLevel()
        {
            levelData.gridSize = new SerializableVector2Int(new Vector2Int(8, 8));
            levelData.colorMatrix = new List<List<SerializableColor>>();
            Color blue = Color.blue;

            // Initialize 8x8 grid with empty color
            for (int y = 0; y < 8; y++)
            {
                var row = new List<SerializableColor>();
                for (int x = 0; x < 8; x++)
                {
                    var empty = Game.Colors[Random.Range(0, Game.Colors.Length)];
                    row.Add(new SerializableColor(empty));
                }
                levelData.colorMatrix.Add(row);
            }

            // Center of grid: 2x2 group, but top-left bubble is one to the left
            // Center is (3,3)-(4,4), but shift top-left to (2,3)
            levelData.colorMatrix[3][2] = new SerializableColor(blue); // shifted top-left
            levelData.colorMatrix[3][4] = new SerializableColor(blue); // top-right
            levelData.colorMatrix[4][3] = new SerializableColor(blue); // bottom-left
            levelData.colorMatrix[4][4] = new SerializableColor(blue); // bottom-right
        }

        // Save the color matrix to a JSON file using Newtonsoft.Json
        public void SaveLevelData()
        {
            string path = Path.Combine(Application.persistentDataPath, jsonFileName);
            string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"Level data saved to {path}");
        }

        // Load the color matrix from a JSON file using Newtonsoft.Json
        public void LoadLevelData()
        {
            string path = Path.Combine(Application.persistentDataPath, jsonFileName);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                levelData = JsonConvert.DeserializeObject<LevelData>(json);
                Debug.Log($"Level data loaded from {path}");
            }
            else
            {
                Debug.LogWarning($"No level data file found at {path}");
            }
        }
    }
}
