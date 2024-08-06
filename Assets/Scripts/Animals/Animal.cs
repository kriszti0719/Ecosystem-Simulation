using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Animal : MonoBehaviour
{
    public GameObject prefab;
    public Material color;
    public int breakCounter = 0;
    public List<GameObject> destructibles = new List<GameObject>();
    public GameObject targetRef;
    public List<GameObject> noTargetRefs = new List<GameObject>();   // TODO: idõvel felejtsen

    public float age;
    public bool isFertile;
    public float size;
    public CauseOfDeath cause;

    public float speed;                 // arrives faster to places BUT costs more stamina
    public int sight;                   // sees further BUT costs more stamina
    public int reproductiveUrge;        // higher chance to reproduction BUT spends more time finding a mate
    public float lifeSpan;              // more time to find a mate BUT with age will getting slower

    public bool isMale;
    //TODO: according to age it can change AND color depends from it
    public int charm;             // more chance at females BUT can be seen from further (predators)
    public int pregnancyDuration;  // stronger kids BUT can die during pregnancy meaning no inheriting at all
    public bool isPregnant = false;

    public Animal mate;
    public int currentPregnancy = 0;

    public Status status;
    public Status prevStatus;
    public Sensor sensor;
    public GameObject barsContainer;
    //StaminaBar
    public StaminaBar staminaBar;
    public int maxStamina = 100;
    public int currentStamina;
    public int restAmount;
    //Hunger
    public HungerBar hungerBar;
    public int maxHunger = 100;
    public int currentHunger;
    //public int eatDuration = 5;        // TODO: eatDuration depends on grass's size
    //public int eatAmount;
    public Plant food;
    public int starving;
    //Thirst
    public ThirstBar thirstBar;
    public int maxThirst = 200;
    public int currentThirst;
    public int drinkDuration = 5;
    public int drinkAmount = 10;
    public int drying;
    //Mating urge
    public MatingUrgeBar matingBar;
    public int maxMatingUrge = 100;
    public int currentMatingUrge;
    public int mateDuration = 1;
    public bool enableMating;

    int maxOxygen = 10;
    public int oxygen;

    float growing = 20f;
    float aging = 0.5f;

    protected virtual void Start()
    {
        if(age > 0.5)
        {
            isFertile = true;
        }
        restAmount = Mathf.RoundToInt(maxStamina * 0.05f);  //TODO: Wut?

        sensor = GetComponent<Sensor>();
        sensor.targetMask = sensor.targetMask = LayerMask.GetMask("None");
        cause = CauseOfDeath.NONE;
        status = Status.WANDERING;
        oxygen = maxOxygen;

        StartCoroutine(Aging());
        StartCoroutine(Step());
        StartCoroutine(Decide());
    }
    public void SetTraits(Animal mother, Animal father)
    {
        age = 0.01f;
        isFertile = false;
        enableMating = false;
        starving = Mathf.RoundToInt(MutateTrait((mother.starving + father.starving) / 2f));
        drying = Mathf.RoundToInt(MutateTrait((mother.drying + father.drying) / 2f));
        size = (mother.size + father.size) / 2f;
        speed = MutateTrait((mother.speed + father.speed) / 2f);
        sight = Mathf.RoundToInt(MutateTrait((mother.sight + father.sight) / 2f));
        reproductiveUrge = Mathf.RoundToInt(MutateTrait((mother.reproductiveUrge + father.reproductiveUrge) / 2f));
        lifeSpan = MutateTrait((mother.lifeSpan + father.lifeSpan) / 2f);
        charm = Mathf.RoundToInt(MutateTrait((mother.charm + father.charm) / 2f));
        pregnancyDuration = Mathf.RoundToInt(MutateTrait((mother.pregnancyDuration + father.pregnancyDuration) / 2f));
    }
    protected float MutateTrait(float averageTrait)
    {
        float mutationFactor = Random.Range(0.9f, 1.1f);

        // A véletlenszerû mutáció alkalmazása
        float mutatedTrait = averageTrait * mutationFactor;

        // Kerekítés és visszatérés
        return mutatedTrait;
    }
    public void SetBars(GameObject barsContainer)
    {
        this.staminaBar = barsContainer.GetComponentInChildren<StaminaBar>();
        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);

        this.hungerBar = barsContainer.GetComponentInChildren<HungerBar>();
        currentHunger = maxHunger;
        hungerBar.SetMaxHunger(maxHunger);

        this.thirstBar = barsContainer.GetComponentInChildren<ThirstBar>();
        currentThirst = maxThirst;
        thirstBar.SetMaxThirst(maxThirst);

        this.matingBar = barsContainer.GetComponentInChildren<MatingUrgeBar>();
        currentMatingUrge = maxMatingUrge;
        matingBar.SetMaxMatingUrge(maxMatingUrge);

        this.destructibles.Add(barsContainer);
    }
    public void SetAnimalData(GameObject prefab, Material color)
    {
        this.prefab = prefab;
        this.color = color;
        this.noTargetRefs.Add(this.GameObject());
        //TODO: set charm according to color
    }
    IEnumerator Aging()
    {
        while (true)
        {
            if(age <= 1)
            {
                age += aging/100f;
                Grow();
                yield return new WaitForSeconds(growing/100f); //TODO: legyen magasabb
            }
            else
            {
                enableMating = true;
                if (!isFertile)
                {
                    isFertile = true;
                }
                age += aging;
                if(status != Status.DIE && age >= lifeSpan)
                {
                    cause = CauseOfDeath.AGE;
                    ToDie();
                }
                yield return new WaitForSeconds(growing); //TODO: legyen magasabb
            }
        }
    }
    protected void Grow()
    {
        this.barsContainer.transform.SetParent(null);
        if (age < 0.4)
            this.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        else
            this.transform.localScale = new Vector3(size * age, size * age, size * age);
        this.barsContainer.transform.SetParent(this.transform);

    }
    IEnumerator Step()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.8f);

            if (transform.localPosition.y < 20 && targetRef == null)
            {
                oxygen--;
            }
            else
            {
                oxygen = maxOxygen;
            }

            if (breakCounter != 0)
                breakCounter--;

            if (isPregnant && currentPregnancy > 0)
            {
                currentPregnancy--;
                if (currentPregnancy == 0)
                {
                    GiveBirth();
                }
            }

            switch (status)
            {
                case Status.RESTING:
                    {
                        if (currentStamina + restAmount < maxStamina)
                            currentStamina += restAmount;
                        else
                        {
                            currentStamina = maxStamina;
                            breakCounter = 0;
                        }
                        currentHunger--;
                        currentThirst--;
                        if(isFertile)
                            currentMatingUrge--;                        
                        break;
                    }
                case Status.EATING:
                    {
                        food.eatDuration--;
                        if (currentStamina + restAmount < maxStamina)
                            currentStamina += restAmount;
                        else
                            currentStamina = maxStamina;
                        if (currentHunger + food.nutrition < maxHunger || food.eatDuration >= 0)
                            currentHunger += food.nutrition;
                        else
                        {
                            currentHunger = maxHunger;
                            breakCounter = 0;
                        }
                        currentThirst--;
                        if (isFertile)
                            currentMatingUrge--;
                        break;
                    }
                case Status.DRINKING:
                    {
                        if (currentStamina + restAmount < maxStamina)
                            currentStamina += restAmount;
                        currentHunger--;
                        if (currentThirst + drinkAmount < maxThirst)
                            currentThirst += drinkAmount;
                        else
                        {
                            currentThirst = maxThirst;
                            breakCounter = 0;
                        }
                        if (isFertile)
                            currentMatingUrge--;
                        break;
                    }
                case Status.MATING:
                    {
                        currentStamina--;
                        currentHunger--;
                        currentThirst--;
                        if (currentMatingUrge != maxMatingUrge)
                            currentMatingUrge = maxMatingUrge;
                        if (breakCounter == 0)
                        {
                            if (!this.isMale && !isPregnant)
                            {
                                // Base success rate for pregnancy
                                float baseSuccessRate = 0.8f;
                                // Random chance factor between -10% to +10%
                                float randomChanceFactor = UnityEngine.Random.Range(-0.1f, 0.1f);
                                // Calculate the overall success rate
                                float overallSuccessRate = baseSuccessRate + randomChanceFactor;

                                // Check if the mating is successful based on the calculated success rate
                                this.isPregnant = UnityEngine.Random.value < overallSuccessRate;
                                if (isPregnant)
                                {
                                    currentPregnancy = pregnancyDuration;
                                }
                                else
                                    mate = null;
                            }
                        }
                        break;
                    }
                case Status.SEARCHINGFOOD:
                case Status.SEARCHINGDRINK:
                case Status.SEARCHINGMATE:
                    {
                        currentStamina--;
                        currentHunger--;
                        currentThirst--;
                        if (isFertile)
                            currentMatingUrge--;
                        
                        break;
                    }
                case Status.DIE: { break; }
                default:
                    {
                        currentStamina--;
                        currentHunger--;
                        currentThirst--;
                        if (isFertile)
                            currentMatingUrge--;
                        break;
                    }
            }
            staminaBar.SetStamina(currentStamina);
            hungerBar.SetHunger(currentHunger);
            thirstBar.SetThirst(currentThirst);
            matingBar.SetMatingUrge(currentMatingUrge);
        }
    }
    IEnumerator Decide()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (status != Status.DIE && currentHunger == 0)
            {
                cause = CauseOfDeath.HUNGER;
                ToDie();
            }
            else if (status != Status.DIE && currentThirst == 0)
            {
                cause = CauseOfDeath.THIRST;
                ToDie();
            }
            else if (status != Status.DIE && oxygen == 0)
            {
                cause = CauseOfDeath.DROWN;
                ToDie();
            }
            switch (status)
            {                
                case Status.SEARCHINGMATE:
                    {
                        if(currentHunger <= starving)
                        {
                            sensor.targetMask = LayerMask.GetMask("BunnyFood");
                            status = Status.SEARCHINGFOOD;
                        }
                        if(currentThirst <= drying)
                        {
                            sensor.targetMask = LayerMask.GetMask("Drink");
                            status = Status.SEARCHINGDRINK;
                        }
                        if (targetRef != null)
                        {
                            Bunny tmp = targetRef.GetComponent<Bunny>();
                            if (tmp != null && tmp.isMale == this.isMale)
                            {
                                this.noTargetRefs.Add(targetRef);
                                this.targetRef = null;
                            }
                            else
                            {
                                if (tmp != null && tmp.isAcceptable(this))
                                {
                                    mate = tmp;
                                    prevStatus = status;
                                    status = Status.MOVING;
                                }
                                else
                                {
                                    this.noTargetRefs.Add(targetRef);
                                    this.targetRef = null;
                                }
                            }
                        }
                        break;
                    }
                case Status.SEARCHINGFOOD:
                case Status.SEARCHINGDRINK:
                case Status.WANDERING:
                    {
                        ChanceToRest();
                        if(!isFertile)
                        {
                            if (currentHunger < currentThirst && sensor.targetMask == LayerMask.GetMask("None"))
                            {
                                sensor.targetMask = LayerMask.GetMask("BunnyFood");
                                status = Status.SEARCHINGFOOD;
                            }
                            else if (sensor.targetMask == LayerMask.GetMask("None"))
                            {
                                sensor.targetMask = LayerMask.GetMask("Drink");
                                status = Status.SEARCHINGDRINK;
                            }
                            if (targetRef != null)
                            {
                                prevStatus = status;
                                status = Status.MOVING;
                            }
                        }
                        else
                        {
                            if(currentHunger < currentThirst && currentHunger < 70 && sensor.targetMask == LayerMask.GetMask("None"))
                            {
                                sensor.targetMask = LayerMask.GetMask("BunnyFood");
                                status = Status.SEARCHINGFOOD;
                            }
                            else if(currentThirst < 70 && sensor.targetMask == LayerMask.GetMask("None"))
                            {
                                sensor.targetMask = LayerMask.GetMask("Drink");
                                status = Status.SEARCHINGDRINK;
                            }
                            else if(enableMating && sensor.targetMask == LayerMask.GetMask("None"))   // TODO: ne konkrét határok, hanem % legyen
                            {
                                sensor.targetMask = LayerMask.GetMask("Bunny");
                                status = Status.SEARCHINGMATE;
                            }
                            if (targetRef != null)
                            {
                                prevStatus = status;
                                status = Status.MOVING;
                            }
                        }
                        break;
                    }
                case Status.MOVING:
                    {
                        ChanceToRest();
                        if (targetRef != null)
                        {
                            float distanceToTarget = Vector3.Distance(transform.position, targetRef.transform.position);
                            if (sensor.targetMask == LayerMask.GetMask("Bunny") && distanceToTarget < 7f)
                            {
                                Bunny mate = targetRef.GetComponent<Bunny>();
                                if (mate != null)
                                {
                                    if(mate.status == Status.WAITING)
                                        mate.ToMate();
                                    ToMate();
                                }
                            }
                            if (distanceToTarget < 5f)
                            {
                                if (sensor.targetMask == LayerMask.GetMask("BunnyFood"))
                                {
                                    ToEat();
                                }
                                else if (sensor.targetMask == LayerMask.GetMask("Drink"))
                                {
                                    ToDrink();
                                }
                            }
                        }
                        else
                            status = prevStatus;
                        break;
                    }
                case Status.RESTING:
                    {
                        if (breakCounter == 0 || currentStamina == maxStamina)
                        {
                            status = prevStatus;
                        }
                        break;
                    }
                case Status.EATING:
                    {
                        if(breakCounter == 0)
                        {
                            if (food != null)
                            {
                                food.Die();
                            }
                            food = null;
                            targetRef = null;
                            sensor.targetMask = LayerMask.GetMask("None");
                            status = Status.WANDERING;
                        }
                        if (currentHunger == maxHunger)
                        {
                            targetRef = null;
                            sensor.targetMask = LayerMask.GetMask("None");
                            status = Status.WANDERING;
                        }
                        break;
                    }
                case Status.DRINKING:
                    {
                        if (breakCounter == 0 || currentThirst == maxThirst)
                        {
                            targetRef = null;
                            sensor.targetMask = LayerMask.GetMask("None");
                            status = Status.WANDERING;
                        }
                        break;
                    }
                case Status.MATING:
                    {
                        if (breakCounter == 0)
                        {
                            targetRef = null;
                            sensor.targetMask = LayerMask.GetMask("None");
                            enableMating = false;
                            status = Status.WANDERING;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }        
        }
    }
    protected void ChanceToRest()
    {
        switch (currentStamina)
        {
            case 30:
                {
                    // According to sum other circumstances it can decide
                    //  - to move on OR
                    //  - to sleep
                    //  - to stop for a normal break
                    //  - to stop for a little break

                    int randomValue = Random.Range(0, 4);
                    if (currentHunger < starving || currentThirst < drying)
                        randomValue = 2;
                    if (randomValue == 1 && (currentHunger < starving + 30 || currentThirst < drying + 30))
                        randomValue = 2;
                    switch (randomValue)
                    {
                        case 0:
                            ToRest(20);
                            break;
                        case 1:
                            ToRest(50);
                            break;
                        default:
                            break;
                    }
                    break;
                }
            case 10:
                {
                    // According to sum other circumstances it can decide
                    //  - to move on OR
                    //  - to sleep
                    //  - to stop for a normal break
                    //  - to stop for a little break

                    int randomValue = Random.Range(0, 3);
                    if (currentHunger < starving || currentThirst < drying)
                        randomValue = 2;
                    if (randomValue==1 && (currentHunger < starving+30 || currentThirst < drying+30))
                        randomValue = 2;
                    switch (randomValue)
                    {
                        case 0:
                            ToRest(20);
                            break;
                        case 1:
                            ToRest(50);
                            break;
                        default:
                            break;
                    }
                    break;
                }
            case 0:
                {
                    // According to sum other circumstances it can decide
                    //  - to sleep
                    //  - to stop for a normal break
                    int randomValue = Random.Range(0, 2);
                    if (randomValue == 0)
                    {
                        ToRest(100);
                    }
                    else if (randomValue == 1)
                    {
                        ToRest(50);
                    }
                    break;
                }
            default:
                break;

        }
    }
    public bool isAcceptable(Animal mate)
    {
        //int charmDifference = mate.charm - charm; // Jó eséllyel pozitív, de lehet - is
        //float matingUrgeDifference = currentMatingUrge / 100f;
        //float acceptanceChance = Mathf.Clamp01(0.5f + charmDifference * 0.01f + matingUrgeDifference); 
        //float randomValue = Random.value;

        bool accepted = (mate.charm + (100 - currentMatingUrge)) < this.charm;

        if (accepted)
        {            
            bool canSee = sensor.FieldOfViewCheck(LayerMask.GetMask("Bunny"), mate.transform.gameObject);
            //prevStatus = status;
            if (!canSee)
            {
                sensor.targetMask = LayerMask.GetMask("None");
                targetRef = null;
                status = Status.WAITING;
            }
            else
            {
                sensor.targetMask = LayerMask.GetMask("Bunny");
                targetRef = mate.transform.gameObject;
                status = Status.MOVING;
            }
            this.mate = mate;
        }
        return accepted;
    }
    protected void GiveBirth()
    {
        isPregnant = false;
        this.GetComponentInParent<AnimalSpawner>().SpawnBabies(this, mate, Random.Range(6, 9));
    }
    protected void ToRest(int time)
    {
        if(status != Status.DIE)
        {
            prevStatus = status;
            status = Status.RESTING;
            if (currentStamina + time < maxStamina)
            {
                breakCounter = time;
            }
            else
            {
                breakCounter = maxStamina - currentStamina;
            }
        }
    }
    protected void ToEat()
    {
        food = targetRef.GetComponent<Plant>();
        if(food != null)
        {

            breakCounter = food.eatDuration;
            status = Status.EATING;
        }
    }
    protected void ToDrink()
    {
        breakCounter = drinkDuration;
        status = Status.DRINKING;
    }
    protected void ToMate()
    {
        breakCounter = mateDuration;
        status = Status.MATING;
    }
    protected void ToDie()
    {
        prevStatus = status;
        status = Status.DIE;
    }
    public void Die()
    {
        if (status == Status.DIE)
        {
            this.GetComponentInParent<AnimalSpawner>().RegisterDeath(this);
            foreach (GameObject g in destructibles)
            {
                Destroy(g);
            }
            Destroy(gameObject);
        }
    }
}