using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private GameManager gameManager;

    [SerializeField] private float distanceAway = 8;
    [SerializeField] private float cameraHeight = 10;
    private float rotationAround;
    private float rotationUp = 55f;
    [SerializeField] private float cameraHeightMin = 0;
    [SerializeField] private float cameraHeightMax = 90;

    float cameraPan = 0f;
    [SerializeField] float camRotateSpeed = 0.2f;
    Vector3 camPosition;


    private float horizontalAxis;
    private float verticalAxis;

    // Start is called before the first frame update
    void Start()
    {
        //camera always starts behind player
        rotationAround = player.transform.eulerAngles.y - 35;
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();   
    }

    // Update is called once per frame
    void Update()
    {


    }

    void LateUpdate()
    {
        if (gameManager.gameActive)
        {
            //gets input axis from mouse input
            horizontalAxis = Input.GetAxis("Mouse X");
            verticalAxis = Input.GetAxis("Mouse Y");

            //adds the mouse input to the axises multiplied by the rotationspeed
            rotationAround += horizontalAxis * camRotateSpeed;
            rotationUp -= verticalAxis * camRotateSpeed;

            //makes the camera only rotate so low/high on the x axis
            rotationUp = Mathf.Clamp(rotationUp, cameraHeightMin, cameraHeightMax);

            //sets a variable for the player position
            Vector3 playerPosition = player.transform.position;
            //sets the rotation based on rotation variables, camera pan is always 0
            Quaternion rotation = Quaternion.Euler(rotationUp, rotationAround, cameraPan);
            //creates an outwards vector based on the rotation
            Vector3 rotateVector = rotation * Vector3.one;

            //sets the camera position away from the player position at a set hight above and distance away in the rotation vector
            camPosition = playerPosition + (Vector3.up * cameraHeight) - (rotateVector * distanceAway);

            //checks if there is a wall in the way and it will set the position away from the wall if there is
            CheckForWall(ref playerPosition);

            //sets the cameras transform to the desired position
            transform.position = camPosition;

            //sets the camera to look back at the player
            transform.LookAt(player.transform);

            //sets the player's rotation to always look toward the direction of the camera
            player.transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);
        }
        
    }

    void CheckForWall(ref Vector3 player)
    {
        //method checks if there will be something in the way of the camera and moves the camera in front of it if there is something

        //declare a new raycast hit.
        RaycastHit wallHit = new RaycastHit();
        //linecast from your player to your cameras desired position to find collisions
        if (Physics.Linecast(player, camPosition, out wallHit) 
            //makes it so it doesn't reposition camera when the raycast hits the players collider
            && Vector3.Distance(player, wallHit.point) > 0.3)
        {
            //the x and z coordinates are pushed away from the wall by hit.normal, the y coordinate stays the same
            camPosition = new Vector3(wallHit.point.x + wallHit.normal.x * 0.5f, camPosition.y, wallHit.point.z + wallHit.normal.z * 0.5f);
        }
    }
}
