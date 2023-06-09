using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sensor : MonoBehaviour
{
    public float Acceleration { get; set; }
    public float Torque { get; set; }

    public abstract void CalculateEffect();
}
