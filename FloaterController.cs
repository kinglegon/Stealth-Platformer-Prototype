using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloaterController : MonoBehaviour
{
    private GameObject target;
    private GameManager gameManager;
    private Light searchLight;
    private float fieldOfView = 38;
    private float viewDistance = 15;
    [SerializeField] private float speed = 3;
    [SerializeField] private float patrolRange = 10;
    private Vector3 startingPosition;
    private Vector3 patrolDestination;

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
        target = GameObject.Find("Player");
        searchLight = GetComponentInChildren<Light>();
        RandomizePatrolDestination();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameActive)
        {
            FloaterMovement();
            SearchForTarget();
        }
        
    }

    void FloaterMovement()
    {
        //creates a vector pointing toward the patrol destination and moves the target toward it
        Vector3 towardsDestination = patrolDestination - transform.position;
        transform.Translate(towardsDestination.normalized * Time.deltaTime * speed);
        //if the floater is nearly on top of the destination create a new destination
        if(towardsDestination.magnitude < 1)
        {
            RandomizePatrolDestination();
        }
    }

    void SearchForTarget()
    {
        //finds various information relative to the turret and the player
        Vector3 targetDirection = target.transform.position - transform.position;
        float angleToPlayer = Vector3.Angle(targetDirection, -transform.up);
        float distanceToPlayer = Vector3.Distance(target.transform.position, transform.position);
        //creates an empty raycast to the player and stores the result
        RaycastHit rayCastHit;
        Physics.Raycast(transform.position, targetDirection, out rayCastHit);
        //checks that the player is in the field of view of the turret and isn't obstructed by checking the raycast hit
        if (angleToPlayer < fieldOfView && distanceToPlayer < viewDistance && rayCastHit.collider.gameObject.Equals(target))
        {
            //if player is found start tracking and turn light red
            searchLight.color = Color.red;
            gameManager.GameOver();
        }
        else
        {
            //turns it back if the player isn't in view
            searchLight.color = Color.white;
        }
    }
    

    void RandomizePatrolDestination()
    {
        //sets a new destination in range from where the player is
        patrolDestination = new Vector3(Random.Range(-patrolRange, patrolRange) + startingPosition.x, startingPosition.y, Random.Range(-patrolRange, patrolRange) + startingPosition.z);
    }


}
