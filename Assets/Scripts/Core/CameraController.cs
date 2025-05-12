using UnityEngine;

public class CameraController : MonoBehaviour
{   
    //Room camera
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;

    //Follow player camera
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float cameraSpeed; //created since there is "speed" already
    private float lookAhead;

    private void Update()
    {
        //Room camera
        //transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z), ref velocity, speed); //speed * Time.deltaTime > changed to just speed accdg to tutor
        //Debug.Log("transform position: " + transform.position);

        //Follow player camera >> remove "+ lookAhead" if you want camera to be centered
        //without lookahead (center camera):
        transform.position = new Vector3(player.position.x, player.position.y+2.5f, transform.position.z); //can change transform to player if you want camera to follow y,z axis also

        //with lookahead:
        //transform.position = new Vector3(player.position.x + lookAhead, transform.position.y, transform.position.z); //can change transform to player if you want camera to follow y,z axis also
        //lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed); //lerp is to gradually increase lookAhead value from 0 to __
    }

    public void MoveToNewRoom(Transform _newRoom) {
        currentPosX = _newRoom.position.x + 4.43f; //added 4.43f to balance the camera offset
    }
}
