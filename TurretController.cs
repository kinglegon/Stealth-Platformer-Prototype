using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    private GameObject target;
    private GameManager gameManager;
    private Light turretLight;
    private Quaternion startingRotation;
    [SerializeField] private float rotationSpeed = 8;
    private float rotationDirection = 1;
    private float playerTrackingMultiplier = 4;
    [SerializeField] private float maxRotationAngle = 30;
    private float fieldOfView = 30;
    private float viewDistance = 30;
    private bool trackingPlayer = false;
    private bool resetingPosition = false;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player");
        startingRotation = transform.rotation;
        turretLight = GetComponent<Light>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameActive)
        {
            PanTurret();
            SearchForTarget();
        }
        
    }

    void PanTurret()
    {
        if (!trackingPlayer && !resetingPosition)
        {
            //rotates the turret back and forth on the y axis
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * rotationDirection);
            //reverses turret direction when it hit rotational bounds
            float leftRotationBounds = startingRotation.eulerAngles.y - maxRotationAngle;
            float rightRotationBounds = startingRotation.eulerAngles.y + maxRotationAngle;
            //loops the bounds around to fit into 360 degrees
            if (leftRotationBounds < 0)
            {
                leftRotationBounds += 360;
            }
            if (rightRotationBounds > 360)
            {
                rightRotationBounds -= 360;
            }
            //if the distance between any of the bounds is less than 2 turn around
            if (Mathf.Abs(transform.rotation.eulerAngles.y - leftRotationBounds) < 2 || Mathf.Abs(transform.rotation.eulerAngles.y - rightRotationBounds) < 2)
            {
                rotationDirection = -rotationDirection;
            }
        }
        else if (trackingPlayer)
        {
            //turret finds information relative to the turret and player
            Vector3 targetDirection = target.transform.position - transform.position;
            float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
            float distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
            //makes a positive number if the player is to the right and negative if they're to the left and normalized it to 1
            float directionToPlayer = Vector3.Dot(transform.right, targetDirection.normalized);
            rotationDirection = Mathf.Sign(directionToPlayer);
            //rotates toward the player
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * rotationDirection * playerTrackingMultiplier);
            
            //sets a raycast to check if player is still in view
            RaycastHit rayCastHit;
            Physics.Raycast(transform.position, targetDirection, out rayCastHit);

            //loses the player if they move out of field of view, range, or has something obstruct vision
            if(angleToPlayer > fieldOfView || distanceToPlayer > viewDistance || !rayCastHit.collider.gameObject.Equals(target))
            {
                //sets turret to stop tracking the player when it loses sight and changes light to indicate that
                trackingPlayer = false;
                resetingPosition = true;
                turretLight.color = Color.white;
            }

        }
        else if (resetingPosition)
        {
            //points toward starting rotation and turns the camera in that direction
            Vector3 towardStartingRotation = startingRotation.eulerAngles - transform.rotation.eulerAngles;
            
            //checks if the turret is close to the starting position, rotates it toward if it isn't close and resets the position to the base if it is
            if (towardStartingRotation.magnitude > 1)
            {
                //changes sign of direction if it's more than 180 degrees away so it won't turn the long way around to base
                float fixedRotation;
                if (towardStartingRotation.y < -180)
                {
                    fixedRotation = towardStartingRotation.y + 360;
                }
                else
                {
                    fixedRotation = towardStartingRotation.y;
                }
                rotationDirection = Mathf.Sign(fixedRotation);
                transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * rotationDirection);
            }
            else
            {
                //snaps turret to starting position
                transform.Rotate(towardStartingRotation);
                resetingPosition = false;
                rotationDirection = 1;
            }
        }
        
     
    }

    void SearchForTarget()
    {
        //finds various information relative to the turret and the player
        Vector3 targetDirection = target.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(targetDirection, transform.forward);
        float distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
        //creates an empty raycast to the player and stores the result
        RaycastHit rayCastHit;
        Physics.Raycast(transform.position, targetDirection, out rayCastHit);
        //checks that the player is in the field of view of the turret and isn't obstructed by checking the raycast hit
        if (angleToPlayer < fieldOfView && distanceToPlayer < viewDistance && rayCastHit.collider.gameObject.Equals(target))
        {
            //if player is found start tracking and turn light red
            turretLight.color = Color.red;
            trackingPlayer = true;
            gameManager.GameOver();
        }
    }
}
