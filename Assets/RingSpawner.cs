using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingSpawner : MonoBehaviour
{
    public static RingSpawner Instance { get; private set; }

    public GameObject ringPrefab;

    public Transform playerTransform;

    public float xRotationRange;
    public float yRotationRange;

    public float DistanceBetweenRings;

    public Vector3 NewStartDistance;

    private void Awake()
    {
        RingSpawner.Instance = this;
    }

    public void SpawnNextRing(Ring previous)
    {
        float xRotation = UnityEngine.Random.Range(-xRotationRange, xRotationRange);
        float yRotation = UnityEngine.Random.Range(-yRotationRange, yRotationRange);
        Quaternion ringRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 nextRingDirection = ringRotation * previous.parentTransform.forward;

        Vector3 nextPosition = previous.parentTransform.position + (nextRingDirection * DistanceBetweenRings);
        Quaternion nextRotation = Quaternion.LookRotation((nextPosition - previous.parentTransform.position).normalized);
        this.SpawnRing(nextPosition, nextRotation);
    }

    internal void SpawnNewStartRing()
    {
        Vector3 spawnPosition = this.playerTransform.position + (Quaternion.AngleAxis(this.playerTransform.rotation.eulerAngles.y, Vector3.up) * this.NewStartDistance) ;
        Quaternion spawnRotation = Quaternion.Euler(0, this.playerTransform.eulerAngles.y, 0); 
        this.SpawnRing(spawnPosition, spawnRotation);
    }

    public Ring SpawnRing(Vector3 position, Quaternion rotation)
    {
        Ring spawned = Instantiate(this.ringPrefab, position, rotation).GetComponentInChildren<Ring>();
        spawned.StartTimeout();
        return spawned;
    }
}
