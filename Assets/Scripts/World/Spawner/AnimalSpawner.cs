using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimalSpawner : Spawner
{
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
            Debug.Log("Generated");

            // Instantiate the prefab and set its position, rotation, and scale
            GameObject instantiatedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(this.prefab, transform);
            instantiatedPrefab.transform.position = hit.point;
            instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y), Space.Self);
            instantiatedPrefab.transform.localScale = new Vector3(
                Random.Range(minScale.x, maxScale.x),
                Random.Range(minScale.y, maxScale.y),
                Random.Range(minScale.z, maxScale.z)
            );

            // Attach the Movement script
            Movement instantiatedMovement = instantiatedPrefab.AddComponent<Movement>();

            // Attach the Gravity script
            Gravity instantiatedGravity = instantiatedPrefab.AddComponent<Gravity>();

            Bunny instantiatedBunny = instantiatedPrefab.AddComponent<Bunny>();
            amount--;
        }
    }
}
