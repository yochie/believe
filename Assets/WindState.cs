using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindState : MonoBehaviour
{
    [Tooltip("Counter clock-wise angle from north")]
    [SerializeField] private float windDirection;

    internal float GetWindDirection()
    {
        return this.windDirection;
    }
}
