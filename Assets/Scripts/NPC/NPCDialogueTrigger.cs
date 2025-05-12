using UnityEngine;

public class NPCDialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] dialogueLines;

    private bool playerInRange = false;
    private UIManager uiManager;
    private Animator animator;
    private bool isTalking = false;

    private void Start()
    {
        uiManager = Object.FindFirstObjectByType<UIManager>();
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("idle", true); // Start in idle state
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !uiManager.IsTalking())
        {
            uiManager.StartDialogue(dialogueLines);
            TriggerActivateAnimation();
            isTalking = true;
        }

        // Reset to idle if dialogue & controls are both inactive and was previously talking
        if (isTalking && !uiManager.IsTalking() && !uiManager.IsControlsUIActive())
        {
            ResetToIdleAnimation();
            isTalking = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void TriggerActivateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("idle", false);      // Not idle anymore
            animator.SetTrigger("activate");      // Play talk animation
        }
    }

    private void ResetToIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("idle", true);       // Return to idle
            animator.ResetTrigger("activate");    // Reset activate trigger
        }
    }
}
