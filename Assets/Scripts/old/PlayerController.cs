using System;
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

    [SerializeField] private float jumpCooldown;


    [SerializeField] private float gravityScale;    

    private Vector3 horizontalMovemInput;

    private readonly float baseGravity = 9.81f;

    private float jumpCooldownRemaining;

    private bool jumped;

    // Start is called before the first frame update
    void Start()
    {
        this.horizontalMovemInput = Vector3.zero;
        this.jumped = false;
        this.jumpCooldownRemaining = 0;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        if (horizontalMovemInput.magnitude > 0)
        {
            //horizontal movement
            float horizontalCamRotation = cam.transform.rotation.eulerAngles.y;
            Vector3 horizontalMoveDirection = Quaternion.Euler(0, horizontalCamRotation, 0) * this.horizontalMovemInput;
            Vector3 floorNormal = this.GetFloorNormal();
            Vector3 moveDirectionAlongSlope = Vector3.ProjectOnPlane(horizontalMoveDirection, floorNormal).normalized;
            Vector3 horizontalVelocity = moveDirectionAlongSlope * this.moveSpeed;
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;


            //rotate in movement direction
            Quaternion rotateTowardsMoveDirection = Quaternion.LookRotation(horizontalMoveDirection);
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, rotateTowardsMoveDirection, this.rotationSpeed);
            this.transform.rotation = smoothedRotation;
        }

        //vertical movement
        if (controller.isGrounded)
        {
            if (this.jumped)
            {
                //controller.gro
                velocity.y = this.jumpSpeed;
            }
            else
            {
                //constant down force needed for character controller to properly detect if grounded
                velocity.y = -this.baseGravity * this.gravityScale;
            }
        }
        else
        {
            velocity.y = this.controller.velocity.y - (this.baseGravity * this.gravityScale * Time.fixedDeltaTime);
        }
        this.controller.Move(velocity * Time.fixedDeltaTime);
        this.jumped = false;
    }

    private void Update()
    {
        this.jumpCooldownRemaining -= Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Space) && this.jumpCooldownRemaining <= 0)
        {
            this.jumped = true;
            this.jumpCooldownRemaining = this.jumpCooldown;
        }
        
        this.horizontalMovemInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }
    
    private Vector3 GetFloorNormal()
    {
        RaycastHit hit;
        float distToGround = controller.bounds.extents.y - controller.center.y;
        if (controller.isGrounded && Physics.Raycast(transform.position, -transform.up, out hit, distToGround + 1f))
        {
            Vector3 normal = hit.normal;
            return normal;
        } else
            return Vector3.up;
    }

    
}
