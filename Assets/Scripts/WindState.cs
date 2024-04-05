using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindState : MonoBehaviour
{
    [Tooltip("Counter clock-wise angle from north")]
    [SerializeField] private float windDirection;

    [SerializeField] private float windStrength;

    [SerializeField] private float changeInterval;

    [Tooltip("Degree delta per interval")]
    [Range(0, 360)]
    [SerializeField] private float changeRate;



    private void Awake()
    {
        //init to random position along increments
        this.windDirection = UnityEngine.Random.Range(0, (int) (360 / changeRate)) * changeRate;
    }

    private void Start()
    {

        this.StartCoroutine(RandomizeWindDirection());
    }

    private void Update()
    {

    }

    internal float GetWindDirection()
    {
        return this.windDirection;
    }

    internal float GetWindStrength()
    {
        return this.windStrength;
    }

    private IEnumerator RandomizeWindDirection()
    {
        while (true)
        {
            int changeDir = UnityEngine.Random.Range(-1, 2);
            this.windDirection += changeDir * changeRate;
            this.windDirection %= 360;
            yield return new WaitForSeconds(this.changeInterval);
        }
    }


}
