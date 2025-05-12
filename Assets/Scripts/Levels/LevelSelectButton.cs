using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int levelNumberToLoad;
    [SerializeField] private GameObject bgLock;   // Reference to BG1-Lock
    [SerializeField] private GameObject lockIcon; // Reference to Lock1
    [SerializeField] private Button button;

    private void Start()
    {
        Refresh();
        button.onClick.AddListener(TryLoadLevel);
    }

    private void TryLoadLevel()
    {
        string sceneToLoad = "Level" + levelNumberToLoad;

        if (SceneManager.GetActiveScene().name == sceneToLoad)
            return; // Already in the scene, don’t reload

        if (LevelUnlockManager.instance.IsLevelUnlocked(levelNumberToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void Refresh()
    {
        bool isUnlocked = LevelUnlockManager.instance.IsLevelUnlocked(levelNumberToLoad);
        bgLock.SetActive(!isUnlocked);
        lockIcon.SetActive(!isUnlocked);
        button.interactable = isUnlocked;
    }

}