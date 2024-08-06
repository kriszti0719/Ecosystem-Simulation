using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotSpeed = 100f;

    public bool isPassive;
    public bool isWandering = false;
    public bool isRotatingLeft = false;
    public bool isRotatingRight = false;
    public bool isWalking = false;

    // Define the minimum and maximum Y positions where the bunnies can wander
    public float minY = 23f;
    private Animal animal;

    public float stepCounter = 0;

    void Start()
    {
        animal = GetComponent<Animal>();
    }

    private void Update()
    {
        stepCounter++;
        if (stepCounter == 20)
        {
            animal.Move();
            stepCounter = 0;
        }

        if (animal.isMoving)
        {
            if (!isWandering)
            {
                StartCoroutine(Wander());
            }

            if (isRotatingRight)
            {
                transform.Rotate(transform.up * Time.deltaTime * rotSpeed);
            }
            if (isRotatingLeft)
            {
                transform.Rotate(transform.up * Time.deltaTime * -rotSpeed);
            }
            if (isWalking)
            {
                // Cast a ray downward to check for ground
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position + new Vector3(0f, 100f, 0f), Vector3.down, out groundHit))
                {
                    if (groundHit.collider.CompareTag("Ground"))
                    {
                        // Check if the bunny is below the valid Y range (water level)
                        if (transform.position.y < minY)
                        {
                            // Change direction to avoid water
                            ChangeDirection();
                        }
                    }
                }

                // Cast a ray forward to check for water
                RaycastHit waterHit;
                if (Physics.Raycast(transform.position, transform.forward, out waterHit, moveSpeed))
                {
                    if (waterHit.collider.CompareTag("Water"))
                    {
                        // Change direction to avoid water
                        ChangeDirection();
                    }
                }

                // Move the bunny forward
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            }
        }        
    }


    IEnumerator Wander()
    {
        int rotTime = Random.Range(1, 3);
        int rotateWait = Random.Range(1, 2);
        int walkWait = Random.Range(1, 2);
        int walkTime = Random.Range(1, 5);

        isWandering = true;
        yield return new WaitForSeconds(walkWait);

        isWalking = true;
        yield return new WaitForSeconds(walkTime);

        // Check if the bunny is below the valid Y range
        if (transform.position.y < minY)
        {
            ChangeDirection(); // Call the method to change direction
        }

        yield return new WaitForSeconds(rotateWait);

        // Choose a random direction
        int rotateDir = Random.Range(0, 2);
        if (rotateDir == 0)
        {
            isRotatingLeft = true;
        }
        else
        {
            isRotatingRight = true;
        }

        yield return new WaitForSeconds(rotTime);
        isRotatingLeft = false;
        isRotatingRight = false;

        isWandering = false;
    }

    void ChangeDirection()
    {
        int rotateDir = Random.Range(0, 2);
        if (rotateDir == 0)
        {
            transform.Rotate(transform.up, -90f); // Rotate left by 90 degrees
        }
        else
        {
            transform.Rotate(transform.up, 90f); // Rotate right by 90 degrees
        }
    }
}
