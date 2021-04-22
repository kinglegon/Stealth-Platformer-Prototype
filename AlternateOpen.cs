using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateOpen : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float delay = 4;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        StartCoroutine("RotateObject");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RotateObject()
    {
        while (true)
        {
            if (gameManager.gameActive)
            {
                //rotates door
                transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                
                //checks to see if it gets to the end rotation points
                if (transform.rotation.eulerAngles.z > 90)
                {
                    //stops rotating and changes rotation direcion
                    yield return new WaitForSeconds(delay);
                    rotationSpeed = -rotationSpeed;
                }
                else if (transform.rotation.eulerAngles.z < 0)
                {
                    //stops rotating and changes rotation direcion
                    yield return new WaitForSeconds(delay);
                    rotationSpeed = -rotationSpeed;
                }
            }
            //wait 1 frame
            yield return 0;

        }
        
    }
}
