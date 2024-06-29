using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class SC_TPSController : MonoBehaviour
{
    private float idleTimer = 0f;
    private float idleSwitchTime = 5f; // Time interval to switch idle animations

    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    [HideInInspector]
    public bool canMove = true;
    private bool isRunning = false;

    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
        Transform snappingTurtle = transform.Find("SnappingTurtle");
        if (snappingTurtle != null)
        {
            animator = snappingTurtle.GetComponent<Animator>(); // Initialize the Animator component
            if (animator == null)
            {
                Debug.LogError("Animator component is missing on SnappingTurtle game object");
            }
        }
        else
        {
            Debug.LogError("SnappingTurtle game object is not found as a child of Player");
        }
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0 ;
            float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal"): 0;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

           // Check if the player is moving forward or backward
            if (curSpeedX != 0 || curSpeedY != 0)
            {
                if (isRunning)
                {
                    animator.Play("Run");
                }
                else
                {
                    animator.Play("Walk");
                }
                idleTimer = 0f; // Reset idle timer when moving
            }
            else
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleSwitchTime)
                {
                    animator.Play("Sit");
                    idleTimer = 0f; // Reset idle timer after switching animation
                }
            }

            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpSpeed;
                animator.Play("Jump"); // Trigger the "Jump" animation
                idleTimer = 0f; // Reset idle timer when jumping
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && canMove)
            {
                isRunning = true;
                speed = speed* (1.5f);
                animator.Play("Run");
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift) && canMove)
            {
                isRunning = false;
                speed = 7f;
            }
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector2(0, rotation.y);
        }
    }
}
