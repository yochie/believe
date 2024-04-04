using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public Transform orientation;
    public GameObject arrow;

    public void Remove()
    {
        Destroy(this.arrow);
        Destroy(this.gameObject);
    }
}
