using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingCollider : MonoBehaviour
{
    public LayerMask ringLayer;

    private bool enteredRing;

    private void Start()
    {
        this.enteredRing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ringLayer.Contains(other.gameObject.layer))
            return;

        Ring ring = other.GetComponent<Ring>();
        Vector3 ringForward = ring.parentTransform.forward;
        Vector3 ringToPlayer = this.transform.position - ring.transform.position;
        bool onEntrySide = Vector3.Angle(ringForward, ringToPlayer) > 90f;
        if (onEntrySide) {
            Debug.Log("entered ring");
            this.enteredRing = true;
        } else
        {
            Debug.Log("wrong side");
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (!ringLayer.Contains(other.gameObject.layer))
            return;
        
        if (!enteredRing)
            return;

        Ring ring = other.GetComponent<Ring>();
        Vector3 ringForward = ring.parentTransform.forward;
        Vector3 ringToPlayer = this.transform.position - ring.transform.position;
        bool onExitSide = Vector3.Angle(ringForward, ringToPlayer) < 90f;
        if (onExitSide)
        {
            Debug.Log("left ring");
            ring.Consume();
        }
 
        this.enteredRing = false;
    }
}
