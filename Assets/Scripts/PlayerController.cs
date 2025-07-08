using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 10f;

    [Header("Jumping")]
    public float jumpForce = 15f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    public float gravityScale = 4f;
    public float fallMultiplier = 2f;

    [Header("Wall Jump")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(12f, 15f);
    public float wallJumpTime = 0.2f;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Raycast Checks")]
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    public float groundCheckDistance = 0.05f;
    public float wallCheckDistance = 0.6f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private int facingDirection = 1;

    private bool jumpPressed;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpTimer;
    private int wallDirection;

    private bool canDash = true;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
            facingDirection = (int)Mathf.Sign(horizontalInput);

        CheckCollisions();

        // Coyote Time
        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        // Jump Buffer
        if (Input.GetButtonDown("Jump")) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        // Jump Conditions
        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || isWallSliding))
        {
            jumpPressed = true;
            jumpBufferTimer = 0;
        }

        // Variable jump height
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Wall Slide Control
        if (!isWallSliding && isTouchingWall && !isGrounded && rb.velocity.y < 0 &&
            Mathf.Sign(horizontalInput) == wallDirection && Mathf.Abs(horizontalInput) > 0.1f && !isWallJumping)
        {
            isWallSliding = true;
        }
        else if (isGrounded || !isTouchingWall || isWallJumping)
        {
            isWallSliding = false;
        }

        // Wall Jump timer
        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0)
                isWallJumping = false;
        }

        // Dash Input
        if (Input.GetKeyDown(KeyCode.Mouse1) && canDash && !isDashing)
        {
            StartDash();
        }

        // Dash Timing
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }

        // Dash Cooldown
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Horizontal movement
        if (!isWallJumping)
        {
            float targetSpeed = horizontalInput * moveSpeed;
            float speedDiff = targetSpeed - rb.velocity.x;
            float accelRate = acceleration;
            rb.velocity = new Vector2(rb.velocity.x + speedDiff * accelRate * Time.fixedDeltaTime, rb.velocity.y);
        }

        // Wall Slide
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }

        // Jump
        if (jumpPressed)
        {
            if (isWallSliding)
            {
                isWallJumping = true;
                wallJumpTimer = wallJumpTime;

                Vector2 jumpDir;

                // Hollow Knight-style: softer jump off if holding toward the wall
                if (Mathf.Sign(horizontalInput) == wallDirection && Mathf.Abs(horizontalInput) > 0.1f)
                {
                    jumpDir = new Vector2(-wallDirection * 0.3f, 1f); // softer outward force
                }
                else
                {
                    jumpDir = new Vector2(-wallDirection, 1f); // strong push away
                }

                jumpDir.Normalize();
                rb.velocity = new Vector2(jumpDir.x * wallJumpForce.x, wallJumpForce.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }

            jumpPressed = false;
            coyoteTimer = 0;
        }

        // Fall multiplier
        if (rb.velocity.y < 0 && !isWallSliding)
            rb.gravityScale = gravityScale * fallMultiplier;
        else
            rb.gravityScale = gravityScale;
    }

    void CheckCollisions()
    {
        Vector2 groundOrigin = (Vector2)transform.position + groundCheckOffset;
        isGrounded = Physics2D.Raycast(groundOrigin, Vector2.down, groundCheckDistance, groundLayer);

        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        isTouchingWall = leftWall || rightWall;
        wallDirection = rightWall ? 1 : leftWall ? -1 : 0;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        canDash = false;
        dashCooldownTimer = dashCooldown;

        float dashDir = horizontalInput != 0 ? horizontalInput : facingDirection;
        rb.velocity = new Vector2(dashDir * dashForce, 0f);
        rb.gravityScale = 0;
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = gravityScale;
    }

    void OnDrawGizmosSelected()
    {
        // Ground check
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + (Vector3)groundCheckOffset,
                        transform.position + (Vector3)groundCheckOffset + Vector3.down * groundCheckDistance);

        // Wall check
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
    }
}
