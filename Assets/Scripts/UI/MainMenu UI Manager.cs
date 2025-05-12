using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    [Header ("UI")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject levelSelectUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private GameObject resetUI;

    [Header ("Selection Arrow")]
    [SerializeField] private SelectionArrow mainMenuArrow;
    [SerializeField] private SelectionArrow levelSelectArrow;
    [SerializeField] private SelectionArrow settingsArrow;
    [SerializeField] private SelectionArrow resetArrow;

    [Header ("Level Button")]
    [SerializeField] private LevelSelectButton[] levelButtons;
    
    private void Awake()
    {
        mainMenuUI.SetActive(true);
        levelSelectUI.SetActive(false);
        settingsUI.SetActive(false);
        mainMenuArrow.ResetArrowPosition();
    }

    public void NewGame()
    {
        SceneManager.LoadScene(1); // or whatever your level 1 index is
    }

    public void LevelSelect()
    {
        mainMenuUI.SetActive(false);
        levelSelectUI.SetActive(true);
        levelSelectArrow.ResetArrowPosition();
    }

    public void Settings()
    {
        mainMenuUI.SetActive(false);
        settingsUI.SetActive(true);
        settingsArrow.ResetArrowPosition();
    }

    public void Reset()
    {
        settingsUI.SetActive(false);
        resetUI.SetActive(true);
        resetArrow.ResetArrowPosition();
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void Back()
    {
        mainMenuUI.SetActive(true);
        levelSelectUI.SetActive(false);
        settingsUI.SetActive(false);
        mainMenuArrow.ResetArrowPosition();
    }

    public void Cancel()
    {
        settingsUI.SetActive(true);
        resetUI.SetActive(false);
        settingsArrow.ResetArrowPosition();
    }

    public void Proceed()
    {
        // Delete level unlock keys
        for (int i = 1; i <= 6; i++)
        {
            PlayerPrefs.DeleteKey("Level" + i);
        }

        // Delete level completion keys
        string[] levelNames = { "Level1", "Level2", "Level3", "Level4", "Level5", "Level6" };
        foreach (string levelName in levelNames)
        {
            string completedKey = "Completed_" + levelName;
            PlayerPrefs.DeleteKey(completedKey);
        }

        // Delete unlocked skills
        if (PlayerSkillManager.instance != null)
        {
            PlayerSkillManager.instance.unlockedSkills.Clear();
        }
        PlayerPrefs.DeleteKey("UnlockedSkills");

        // Reset skill points
        PlayerPrefs.SetInt("SkillPoints", 0);

        PlayerPrefs.Save();

        // Refresh level UI buttons
        foreach (var button in levelButtons)
        {
            button.Refresh();
        }

        // Restore UI
        resetUI.SetActive(false);
        mainMenuUI.SetActive(true);
        mainMenuArrow.ResetArrowPosition();
    }
}
