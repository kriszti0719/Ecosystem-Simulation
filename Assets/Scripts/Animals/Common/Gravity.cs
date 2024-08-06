using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Ray downRay = new Ray(transform.position + new Vector3(0f, 100f, 0f), Vector3.down);
        if (Physics.Raycast(downRay, out hit))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 100f - hit.distance, transform.position.z);
            }
        }
    }
}
