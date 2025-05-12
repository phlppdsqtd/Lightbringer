using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpointSound; //sound that we'll play when picking up a new checkpoint
    private Transform currentCheckpoint; //we'll store our last checkpoint here
    private Health playerHealth;
    private UIManager uiManager;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        uiManager = FindFirstObjectByType<UIManager>(); //new version of FindObjectOfType
    }

    private void CheckRespawn()
    {
        //check if check point available
        if (currentCheckpoint == null)
        {
            //show game over screen
            uiManager.GameOver();

            return; //don't execute the rest of the function
        }

        transform.position = currentCheckpoint.position; //move player to checkpoint position
        playerHealth.Respawn(); //restore player health and reset animation

        //move camera to the checkpoint room
        //(for this to work the checkpoint objects has to be placed as a child of the room object)
        Camera.main.GetComponent<CameraController>().MoveToNewRoom(currentCheckpoint.parent);
    }

    //activate checkpoints
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Checkpoint")
            {
                currentCheckpoint = collision.transform; //store the checkpoint that we activated as the current one
                SoundManager.instance.PlaySound(checkpointSound);
                collision.GetComponent<Collider2D>().enabled = false; //deactivate checkpoint collider
                collision.GetComponent<Animator>().SetTrigger("appear"); //trigger checkpoint animation
            }
    }
}
