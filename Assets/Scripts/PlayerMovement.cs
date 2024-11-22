using UnityEngine;
using UnityEngine.InputSystem;
using InspectorDebugger;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform bottomOfPlayer;

    [SerializeField] private float accelertion = 1;
    [SerializeField] private float deccelartion = 1;
    [SerializeField] private float walkSpeed = 10;

    [ReadOnly][SerializeField] private float currentSpeed = 0;

    private Vector2 movementDirection;
    private Transform cameraTransform;

    private bool jumping = false;
    private float jumpDelay = .5f;

    //checks if the player is grounded
    private bool isGrounded = false;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    //This control player rotation so they face where the camera faces
    private void Update()
    {
        var playerFacing = new Vector3(cameraTransform.forward.x + transform.position.x, transform.position.y, cameraTransform.forward.z + transform.position.z);
        transform.LookAt(playerFacing);

        jumpDelay-=Time.deltaTime;
        //debug value
        
    }

    //Used for physics based movement. Could create functions for jumping and walking to clean up the code
    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(bottomOfPlayer.position, Vector3.down, .05f, ground);

        if (jumping) 
        {
            rb.AddForce(Vector3.up * 25, ForceMode.VelocityChange);
            jumping = false;
        }

        var horizontalMovement = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        var speed = horizontalMovement.magnitude;
        if (speed < walkSpeed && isGrounded && movementDirection.magnitude > 0)
        {
            Vector3 move = cameraTransform.forward * movementDirection.y + cameraTransform.right * movementDirection.x;
            move.y = 0;
            rb.AddForce(move.normalized * accelertion, ForceMode.VelocityChange);
        }
        else if(!isGrounded)
        {
            Vector3 move = cameraTransform.forward * movementDirection.y + cameraTransform.right * movementDirection.x;
            move.y = 0f;
            rb.AddForce(move.normalized * accelertion + Vector3.down, ForceMode.VelocityChange);
        }
        else if(isGrounded && movementDirection.magnitude <= 0)
        { 
            if(speed < 1)
            {
                rb.AddForce(-rb.linearVelocity, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddForce(-rb.linearVelocity.normalized * deccelartion, ForceMode.VelocityChange);
            }
        }

        //Debug Value for testing. Remove later.
        currentSpeed = horizontalMovement.magnitude;
    }

    //The functions below are connected to the player input
    public void OnMovement(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context) 
    {
        if(context.performed && isGrounded && jumpDelay <= 0)
        {
            jumping = true;
            jumpDelay = .5f;
        }
    }
}
