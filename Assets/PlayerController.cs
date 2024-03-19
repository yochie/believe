using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    
    [SerializeField] private Camera cam;

    [SerializeField] private float moveSpeed;

    [Range(0,1)]
    [SerializeField] private float rotationSpeed;

    [SerializeField] private float jumpSpeed;

    [SerializeField] private float gravityScale;    

    private Vector3 horizontalMovemInput;

    private readonly float baseGravity = 9.81f;

    // Start is called before the first frame update
    void Start()
    {
        this.horizontalMovemInput = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = Vector3.zero;

        this.horizontalMovemInput = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (horizontalMovemInput.magnitude > 0)
        {
            //horizontal movement
            float horizontalCamRotation = cam.transform.rotation.eulerAngles.y;
            Vector3 horizontalMoveDirection = Quaternion.Euler(0, horizontalCamRotation, 0) * this.horizontalMovemInput;
            Vector3 horizontalVelocity = horizontalMoveDirection * this.moveSpeed;
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;


            //rotate in movement direction
            Quaternion rotateTowardsMoveDirection = Quaternion.LookRotation(horizontalMoveDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, rotateTowardsMoveDirection, this.rotationSpeed);
            this.transform.rotation = smoothedRotation;
        }

        //vertical movement
        Debug.Log(controller.isGrounded);
        if (controller.isGrounded)
        {
            if (this.IsJumping())
                velocity.y = this.jumpSpeed;
            else
                velocity.y = -this.baseGravity * this.gravityScale;
        }
        else
        {
            velocity.y = this.controller.velocity.y - (this.baseGravity * this.gravityScale * Time.deltaTime);
        }
        this.controller.Move(velocity * Time.deltaTime);
    }

    bool IsJumping()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}
