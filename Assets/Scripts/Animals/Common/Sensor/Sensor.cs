using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    // Gyorsulás
    public float Acceleration { get; set; }
    // Nyomaték
    public float Torque { get; set; }

    public abstract void CalculateEffect();
}
