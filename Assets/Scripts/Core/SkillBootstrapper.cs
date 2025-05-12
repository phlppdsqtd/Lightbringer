using UnityEngine;

public class SkillBootstrapper : MonoBehaviour
{
    public GameObject playerSkillManagerPrefab;

    void Start()
    {
        if (PlayerSkillManager.instance == null && playerSkillManagerPrefab != null)
        {
            GameObject skillManagerObject = Instantiate(playerSkillManagerPrefab);
            DontDestroyOnLoad(skillManagerObject);
            Debug.Log("PlayerSkillManager instantiated.");
        }

        SetupSkillSpecificManagers();
    }

    private void SetupSkillSpecificManagers()
    {
        if (PlayerSkillManager.instance != null)
        {
            //Debug.Log("Skill manager is ready to handle skill-related tasks.");
        }
    }
}
