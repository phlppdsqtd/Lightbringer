using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager instance;

    private const string SkillPointsKey = "SkillPoints";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== LEVEL UNLOCKING =====

    private string GetUnlockKey(int levelNumber) => $"Level{levelNumber}";

    public void UnlockLevelIfNotAlready(int levelNumber)
    {
        string key = GetUnlockKey(levelNumber);
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"Level {levelNumber} unlocked.");
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return PlayerPrefs.GetInt(GetUnlockKey(levelNumber), 0) == 1;
    }

    // ===== LEVEL COMPLETION =====

    private string GetCompletedKey(string levelName) => $"Completed_{levelName}";

    public void MarkLevelComplete(string levelName)
    {
        string completeKey = GetCompletedKey(levelName);

        if (PlayerPrefs.GetInt(completeKey, 0) == 0)
        {
            PlayerPrefs.SetInt(completeKey, 1);
            PlayerPrefs.Save();
            AddSkillPoint(1);
            Debug.Log($"Level '{levelName}' marked as complete. Skill point awarded.");
        }
        else
        {
            Debug.Log($"Level '{levelName}' already completed. No skill point awarded.");
        }
    }

    public bool HasCompletedLevel(string levelName)
    {
        return PlayerPrefs.GetInt(GetCompletedKey(levelName), 0) == 1;
    }

    // ===== SKILL POINTS =====

    public void AddSkillPoint(int amount)
    {
        if (amount <= 0) return;

        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        int newTotal = current + amount;

        PlayerPrefs.SetInt(SkillPointsKey, newTotal);
        PlayerPrefs.Save();

        Debug.Log($"Added {amount} skill point(s). Total: {newTotal}");
    }

    public void SpendSkillPoint()
    {
        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        if (current > 0)
        {
            PlayerPrefs.SetInt(SkillPointsKey, current - 1);
            PlayerPrefs.Save();
            Debug.Log("Spent 1 skill point.");
        }
        else
        {
            Debug.LogWarning("No skill points to spend.");
        }
    }

    public int GetSkillPoints()
    {
        return PlayerPrefs.GetInt(SkillPointsKey, 0);
    }
}

/*
using UnityEngine;
using System.Collections.Generic;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager instance;

    private HashSet<string> completedLevels = new HashSet<string>();
    private const string CompletedLevelsKey = "CompletedLevels";
    private const string SkillPointsKey = "SkillPoints";

    private string GetLevelKey(int levelNumber) => $"Level{levelNumber}";
    private string GetCompletedKey(string levelName) => $"Completed_{levelName}";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCompletedLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // === LEVEL UNLOCKING ===
    public void UnlockLevel(int levelNumber)
    {
        string key = "Level" + levelNumber;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        string key = "Level" + levelNumber;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // === LEVEL COMPLETION ===
    public void MarkLevelComplete(string levelName)
    {
        if (!completedLevels.Contains(levelName))
        {
            completedLevels.Add(levelName);
            SaveCompletedLevels();
            AddSkillPoint(1); // Reward 1 skill point per level completion
        }
    }

    public bool HasCompletedLevel(string levelName)
    {
        return completedLevels.Contains(levelName);
    }

    private void LoadCompletedLevels()
    {
        string data = PlayerPrefs.GetString(CompletedLevelsKey, "");
        if (!string.IsNullOrEmpty(data))
        {
            completedLevels = new HashSet<string>(data.Split(','));
        }
    }

    private void SaveCompletedLevels()
    {
        string data = string.Join(",", completedLevels);
        PlayerPrefs.SetString(CompletedLevelsKey, data);
        PlayerPrefs.Save();
    }

    // === SKILL POINTS ===
    public void AddSkillPoint(int amount)
    {
        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        PlayerPrefs.SetInt(SkillPointsKey, current + amount);
        PlayerPrefs.Save();
    }

    public void SpendSkillPoint()
    {
        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        if (current > 0)
        {
            PlayerPrefs.SetInt(SkillPointsKey, current - 1);
            PlayerPrefs.Save();
        }
    }

    public int GetSkillPoints()
    {
        return PlayerPrefs.GetInt(SkillPointsKey, 0);
    }

    // === INITIALIZE LEVEL DATA ===
    public void InitializeLevelData()
    {
        // Load completed levels
        LoadCompletedLevels();
    }
}
*/