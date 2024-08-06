using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Collections;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System.Collections.Generic;
using System.IO;

public class AnimalSpawner : Spawner
{
    public GameObject staminaCanvas;
    public GameObject hungerCanvas;
    public GameObject thirstCanvas;
    public GameObject matingCanvas;
    public Transform mainCamera;

    public Material maleColor;
    public Material femaleColor;

    protected int step = 0;


    // Start is called before the first frame update
    public override void Generate()
    {
        Clear();
        SpawnAnimal(prefab, amount);
    }

    protected virtual void Start()
    {
        StartCoroutine(RegisterPopulation());
    }

    public void SpawnAnimal(GameObject prefab, float amount)
    {
        // TODO: The capsule collider's radious should depend on it's GameObject (size, desirability etc.)
        // It MUST BE while otherwise there's no guarantee that we get the amount of prefabs we wanted,
        // because of the continues when raycasting
        while (amount > 0)
        {
            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleY = Random.Range(zRange.x, zRange.y);
            Vector3 rayStart = new Vector3(sampleX, maxHeight, sampleY);

            int groundLayerMask = LayerMask.GetMask("Island");

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
                continue;
            if (hit.point.y < minHeight)
                continue;

            // Instantiate the prefab and set its position, rotation, and scale:
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(this.prefab, transform);
            instantiatedPrefab.transform.position = hit.point;
            instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y), Space.Self);
            float rnd = Random.Range(minScale.x, maxScale.x);
            instantiatedPrefab.transform.localScale = new Vector3(
                rnd,
                rnd,
                rnd
            );
            instantiatedPrefab.layer = LayerMask.NameToLayer("Bunny");

            CapsuleCollider capsuleCollider = instantiatedPrefab.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 20;
            capsuleCollider.radius = 8;

            // Attach scripts:
            Movement instantiatedMovement = instantiatedPrefab.AddComponent<Movement>();
            Gravity instantiatedGravity = instantiatedPrefab.AddComponent<Gravity>();
            Bunny instantiatedBunny = instantiatedPrefab.AddComponent<Bunny>();
            instantiatedBunny.size = rnd;
            instantiatedBunny.age = 1;
            instantiatedBunny.speed = Random.Range(1f, 10f);
            instantiatedBunny.sight = Random.Range(30, 70);
            instantiatedBunny.reproductiveUrge = Random.Range(30, 50);
            instantiatedBunny.charm = Random.Range(20, 100);
            instantiatedBunny.lifeSpan = Random.Range(4f, 6f);
            instantiatedBunny.pregnancyDuration = Random.Range(30, 60);
            instantiatedBunny.drying = Random.Range(25, 35);
            instantiatedBunny.starving = Random.Range(15, 25);
            int randomValue = Random.Range(0, 2);
            if (randomValue == 0)
            {
                instantiatedBunny.isMale = false;
                instantiatedBunny.SetAnimalData(prefab, femaleColor);
            }
            else
            {
                instantiatedBunny.isMale = true;
                instantiatedBunny.SetAnimalData(prefab, maleColor);
            }
            Sensor instantiatedSensor = instantiatedPrefab.AddComponent<Sensor>();

            //Change color
            Transform[] children = instantiatedPrefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == instantiatedPrefab.transform)
                    continue;
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!(child.name.Contains("Nose")) && !(child.name.Contains("Eyes")))
                    {
                        if(randomValue == 0)    // TODO: ne rnd, hanem fele-fele legyen
                        {
                            renderer.material = femaleColor;
                        }
                        else
                        {
                            renderer.material = maleColor;
                        }
                    }
                }
            }            

            // Create a BarsContainer as a child of instantiatedPrefab
            GameObject barsContainer = new GameObject("BarsContainer");
            barsContainer.transform.SetParent(instantiatedPrefab.transform);
            instantiatedBunny.barsContainer = barsContainer;

            GameObject instantiatedStamina = (GameObject)PrefabUtility.InstantiatePrefab(this.staminaCanvas, barsContainer.transform);
            instantiatedStamina.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 20, instantiatedPrefab.transform.position.z);
            instantiatedStamina.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedHunger = (GameObject)PrefabUtility.InstantiatePrefab(this.hungerCanvas, barsContainer.transform);
            instantiatedHunger.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 16, instantiatedPrefab.transform.position.z);
            instantiatedHunger.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedThirst = (GameObject)PrefabUtility.InstantiatePrefab(this.thirstCanvas, barsContainer.transform);
            instantiatedThirst.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 12, instantiatedPrefab.transform.position.z);
            instantiatedThirst.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedMating = (GameObject)PrefabUtility.InstantiatePrefab(this.matingCanvas, barsContainer.transform);
            instantiatedMating.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 8, instantiatedPrefab.transform.position.z);
            instantiatedMating.GetComponent<Billboard>().cam = mainCamera;

            instantiatedBunny.SetBars(barsContainer);

            amount--;
        }
    }
    public void SpawnBabies(Animal mother, Animal father, int amount)
    {
        GameObject babyPrefab = mother.prefab;
        for(int i = 0; i < amount; i++)
        {
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(babyPrefab, transform);
            instantiatedPrefab.transform.position = mother.transform.position;
            instantiatedPrefab.transform.rotation = mother.transform.rotation;
            instantiatedPrefab.transform.localScale = mother.transform.localScale;
            instantiatedPrefab.layer = LayerMask.NameToLayer("Bunny");

            CapsuleCollider capsuleCollider = instantiatedPrefab.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 20;
            capsuleCollider.radius = 8;

            // Attach scripts:
            Movement instantiatedMovement = instantiatedPrefab.AddComponent<Movement>();
            instantiatedMovement.ChangeDirection(amount, i);
            Gravity instantiatedGravity = instantiatedPrefab.AddComponent<Gravity>();
            Bunny instantiatedBunny = instantiatedPrefab.AddComponent<Bunny>();

            int randomValue = Random.Range(0, 2);
            if (randomValue == 0)
            {
                instantiatedBunny.isMale = false;
                instantiatedBunny.SetAnimalData(prefab, femaleColor);
            }
            else
            {
                instantiatedBunny.isMale = true;
                instantiatedBunny.SetAnimalData(prefab, maleColor);
            }
            Sensor instantiatedSensor = instantiatedPrefab.AddComponent<Sensor>();

            //Change color
            //TODO: eddig ez csak nyuszikra mûködik
            Transform[] children = instantiatedPrefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child == instantiatedPrefab.transform)
                    continue;
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!(child.name.Contains("Nose")) && !(child.name.Contains("Eyes")))
                    {
                        if (randomValue == 0)    // TODO: ne rnd, hanem fele-fele legyen
                        {
                            renderer.material = femaleColor;

                        }
                        else
                        {
                            renderer.material = maleColor;
                        }
                    }
                }
            }

            // Create a BarsContainer as a child of instantiatedPrefab
            GameObject barsContainer = new GameObject("BarsContainer");
            barsContainer.transform.SetParent(instantiatedPrefab.transform);
            instantiatedBunny.barsContainer = barsContainer;

            GameObject instantiatedStamina = (GameObject)PrefabUtility.InstantiatePrefab(this.staminaCanvas, barsContainer.transform);
            instantiatedStamina.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 20, instantiatedPrefab.transform.position.z);
            instantiatedStamina.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedHunger = (GameObject)PrefabUtility.InstantiatePrefab(this.hungerCanvas, barsContainer.transform);
            instantiatedHunger.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 16, instantiatedPrefab.transform.position.z);
            instantiatedHunger.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedThirst = (GameObject)PrefabUtility.InstantiatePrefab(this.thirstCanvas, barsContainer.transform);
            instantiatedThirst.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 12, instantiatedPrefab.transform.position.z);
            instantiatedThirst.GetComponent<Billboard>().cam = mainCamera;

            GameObject instantiatedMating = (GameObject)PrefabUtility.InstantiatePrefab(this.matingCanvas, barsContainer.transform);
            instantiatedMating.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 8, instantiatedPrefab.transform.position.z);
            instantiatedMating.GetComponent<Billboard>().cam = mainCamera;

            instantiatedBunny.SetBars(barsContainer);
            instantiatedBunny.SetTraits(mother, father);
        }
    }
    IEnumerator RegisterPopulation()
    {
        while (true)
        {
            step++;
            Transform[] children = this.GetComponentsInChildren<Transform>(true);
            int counter = Counter("Bunny");

            string filePath = "c:\\Work\\EcosystemSimulation\\Data\\PopulationData.csv";

            // Ellenõrizd, hogy a fájl létezik-e, és ha nem, hozd létre
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Time,Bunnies");
                }
            }

            // Írás a fájlba
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{step},{counter}");
            }
            yield return new WaitForSeconds(10f);
        }
    }
    public void RegisterDeath(Animal animal)
    {
        string filePath = "c:\\Work\\EcosystemSimulation\\Data\\DeathData.csv";

        // Ellenõrizd, hogy a fájl létezik-e, és ha nem, hozd létre
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Step\tDeathCause\tAge\tSpeed\tSight\tReproductiveUrge\tLifeSpan\tCharm\tPregnancyDuration\tStatus\tStarving\tDrying");
            }
        }

        // Írás a fájlba
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string dataLine = $"{step}\t{animal.cause.ToString()}\t{animal.age}\t{animal.speed}\t{animal.sight}\t{animal.reproductiveUrge}\t{animal.lifeSpan}\t{animal.charm}\t{animal.pregnancyDuration}\t{animal.prevStatus.ToString()}\t{animal.starving}\t{animal.drying}";
            writer.WriteLine(dataLine);
        }
    }
}