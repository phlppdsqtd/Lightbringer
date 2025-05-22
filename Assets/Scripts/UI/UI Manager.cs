using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject controlsUI;

    [Header("Manabar")]
    [SerializeField] private Image manaBarFlashImage;
    [SerializeField] private float flashDuration;
    [SerializeField] private int flashCount;

    [Header("Counter")]
    [SerializeField] private TMPro.TextMeshProUGUI enemiesLeftText;
    [SerializeField] private TMPro.TextMeshProUGUI collectiblesLeftText;

    [Header("Skill Selection")]
    [SerializeField] private GameObject skillSelectionPanel;  //panel to show skills UI
    [SerializeField] private Button fireballButton;  //button for Fireball
    [SerializeField] private Button doubleJumpButton;  //button for Double Jump
    [SerializeField] private Skill fireballSkill;  //fireball Skill ScriptableObject
    [SerializeField] private Skill doubleJumpSkill;  //double Jump Skill ScriptableObject
    [SerializeField] private Image fireballIcon;  //image component for Fireball icon
    [SerializeField] private Image doubleJumpIcon;  //image component for Double Jump icon
    [SerializeField] private TMPro.TextMeshProUGUI skillPointsText;
    //[SerializeField] private SelectionArrow selectionArrow;

    [Header("Skill Warning")]
    [SerializeField] private GameObject warningUI;
    [SerializeField] private TMPro.TextMeshProUGUI warning;
    [SerializeField] private float warningFlashTime;

    [Header("Skill Awakened Display")]
    [SerializeField] private GameObject awakenUI; // UI panel container
    [SerializeField] private TMPro.TextMeshProUGUI awakenText; // actual text object
    [SerializeField] private float displayFlashTime = 2f; // duration text is visible
    [SerializeField] private float characterRevealDelay = 0.05f; // optional fine-tune

    [Header("Boss Flash UI")]
    [SerializeField] private GameObject bossFlashUI;
    [SerializeField] private float bossFlashDuration;
    [SerializeField] private int bossFlashCount;
    [SerializeField] private List<string> bossScenes;

    private Queue<string> dialogueQueue = new Queue<string>();
    private bool isTalking = false;
    private bool awaitingControlClose = false;
    private bool justClosedControlsUI = false;
    private Coroutine flashRoutine;
    private List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> collectibles = new List<GameObject>();
    private List<string> unlockedSkills = new List<string>();
    private Color unlockedColor = new Color(0.60f, 0.85f, 0.71f); // #99D9B4
    private bool skillUIOpen = false;
    private Coroutine awakenRoutine;

    //added
    private int previousEnemyCount = -1;
    private int previousCollectibleCount = -1;

    //added for dialogue skip issue
    private float dialogueInputDelay = 0.2f;
    private float dialogueTimer = 0f;

    //singleton reference
    public static UIManager Instance { get; private set; }

    /*
    //reference
    private PlayerSkillManager playerSkillManager;
    */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        //playerSkillManager = FindFirstObjectByType<PlayerSkillManager>();

        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
        dialogueUI.SetActive(false);
        controlsUI.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(DelayedSkillApply());

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            RegisterEnemy(enemy);

        foreach (var collectible in GameObject.FindGameObjectsWithTag("Collectible"))
            RegisterCollectible(collectible);

        //added for NPC skipping line0 issue
        justClosedControlsUI = false;
        awaitingControlClose = false;
        isTalking = false;

        // Optional: clear dialogue queue just in case
        dialogueQueue.Clear();

        string currentScene = SceneManager.GetActiveScene().name;
        if (bossFlashUI != null && bossScenes.Contains(currentScene))
            StartCoroutine(FlashBossUI());
    }

    private IEnumerator DelayedSkillApply()
    {
        while (PlayerSkillManager.instance == null)
            yield return null;

        //ApplyUnlockedSkills();

        if (PlayerSkillManager.instance.HasSkill(fireballSkill.name))
            ApplySkillUIState(fireballSkill, fireballButton, fireballIcon);

        if (PlayerSkillManager.instance.HasSkill(doubleJumpSkill.name))
            ApplySkillUIState(doubleJumpSkill, doubleJumpButton, doubleJumpIcon);
        Debug.Log("Finished DelayedSkillApply");

        UpdateSkillPointsDisplay();
        yield break;
    }

    private void Update()
    {
        if (gameOverScreen.activeInHierarchy || skillSelectionPanel.activeInHierarchy)
            return;

        /*
        //handles dialogue portion
        if (isTalking && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
        {
            DisplayNextLine();
        }
        */

        // Increment timer every frame if dialogue is active
        if (isTalking)
        {
            dialogueTimer += Time.unscaledDeltaTime;

            if (dialogueTimer >= dialogueInputDelay && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                DisplayNextLine();
                dialogueTimer = 0f; // Optional: reset timer after each advance
            }
        }
        else if (awaitingControlClose && controlsUI.activeInHierarchy && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
        {
            controlsUI.SetActive(false);
            awaitingControlClose = false;
            justClosedControlsUI = true;
            StartCoroutine(ResetJustClosedFlag());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //if pause screen already active unpause and vice versa
            if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
        }
    }

    #region DIALOGUE
    public void StartDialogue(string[] lines)
    {
        Debug.Log("StartDialogue called with " + lines.Length + " lines");
        if (lines == null || lines.Length == 0)
            return;

        if (controlsUI.activeInHierarchy || awaitingControlClose || justClosedControlsUI)
        {
            Debug.Log("Dialogue blocked by control flags.");
            return;
        }

        dialogueQueue.Clear();
        foreach (string line in lines)
        {
            dialogueQueue.Enqueue(line);
        }

        dialogueUI.SetActive(true);
        isTalking = true;
        awaitingControlClose = false;
        dialogueTimer = 0f; // prevent accidental immediate input

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        Debug.Log("DisplayNextLine called. Queue count: " + dialogueQueue.Count);
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = dialogueQueue.Dequeue();
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        controlsUI.SetActive(true);
        isTalking = false;
        awaitingControlClose = true;
    }

    public bool IsTalking()
    {
        return isTalking;
    }

    public bool IsControlsUIActive()
    {
        return controlsUI.activeInHierarchy;
    }

    private System.Collections.IEnumerator ResetJustClosedFlag()
    {
        yield return null; // Wait one frame
        justClosedControlsUI = false;
    }
    #endregion

    #region GAME OVER
    //activate game over screen
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        SoundManager.instance.PlaySound(gameOverSound);
    }

    //game over functions
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return; // Already in Main Menu

        Time.timeScale = 1f; // Make sure time resumes if returning from Pause
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit(); //quits the game (only works on build)

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; //exits play mode (only executed in editor)
#endif
    }
    #endregion 

    #region PAUSE
    public void PauseGame(bool status)
    {
        //if status == true, pause | status == false, unpause
        pauseScreen.SetActive(status);

        //when pause status is true change timescale to 0 (time stops)
        //when it's false change it back to 1 (time goes by normally)
        if (status)
            Time.timeScale = 0; //pause
        else
            Time.timeScale = 1; //unpause
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
    }
    #endregion

    #region MANA BAR
    public void FlashManaBar()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashBarImage());
    }

    private IEnumerator FlashBarImage()
    {
        for (int i = 0; i < flashCount; i++)
        {
            manaBarFlashImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(flashDuration);
            manaBarFlashImage.gameObject.SetActive(false);
            yield return new WaitForSeconds(flashDuration);
        }
    }
    #endregion

    #region COUNTER
    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);

        UpdateCounterUI();
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);

        UpdateCounterUI();
    }

    public void RegisterCollectible(GameObject collectible)
    {
        if (!collectibles.Contains(collectible))
            collectibles.Add(collectible);

        UpdateCounterUI();
    }

    public void UnregisterCollectible(GameObject collectible)
    {
        if (collectibles.Contains(collectible))
            collectibles.Remove(collectible);

        UpdateCounterUI();
    }

    /*
    public void UpdateCounterUI()
    {
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        collectibles.RemoveAll(c => c == null || !c.activeInHierarchy);

        enemiesLeftText.text = "" + enemies.Count;
        collectiblesLeftText.text = "" + collectibles.Count;
    }
    */

    public void UpdateCounterUI()
    {
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        collectibles.RemoveAll(c => c == null || !c.activeInHierarchy);

        int currentEnemyCount = enemies.Count;
        int currentCollectibleCount = collectibles.Count;

        enemiesLeftText.text = currentEnemyCount.ToString();
        collectiblesLeftText.text = currentCollectibleCount.ToString();

        if (previousEnemyCount != currentEnemyCount)
            StartCoroutine(AnimateTextChange(enemiesLeftText));

        if (previousCollectibleCount != currentCollectibleCount)
            StartCoroutine(AnimateTextChange(collectiblesLeftText));

        previousEnemyCount = currentEnemyCount;
        previousCollectibleCount = currentCollectibleCount;
    }

    private IEnumerator AnimateTextChange(TMPro.TextMeshProUGUI textElement)
    {
        Vector3 originalScale = textElement.transform.localScale;
        Vector3 enlargedScale = originalScale * 1.2f;
        float upDuration = 0.1f;
        float downDuration = 0.1f;

        // Scale up
        float elapsed = 0f;
        while (elapsed < upDuration)
        {
            textElement.transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsed / upDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        textElement.transform.localScale = enlargedScale;

        // Scale back down
        elapsed = 0f;
        while (elapsed < downDuration)
        {
            textElement.transform.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsed / downDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        textElement.transform.localScale = originalScale;
    }

    public int GetEnemyCount()
    {
        enemies.RemoveAll(e => e == null || !e.activeInHierarchy);
        return enemies.Count;
    }

    public int GetCollectibleCount()
    {
        collectibles.RemoveAll(c => c == null || !c.activeInHierarchy);
        return collectibles.Count;
    }

    #endregion

    #region SKILL SELECTION
    public void ShowSkillSelectionUI()
    {
        //ResetSelectionArrowPosition(); //COMMENT OUT AS NEEDED
        //selectionArrow.ResetArrowPosition();
        //selectionArrow.UpdateSkillInfo(); //COMMENT OUT AS NEEDED
        skillSelectionPanel.SetActive(true);
        Time.timeScale = 0;
        skillUIOpen = true;
        UpdateSkillPointsDisplay();
    }

    public void HideSkillSelectionUI()
    {
        skillSelectionPanel.SetActive(false);
        Time.timeScale = 1;
        skillUIOpen = false;
        //selectionArrow.ResetArrowPosition();
        //ResetSelectionArrowPosition(); //COMMENT OUT AS NEEDED
        //selectionArrow.UpdateSkillInfo(); //COMMENT OUT AS NEEDED
    }

    public bool IsSkillUIOpen()
    {
        return skillUIOpen;
    }

    public void ChooseFireball()
    {
        StartCoroutine(AttemptUnlock(fireballSkill, fireballButton, fireballIcon));
        /*
        UnlockSkill(fireballSkill);
        fireballButton.interactable = false;
        UpdateSkillPointsDisplay();
        HideSkillSelectionUI();
        */
    }

    public void ChooseDoubleJump()
    {
        StartCoroutine(AttemptUnlock(doubleJumpSkill, doubleJumpButton, doubleJumpIcon));
        /*
        UnlockSkill(doubleJumpSkill);
        doubleJumpButton.interactable = false;
        UpdateSkillPointsDisplay();
        HideSkillSelectionUI();
        */
    }

    /*
    private void UnlockSkill(Skill skill)
    {
        if (!unlockedSkills.Contains(skill.name))
        {
            unlockedSkills.Add(skill.name);
            SaveGame();
            Debug.Log($"Unlocked skill: {skill.name}");
        }

        // Update color and disable button
        if (skill.name == fireballSkill.name)
            ApplySkillUIState(fireballSkill, fireballButton, fireballIcon);
        else if (skill.name == doubleJumpSkill.name)
            ApplySkillUIState(doubleJumpSkill, doubleJumpButton, doubleJumpIcon);

        if (PlayerSkillManager.instance != null)
            PlayerSkillManager.instance.UnlockSkill(skill);
    }
    */

    //private void AttemptUnlock(Skill skill, Button button, Image icon)
    private IEnumerator AttemptUnlock(Skill skill, Button button, Image icon)
    {
        int skillPoints = LevelUnlockManager.instance.GetSkillPoints();
        //Debug.Log("skill is null? " + (skill == null));
        //Debug.Log("button is null? " + (button == null));
        //Debug.Log("icon is null? " + (icon == null));

        /*
        //this is the one causing null reference error prior to fix
        if (PlayerSkillManager.instance.HasSkill(skill.name))
        {
            ShowWarningFlash("POWER ALREADY AWAKENED");
            return;
        }

        if (skillPoints <= 0)
        {
            ShowWarningFlash("NOT ENOUGH POINTS");
            return;
        }
        */

        /*
            //this is the one causing null reference error prior to fix
            if (PlayerSkillManager.instance.HasSkill(skill.name))
            {
                StartCoroutine(ShowWarningFlash("ALREADY AWAKENED"));
                //return;
                yield break;
            }

            else if (skillPoints <= 0 && !PlayerSkillManager.instance.HasSkill(skill.name))
            {
                StartCoroutine(ShowWarningFlash("NOT ENOUGH POINTS"));
                //return;
                yield break;
            }
        */

        bool unlocked = PlayerSkillManager.instance.TryUnlockSkill(skill);

        if (unlocked)
        {
            ApplySkillUIState(skill, button, icon);
            UpdateSkillPointsDisplay();
            yield return StartCoroutine(ShowAwakenFlash($"{skill.name.ToUpper()} AWAKENED"));
            HideSkillSelectionUI();
        }

        else
        {

            //this is the one causing null reference error prior to fix
            if (PlayerSkillManager.instance.HasSkill(skill.name))
            {
                StartCoroutine(ShowWarningFlash("ALREADY AWAKENED"));
                //return;
                yield break;
            }

            else if (skillPoints <= 0)
            {
                StartCoroutine(ShowWarningFlash("NOT ENOUGH POINTS"));
                //return;
                yield break;
            }

        }
    }

    public void ResetSkillIconColors()
    {
        Color defaultColor = Color.white;
        fireballIcon.color = defaultColor;
        doubleJumpIcon.color = defaultColor;
    }

    private void ApplySkillUIState(Skill skill, Button button, Image icon)
    {
        icon.color = unlockedColor;
        //button.interactable = false;
    }

    /*
    private void SaveGame()
    {
        string skillData = string.Join(",", unlockedSkills);
        PlayerPrefs.SetString("UnlockedSkills", skillData);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        unlockedSkills = new List<string>();

        if (PlayerPrefs.HasKey("UnlockedSkills"))
        {
            string skillData = PlayerPrefs.GetString("UnlockedSkills");
            unlockedSkills = new List<string>(
                skillData.Split(',')
                        .Select(s => s.Trim())
                        .Distinct()
            );
        }
    }
    
    private void ApplyUnlockedSkills()
    {
        if (PlayerSkillManager.instance == null)
            return;

        foreach (string skillName in unlockedSkills)
        {
            if (skillName == fireballSkill.name)
            {
                PlayerSkillManager.instance.UnlockSkill(fireballSkill);
            }
            else if (skillName == doubleJumpSkill.name)
            {
                PlayerSkillManager.instance.UnlockSkill(doubleJumpSkill);
            }
        }
    }


    public bool IsSkillUnlocked(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }
    */

    private void UpdateSkillPointsDisplay()
    {
        if (LevelUnlockManager.instance != null)
        {
            int currentPoints = LevelUnlockManager.instance.GetSkillPoints();
            skillPointsText.text = $"{currentPoints}";
        }
    }

    private IEnumerator ShowWarningFlash(string message)
    {
        if (warningUI != null)
        {
            warning.text = message;
            warningUI.SetActive(true);
            yield return new WaitForSecondsRealtime(warningFlashTime);
            warningUI.SetActive(false);
        }
    }

    private IEnumerator ShowAwakenFlash(string message)
    {
        if (warningUI != null)
        {
            Debug.Log("Flash start1");
            awakenUI.SetActive(true);
            awakenText.text = ""; // clear previous text

            // Animate character-by-character
            for (int i = 0; i < message.Length; i++)
            {
                awakenText.text += message[i];
                yield return new WaitForSecondsRealtime(characterRevealDelay);
            }

            // Wait for full message display
            yield return new WaitForSecondsRealtime(displayFlashTime);

            Debug.Log("Flash start2");
            awakenUI.SetActive(false);
        }
    }

    /*
    // NEW: Flash the warning UI if skill unlock fails
    private void ShowWarningFlash(string message)
    {
        if (warningUI != null)
        {
            warning.text = message;
            StartCoroutine(FlashWarningUI());
        }
    }

    private IEnumerator FlashWarningUI()
    {
        warningUI.SetActive(true);
        yield return new WaitForSecondsRealtime(warningFlashTime);
        warningUI.SetActive(false);
    }

    public void ShowAwakenFlash(string message)
    {
        if (awakenUI != null)
        {
            StartCoroutine(AnimateAwakenText(message));
        }
    }
    
    private IEnumerator AnimateAwakenText(string message)
    {
        Debug.Log("Flash start1");
        awakenUI.SetActive(true);
        awakenText.text = ""; // clear previous text

        // Animate character-by-character
        for (int i = 0; i < message.Length; i++)
        {
            awakenText.text += message[i];
            yield return new WaitForSecondsRealtime(characterRevealDelay);
        }

        // Wait for full message display
        yield return new WaitForSecondsRealtime(displayFlashTime);

        Debug.Log("Flash start2");
        awakenUI.SetActive(false);
    }
    */
    #endregion

    #region BOSS FLASH
    private IEnumerator FlashBossUI()
    {
        for (int i = 0; i < bossFlashCount; i++)
        {
            bossFlashUI.SetActive(true);
            yield return new WaitForSeconds(bossFlashDuration);
            bossFlashUI.SetActive(false);
            yield return new WaitForSeconds(bossFlashDuration);
        }
    }
    #endregion
}