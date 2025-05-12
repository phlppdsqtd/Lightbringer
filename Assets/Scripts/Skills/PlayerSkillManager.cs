using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerSkillManager : MonoBehaviour
{
    [SerializeField] private Skill fireballSkill;
    [SerializeField] private Skill doubleJumpSkill;

    public static PlayerSkillManager instance;
    public List<string> unlockedSkills { get; private set; } = new List<string>();
    public static event Action<string> OnSkillUnlocked;

    private const string UnlockedSkillsKey = "UnlockedSkills";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadUnlockedSkills();
        }
    }

    // Attempts to unlock a skill. Returns true if successful.
    public bool TryUnlockSkill(Skill skill)
    {
        if (skill == null || unlockedSkills.Contains(skill.name))
            return false;

        int skillPoints = LevelUnlockManager.instance.GetSkillPoints();
        if (skillPoints <= 0)
        {
            Debug.LogWarning("Not enough skill points to unlock this skill.");
            return false; // <-- NEW: inform caller of failure
        }

        // Spend point and unlock
        LevelUnlockManager.instance.SpendSkillPoint();
        unlockedSkills.Add(skill.name);
        SaveUnlockedSkills();

        Debug.Log($"Skill unlocked: {skill.name}");
        OnSkillUnlocked?.Invoke(skill.name);
        return true;
    }

    public bool HasSkill(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }

    private void LoadUnlockedSkills()
    {
        string data = PlayerPrefs.GetString(UnlockedSkillsKey, "");
        unlockedSkills = string.IsNullOrEmpty(data)
            ? new List<string>()
            : new List<string>(data.Split(','));

        // Notify listeners for each unlocked skill
        foreach (string skillName in unlockedSkills)
        {
            OnSkillUnlocked?.Invoke(skillName);
        }
    }

    private void SaveUnlockedSkills()
    {
        string data = string.Join(",", unlockedSkills);
        PlayerPrefs.SetString(UnlockedSkillsKey, data);
        PlayerPrefs.Save();
    }
}
