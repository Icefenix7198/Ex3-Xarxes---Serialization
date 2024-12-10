using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("________________________MOVEMENT________________________")]
    [Space(10)]
    public float speed;
    public float runMultiplier;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool canJump;

    //public float groundDrag;

    [Header("________________________KEYBINDS________________________")]
    [Space(10)]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode run = KeyCode.LeftShift;

    [Header("_____________________GROUND CHECKER_____________________")]
    [Space(10)]
    public LayerMask gdmask;
    float playerHeight;
    public bool isGrounded;

    [Header("________________________ASSIGN________________________")]
    [Space(10)]
    public Transform orientation;

    float horizontalMovement;
    float verticalMovement;

    Vector3 movementDirection;

    Rigidbody playerRb;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.freezeRotation = true;
        playerHeight = this.transform.localScale.y * 2;
        canJump = true;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        //Check if the player hits ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, gdmask);

        ReadInputs();
        SpeedController();

        if(playerRb.velocity.magnitude >  0)
        {
            animator.SetFloat("Speed", playerRb.velocity.magnitude);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetBool("Run", true);
            }
            else
            {
                animator.SetBool("Run", false);
            }
        }
        else
        {
            animator.SetBool("Run", false);
        }
    }

    private void MovePlayer()
    {
        movementDirection = transform.forward * verticalMovement + orientation.right * horizontalMovement;

        if (isGrounded)
        {
            if (horizontalMovement != 0 || verticalMovement != 0)
            {
                if (Input.GetKey(run))
                {
                    playerRb.velocity += movementDirection.normalized * speed * 10f * 2f;
                }
                else
                {
                    playerRb.velocity += movementDirection.normalized * speed * 10f;

                }
            }
            else
            {
                playerRb.velocity = Vector3.zero;
            }
        }
        else
        {
            if (Input.GetKey(run))
            {
                playerRb.velocity += movementDirection.normalized * speed * 10f * 2f * airMultiplier;
            }
            else
            {
                playerRb.velocity += movementDirection.normalized * speed * 10f * airMultiplier;
            }
        }
    }

    private void ReadInputs()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && canJump && isGrounded)
        {
            Jump();

            Invoke(nameof(JumpReset), jumpCooldown);

            canJump = false;
        }
    }

    private void SpeedController()
    {
        Vector3 actualVel = new Vector3(playerRb.velocity.x, 0 ,playerRb.velocity.z);

        if(actualVel.magnitude > speed)
        {
            Vector3 limitedVel = actualVel.normalized * speed;
            playerRb.velocity = new Vector3(limitedVel.x, playerRb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        playerRb.velocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
        playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void JumpReset()
    {
        canJump = true;
    }
}
