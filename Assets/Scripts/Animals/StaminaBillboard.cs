using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBillboard : MonoBehaviour
{
    public Transform cam;
    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
