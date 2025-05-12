using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject levelUnlockManagerPrefab;

    void Start()
    {
        if (LevelUnlockManager.instance == null)
        {
            if (levelUnlockManagerPrefab != null)
            {
                GameObject obj = Instantiate(levelUnlockManagerPrefab);
                obj.name = "LevelUnlockManager"; // Optional: consistent naming
                DontDestroyOnLoad(obj);
                Debug.Log("LevelUnlockManager instantiated.");
            }
            else
            {
                Debug.LogWarning("LevelUnlockManager prefab is missing in LevelBootstrapper.");
            }
        }

        // Wait one frame to ensure the instance is fully initialized
        StartCoroutine(WaitAndInitialize());
    }

    private IEnumerator WaitAndInitialize()
    {
        yield return null; // Wait for LevelUnlockManager to finish Awake()
        InitializeLevelDataIfReady();
    }

    private void InitializeLevelDataIfReady()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level") &&
            int.TryParse(sceneName.Replace("Level", ""), out int levelNumber))
        {
            LevelUnlockManager.instance.UnlockLevelIfNotAlready(levelNumber);
        }
    }
}