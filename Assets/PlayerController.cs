using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;

    [SerializeField] private float moveSpeed;

    [Range(0,1)]
    [SerializeField] private float rotationSpeed;

    [SerializeField] private Camera cam;

    private Vector3 horizontalMovementInput;

    // Start is called before the first frame update
    void Start()
    {
        this.horizontalMovementInput = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        this.horizontalMovementInput = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (horizontalMovementInput.magnitude > 0)
        {
            float camRotation = cam.transform.rotation.eulerAngles.y;
            Vector3 moveDir = (Quaternion.Euler(0, camRotation, 0) * this.horizontalMovementInput);
            this.controller.Move(moveDir * this.moveSpeed * Time.deltaTime);

            //rotation
            Quaternion rotateTowardsMoveDirection = Quaternion.LookRotation(moveDir);
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, rotateTowardsMoveDirection, this.rotationSpeed);
            this.transform.rotation = smoothedRotation;
        }
    }
}
