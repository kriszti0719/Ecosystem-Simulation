using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{
    //StaminaBar
    public StaminaBar staminaBar;

    public int maxStamina = 100;
    public int currentStamina;
    public bool isMoving = true;
    public int breakCounter = 0;


    protected virtual void Start()
    {
        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);
    }
    protected void Update()
    {
    }

    public void SetStaminaBar(StaminaBar staminaBar)
    {
        this.staminaBar = staminaBar;
    }

    public void Move()
    {
        if(isMoving)
        {
            currentStamina--;

            if (currentStamina == 70)
            {
                // According to sum other circumstances it can decide
                //  - to move on OR
                //  - to stop for a little break

                // Generate random number (0 or 1)
                int randomValue = Random.Range(0, 2);
                // Decide whether to stop for a break or continue moving
                if (randomValue == 0)
                {
                    LittleBreak();
                }

            }
            else if (currentStamina == 50)
            {
                // According to sum other circumstances it can decide
                //  - to move on OR
                //  - to stop for a normal break
                //  - to stop for a little break

                int randomValue = Random.Range(0, 3);
                if (randomValue == 0)
                {
                    LittleBreak();
                } 
                else if (randomValue == 1)
                {
                    Break();
                }
            }
            else if (currentStamina == 30)
            {
                // According to sum other circumstances it can decide
                //  - to move on OR
                //  - to sleep
                //  - to stop for a normal break
                //  - to stop for a little break

                int randomValue = Random.Range(0, 4);
                if (randomValue == 0)
                {
                    LittleBreak();
                }
                else if (randomValue == 1)
                {
                    Break();
                }
                else if (randomValue == 2)
                {
                    Sleep();
                }
            }
            else if (currentStamina == 10)
            {
                // According to sum other circumstances it can decide
                //  - to move on OR
                //  - to sleep
                //  - to stop for a normal break
                //  - to stop for a little break

                int randomValue = Random.Range(0, 4);
                if (randomValue == 0)
                {
                    LittleBreak();
                }
                else if (randomValue == 1)
                {
                    Break();
                }
                else if (randomValue == 2)
                {
                    Sleep();
                }
            }
            else if (currentStamina == 5)
            {
                // According to sum other circumstances it can decide
                //  - to sleep
                //  - to stop for a normal break
                int randomValue = Random.Range(0, 2);
                if (randomValue == 0)
                {
                    Sleep();
                }
                else if (randomValue == 1)
                {
                    Break();
                }
            }

        }
        else
        {
            currentStamina++;
            breakCounter--;
            if(breakCounter == 0)
            {
                isMoving = true;
            }
        }

        staminaBar.SetStamina(currentStamina);
        
    }

    protected void LittleBreak()
    {
        isMoving = false;
        if (currentStamina + 20 < maxStamina)
        {
            breakCounter = 20;
        }
        else
        {
            breakCounter = maxStamina - currentStamina;
        }

    }

    protected void Break()
    {
        isMoving = false;
        if (currentStamina + 50 < maxStamina)
        {
            breakCounter = 50;
        }
        else
        {
            breakCounter = maxStamina - currentStamina;
        }
    }

    protected void Sleep()
    {
        isMoving = false;
        breakCounter = maxStamina - currentStamina;
    }
}