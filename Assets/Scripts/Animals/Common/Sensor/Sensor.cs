using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    // Gyorsul�s
    public float Acceleration { get; set; }
    // Nyomat�k
    public float Torque { get; set; }

    public abstract void CalculateEffect();
}
