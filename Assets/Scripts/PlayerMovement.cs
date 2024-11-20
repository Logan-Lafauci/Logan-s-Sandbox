using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float accelertion = 1;
    [SerializeField] private float walkSpeed = 10;

    private Vector2 movementDirection;
    private Transform cameraTransform;

    //Clean up and create a check to see if the player is grouned

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        var playerFacing = new Vector3(cameraTransform.forward.x + transform.position.x, transform.position.y, cameraTransform.forward.z + transform.position.z);
        transform.LookAt(playerFacing);
    }

    private void FixedUpdate()
    {
        if(rb.linearVelocity.magnitude < walkSpeed)
        {
            Vector3 move = cameraTransform.forward * movementDirection.y + cameraTransform.right * movementDirection.x;
            move.y = 0;
            rb.AddForce(move.normalized * accelertion, ForceMode.VelocityChange);
        }
    }

    //The functions below are connected to the player input
    public void OnMovement(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context) 
    {
        if(context.performed)
        {
            rb.AddForce(new Vector3(0, 25, 0), ForceMode.VelocityChange);
        }
    }
}
