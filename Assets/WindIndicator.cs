using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindIndicator : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private Transform windArrow;
    [Tooltip("Counter clockwise angle from default arrow position to north pointing")]
    [SerializeField] private float windArrowOffset;
    [SerializeField] private WindState windState;

    private float camYaw;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.camYaw = cam.transform.rotation.eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, 0, camYaw);
        this.SetWindDirection(this.windState.GetWindDirection());
    }

    public void SetWindDirection(float counterClockwiseAngleFromNorth)
    {
        this.windArrow.localRotation = Quaternion.Euler(0, 0, counterClockwiseAngleFromNorth + this.windArrowOffset);
    }
}
