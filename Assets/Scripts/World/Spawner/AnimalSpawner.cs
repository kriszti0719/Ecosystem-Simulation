using UnityEditor;
using UnityEngine;

public class AnimalSpawner : Spawner
{
    public GameObject canvasPrefab;
    public Transform mainCamera;

    // Start is called before the first frame update
    public override void Generate()
    {
        Clear();
        SpawnAnimal(prefab, amount);
    }

    public void SpawnAnimal(GameObject prefab, float amount)
    {
        while (amount > 0)
        {
            float sampleX = Random.Range(xRange.x, xRange.y);
            float sampleY = Random.Range(zRange.x, zRange.y);
            Vector3 rayStart = new Vector3(sampleX, maxHeight, sampleY);

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                continue;
            if (hit.point.y < minHeight)
                continue;

            // Instantiate the prefab and set its position, rotation, and scale:
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(this.prefab, transform);
            instantiatedPrefab.transform.position = hit.point;
            instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y), Space.Self);
            instantiatedPrefab.transform.localScale = new Vector3(
                Random.Range(minScale.x, maxScale.x),
                Random.Range(minScale.y, maxScale.y),
                Random.Range(minScale.z, maxScale.z)
            );

            // Attach scripts:
            Movement instantiatedMovement = instantiatedPrefab.AddComponent<Movement>();
            Gravity instantiatedGravity = instantiatedPrefab.AddComponent<Gravity>();
            Bunny instantiatedBunny = instantiatedPrefab.AddComponent<Bunny>();

            GameObject instantiatedCanvas = (GameObject)PrefabUtility.InstantiatePrefab(this.canvasPrefab, transform);

            if (instantiatedCanvas != null)
            {
                Debug.Log("B");


                instantiatedCanvas.transform.SetParent(instantiatedPrefab.transform);
                instantiatedCanvas.transform.position = new Vector3(instantiatedPrefab.transform.position.x, instantiatedPrefab.transform.position.y + 8, instantiatedPrefab.transform.position.z);
                Debug.Log(instantiatedCanvas.transform.position);

                instantiatedBunny.SetStaminaBar(instantiatedCanvas.GetComponentInChildren<StaminaBar>());
                instantiatedCanvas.GetComponent<Billboard>().cam = mainCamera;

                
            }


            amount--;
        }
    }
}