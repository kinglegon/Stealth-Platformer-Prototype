using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private float baseSpeed = 10;
    private float speed;
    private float maxSpeedDefaultRunning = 10;
    private float maxSpeedDefaultCrouching = 4;
    private float maxSpeedMagnitude;
    private float jumpForce = 6;
    private float slideDeceleration = 0.7f;
    private float slideCutoffSpeed = 4;
    private float colliderCrouchingOffset = 0.5f;
    private bool isOnGround = true;
    private bool isOnWall = false;
    private bool isSliding = false;
    private bool isCrouching = false;
    private int jumpCount = 0;
    private Rigidbody playerRigidBody;
    private CapsuleCollider playerCollider;
    private Animator playerAnimator;

    public GameObject mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        playerRigidBody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        playerAnimator = GetComponent<Animator>();

        speed = baseSpeed;
        maxSpeedMagnitude = maxSpeedDefaultRunning;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameActive)
        {
            MovePlayer();
            AnimatePlayer();
        }
        else
        {
            //stops the player moving on game over
            playerRigidBody.velocity = new Vector3(0, 0, 0);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        //on collision with the ground let us jump again, reset jump count
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
            jumpCount = 0;
            speed = baseSpeed;
        }

        //on collision with wall let us jump
        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = true;
        }

        //sets victory if the exit is touched
        if(collision.gameObject.CompareTag("Escape"))
        {
            gameManager.Victory();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //when you leave the ground make it so you can't jump mid air
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = false;
        }

        //when leaving wall makes it so you can't jump in midair
        if (collision.gameObject.CompareTag("Wall"))
        {
            isOnWall = false;
        }
    }


    float getXYMagnitude(Vector3 vector)
    {
        //find the magnitude of x and y and sets it to the max magnitude in the given direction
        return Mathf.Sqrt((vector.x * vector.x ) + (vector.z * vector.z));
    }


    Vector3 LimitXY(Vector3 vector)
    {
        Vector3 convertedVector = new Vector3();
        float vectorMagnitude = getXYMagnitude(vector);
        convertedVector.x = vector.x / vectorMagnitude * maxSpeedMagnitude;
        convertedVector.z = vector.z / vectorMagnitude * maxSpeedMagnitude;
        convertedVector.y = vector.y;

        return convertedVector;
    }


    void MovePlayer()
    {
        //sets a new vector3 based on input for horizontal and vertical inputs and assigns those to the x and z of the vector3
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        Vector3 direction = new Vector3();
        direction.z = verticalInput;
        direction.x = horizontalInput;

        AddMovement(direction);

        //stops the player if no input is detected, only affecting x and z axis
        if (verticalInput == 0 && horizontalInput == 0 && !isSliding)
        {
            DeceleratePlayer();
        }

        //sets a maximum speed a player can go and sets the velocity to it if it goes over, preserves y velocity for jumping
        if (getXYMagnitude(playerRigidBody.velocity) > maxSpeedMagnitude && getXYMagnitude(playerRigidBody.velocity) != 0)
        {
            playerRigidBody.velocity = LimitXY(playerRigidBody.velocity);
        }

        CheckJumpInput();
        CheckSlideInput();
        
    }


    void CheckJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (isOnGround || isOnWall) && jumpCount < 2)
        {
            //resets vertical momentum before the jump to ensure wall jumps act consistently
            playerRigidBody.AddForce(new Vector3(0, -playerRigidBody.velocity.y, 0), ForceMode.VelocityChange);

            //adds upward jump force and increases jump count
            playerRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpCount++;

            //adjusts the player speed for midair, adjusting to make it more maneuverable after wall jump
            if (jumpCount == 1)
            {
                speed = baseSpeed / 4;
            }
            else if (jumpCount == 2)
            {
                speed = baseSpeed / 2;
            }
        }
    }


    void CheckSlideInput()
    {
        //handles slide input
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSliding = true;
            //creates crouched capsule collider
            playerCollider.height /= 2;
            playerCollider.center = new Vector3(
                playerCollider.center.x, 
                playerCollider.center.y - colliderCrouchingOffset, 
                playerCollider.center.z);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isSliding = false;
            isCrouching = false;
            //resets capsule collider back to default
            playerCollider.height *= 2;
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                playerCollider.center.y + colliderCrouchingOffset,
                playerCollider.center.z);
            //resets max speed to running speed
            maxSpeedMagnitude = maxSpeedDefaultRunning;
        }
    }


    void AddMovement(Vector3 direction)
    {
        //normalizes the vector 3 to avoid faster diagonal movement and adds force player based on that direction while on the ground moving normally
        //player is on ground running
        if (isOnGround && !isSliding && !isCrouching)
        {
            playerRigidBody.AddRelativeForce(direction.normalized * speed, ForceMode.Acceleration);
        }
        //player has decelerated sliding and will move at half speed until they stand back up
        else if (isOnGround && isCrouching)
        {
            playerRigidBody.AddRelativeForce(direction.normalized * speed / 2, ForceMode.Acceleration);
        }
        //player jumped once and hasn't wall jumped
        else if (!isOnGround && jumpCount < 2)
        {
            playerRigidBody.AddRelativeForce(direction.normalized * speed, ForceMode.Acceleration);
        }
        //player has wall jumped make the speed a little faster
        else if (!isOnGround && jumpCount == 2)
        {
            playerRigidBody.AddRelativeForce(direction.normalized * speed, ForceMode.Acceleration);
        }
        //player is sliding
        else if (isOnGround && isSliding)
        {
            //gradually slows the player down if sliding
            Vector3 slidingDecelerationVector = new Vector3(
                -playerRigidBody.velocity.normalized.x * slideDeceleration,
                0,
                -playerRigidBody.velocity.normalized.z * slideDeceleration);

            playerRigidBody.AddForce(slidingDecelerationVector, ForceMode.Acceleration);

            //if player speed gets low enough change state to crouching
            if (getXYMagnitude(playerRigidBody.velocity) < slideCutoffSpeed)
            {
                isSliding = false;
                isCrouching = true;
                //sets the max speed to hald when crouching
                maxSpeedMagnitude = maxSpeedDefaultCrouching;
            }

        }
    }


    void DeceleratePlayer()
    {
        if (playerRigidBody.velocity.magnitude < 1 && isOnGround)
        {
            playerRigidBody.AddForce(-playerRigidBody.velocity, ForceMode.Acceleration);
        }
        else
        {
            Vector3 decelerationVector = new Vector3(
                    -playerRigidBody.velocity.normalized.x * baseSpeed,
                    0,
                    -playerRigidBody.velocity.normalized.z * baseSpeed);
            if (isOnGround)
            {

                //decelerates player based on ground speed
                playerRigidBody.AddForce(decelerationVector, ForceMode.Acceleration);
            }
            else
            {
                //halves speed deceleration in mid-air
                playerRigidBody.AddForce(decelerationVector / 2, ForceMode.Acceleration);
            }

        }
    }


    void AnimatePlayer()
    {
        //sets all the relative animation parameters to the variables they corespond to
        playerAnimator.SetFloat("MovementSpeed", playerRigidBody.velocity.magnitude);
        playerAnimator.SetBool("OnGround", isOnGround);
        playerAnimator.SetBool("Sliding", isSliding);
        playerAnimator.SetBool("GameActive", gameManager.gameActive);
    }

}
