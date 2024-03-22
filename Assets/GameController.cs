using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    [SerializeField] private float windDirection;
    [SerializeField] private WindIndicator windIndicator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.windIndicator.SetWindDirection(this.windDirection);
    }
}
