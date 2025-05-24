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
    [SerializeField] private GameObject skillSelectionPanel;
    [SerializeField] private Button fireballButton;
    [SerializeField] private Button doubleJumpButton;
    [SerializeField] private Skill fireballSkill;
    [SerializeField] private Skill doubleJumpSkill;
    [SerializeField] private Image fireballIcon;
    [SerializeField] private Image doubleJumpIcon;
    [SerializeField] private TMPro.TextMeshProUGUI skillPointsText;

    [Header("Skill Warning")]
    [SerializeField] private GameObject warningUI;
    [SerializeField] private TMPro.TextMeshProUGUI warning;
    [SerializeField] private float warningFlashTime;

    [Header("Skill Awakened Display")]
    [SerializeField] private GameObject awakenUI;
    [SerializeField] private TMPro.TextMeshProUGUI awakenText;
    [SerializeField] private float displayFlashTime = 2f;
    [SerializeField] private float characterRevealDelay = 0.05f;

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
    private Color unlockedColor = new Color(0.60f, 0.85f, 0.71f);
    private bool skillUIOpen = false;
    private Coroutine awakenRoutine;

    private int previousEnemyCount = -1;
    private int previousCollectibleCount = -1;

    private float dialogueInputDelay = 0.2f;
    private float dialogueTimer = 0f;

    public static UIManager Instance { get; private set; }

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

        justClosedControlsUI = false;
        awaitingControlClose = false;
        isTalking = false;

        dialogueQueue.Clear();

        string currentScene = SceneManager.GetActiveScene().name;
        if (bossFlashUI != null && bossScenes.Contains(currentScene))
            StartCoroutine(FlashBossUI());
    }

    private IEnumerator DelayedSkillApply()
    {
        while (PlayerSkillManager.instance == null)
            yield return null;

        if (PlayerSkillManager.instance.HasSkill(fireballSkill.name))
            ApplySkillUIState(fireballSkill, fireballButton, fireballIcon);

        if (PlayerSkillManager.instance.HasSkill(doubleJumpSkill.name))
            ApplySkillUIState(doubleJumpSkill, doubleJumpButton, doubleJumpIcon);

        UpdateSkillPointsDisplay();
        yield break;
    }

    private void Update()
    {
        if (gameOverScreen.activeInHierarchy || skillSelectionPanel.activeInHierarchy)
            return;

        if (isTalking)
        {
            dialogueTimer += Time.unscaledDeltaTime;

            if (dialogueTimer >= dialogueInputDelay && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)))
            {
                DisplayNextLine();
                dialogueTimer = 0f;
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
            if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
        }
    }

    #region DIALOGUE
    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        if (controlsUI.activeInHierarchy || awaitingControlClose || justClosedControlsUI)
        {
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
        dialogueTimer = 0f;

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
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
        yield return null;
        justClosedControlsUI = false;
    }
    #endregion

    #region GAME OVER
    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        SoundManager.instance.PlaySound(gameOverSound);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    #endregion 

    #region PAUSE
    public void PauseGame(bool status)
    {
        pauseScreen.SetActive(status);

        if (status)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
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

        float elapsed = 0f;
        while (elapsed < upDuration)
        {
            textElement.transform.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsed / upDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        textElement.transform.localScale = enlargedScale;

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
    }

    public bool IsSkillUIOpen()
    {
        return skillUIOpen;
    }

    public void ChooseFireball()
    {
        StartCoroutine(AttemptUnlock(fireballSkill, fireballButton, fireballIcon));
    }

    public void ChooseDoubleJump()
    {
        StartCoroutine(AttemptUnlock(doubleJumpSkill, doubleJumpButton, doubleJumpIcon));
    }

    private IEnumerator AttemptUnlock(Skill skill, Button button, Image icon)
    {
        int skillPoints = LevelUnlockManager.instance.GetSkillPoints();
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
            if (PlayerSkillManager.instance.HasSkill(skill.name))
            {
                StartCoroutine(ShowWarningFlash("ALREADY AWAKENED"));
                yield break;
            }

            else if (skillPoints <= 0)
            {
                StartCoroutine(ShowWarningFlash("NOT ENOUGH POINTS"));
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
    }

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
            awakenUI.SetActive(true);
            awakenText.text = "";

            for (int i = 0; i < message.Length; i++)
            {
                awakenText.text += message[i];
                yield return new WaitForSecondsRealtime(characterRevealDelay);
            }
            yield return new WaitForSecondsRealtime(displayFlashTime);
            
            awakenUI.SetActive(false);
        }
    }
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