using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            int enemyCount = UIManager.Instance.GetEnemyCount();
            int collectibleCount = UIManager.Instance.GetCollectibleCount();

            if (enemyCount == 0 && collectibleCount == 0)
            {
                string msg = "";
                msg += "DARKNESS HAS BEEN VANQUISHED HERE\n";
                msg += "THE PATH OPENS BEFORE YOU";
                StartCoroutine(ShowMessageAndLoadScene(msg));
            }
            else
            {
                string msg = "";
                msg += "THE PATH IS SEALED UNTIL YOUR TASK IS COMPLETE\n";

                if (enemyCount > 0)
                {
                    string umbraWord = enemyCount == 1 ? "UMBRA LINGERS" : "UMBRA LINGER";
                    msg += $"{enemyCount} {umbraWord} IN THE SHADOWS\n";
                }

                if (collectibleCount > 0)
                {
                    string fragmentWord = collectibleCount == 1 ? "LIGHT FRAGMENT" : "LIGHT FRAGMENTS";
                    msg += $"{collectibleCount} {fragmentWord} LEFT BEHIND";
                }
                ShowMessage(msg);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    private void ShowMessage(string msg)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = msg;
            messagePanel.SetActive(true);
            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDisplayTime);
        }
    }

    private void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    
    private IEnumerator ShowMessageAndLoadScene(string msg)
    {
        ShowMessage(msg);
        yield return new WaitForSeconds(messageDisplayTime);

        string currentSceneName = SceneManager.GetActiveScene().name;
        LevelUnlockManager.instance.MarkLevelComplete(currentSceneName);

        UIManager.Instance.ShowSkillSelectionUI();

        while (UIManager.Instance.IsSkillUIOpen())
        {
            yield return null;
        }
        SceneManager.LoadScene(nextSceneName);
    }
}