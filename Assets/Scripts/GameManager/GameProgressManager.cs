using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [SerializeField] private WorldDataAsset worldDataAsset;
    [SerializeField] private string levelSelectionScene = "LevelChoosing";
    public WorldData CurrentWorld { get; private set; }

    string SavePath => Path.Combine(Application.persistentDataPath, "world_progress.json");

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Luôn đồng bộ từ ScriptableObject xuống CurrentWorld
        LoadFromScriptableObject();
    }

    /// <summary>
    /// Copy toàn bộ dữ liệu từ ScriptableObject sang CurrentWorld
    /// (bao gồm cả trạng thái unlock, passed, key)
    /// </summary>
    void LoadFromScriptableObject()
    {
        CurrentWorld = JsonUtility.FromJson<WorldData>(JsonUtility.ToJson(worldDataAsset.World));

        // Ghi đè file JSON cho đồng bộ
        Save();
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(CurrentWorld);
        File.WriteAllText(SavePath, json);
    }

    public static string GetSceneName(int floorId, int levelId)
    {
        return $"Floor{floorId}_Level{levelId}";
    }

    public void LoadLevel(int floorId, int levelId)
    {
        var floor = CurrentWorld.floors.FirstOrDefault(f => f.floorId == floorId);
        if (floor == null) return;
        var level = floor.levels.FirstOrDefault(l => l.levelId == levelId);
        if (level == null) return;
        if (!level.isUnlocked) return;
        SceneManager.LoadScene(GetSceneName(floorId, levelId));
    }

    public void MarkLevelPassed(int floorId, int levelId)
    {
        var floor = CurrentWorld.floors.FirstOrDefault(f => f.floorId == floorId);
        if (floor == null) return;
        var level = floor.levels.FirstOrDefault(l => l.levelId == levelId);
        if (level == null) return;
        if (!level.isUnlocked) return;
        if (!level.isPassed)
        {
            level.isPassed = true;
            if (level.isKeyLevel)
            {
                UnlockNextFloor(floorId);
            }
            else
            {
                UnlockNextLevelInFloor(floor);
            }
            Save();
        }
    }

    void UnlockNextLevelInFloor(FloorData floor)
    {
        var ordered = floor.levels.OrderBy(l => l.levelId).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].isPassed && i + 1 < ordered.Count)
            {
                if (!ordered[i + 1].isUnlocked)
                {
                    ordered[i + 1].isUnlocked = true;
                }
            }
        }
    }

    void UnlockNextFloor(int currentFloorId)
    {
        var nextFloor = CurrentWorld.floors.FirstOrDefault(f => f.floorId == currentFloorId + 1);
        if (nextFloor == null) return;
        if (!nextFloor.isUnlocked)
        {
            nextFloor.isUnlocked = true;
            var firstLevel = nextFloor.levels.OrderBy(l => l.levelId).FirstOrDefault();
            if (firstLevel != null)
            {
                firstLevel.isUnlocked = true;
            }
        }
    }

    public int GetCurrentFloorId()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Floor") && sceneName.Contains("_Level"))
        {
            var parts = sceneName.Split('_');
            if (parts.Length >= 2)
            {
                var floorPart = parts[0].Replace("Floor", "");
                if (int.TryParse(floorPart, out int floorId))
                {
                    return floorId;
                }
            }
        }
        return -1;
    }

    public int GetCurrentLevelId()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Floor") && sceneName.Contains("_Level"))
        {
            var parts = sceneName.Split('_');
            if (parts.Length >= 2)
            {
                var levelPart = parts[1].Replace("Level", "");
                if (int.TryParse(levelPart, out int levelId))
                {
                    return levelId;
                }
            }
        }
        return -1;
    }

    public void LoadLevelSelectScene()
    {
        SceneManager.LoadScene(levelSelectionScene);
    }

    /// <summary>
    /// Cho phép reset dữ liệu về ScriptableObject
    /// </summary>
    public void ResetProgress()
    {
        LoadFromScriptableObject();
    }
}
