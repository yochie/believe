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
    [Range(0,1)]
    [SerializeField] private float smoothFactor;

    private float camYaw;

    private void Start()
    {
        //initialize without smoothing
        this.windArrow.localRotation = Quaternion.Euler(0, 0, this.windState.GetWindDirection() + this.windArrowOffset);
    }

    // Update is called once per frame
    void Update()
    {
        this.camYaw = cam.transform.rotation.eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, 0, camYaw);
        this.UpdateArrow(this.windState.GetWindDirection());
    }

    public void UpdateArrow(float counterClockwiseAngleFromNorth)
    {
        Quaternion target = Quaternion.Euler(0, 0, counterClockwiseAngleFromNorth + this.windArrowOffset);        
        this.windArrow.localRotation = Quaternion.Lerp(this.windArrow.localRotation, target, this.smoothFactor * Time.deltaTime);
    }

}
