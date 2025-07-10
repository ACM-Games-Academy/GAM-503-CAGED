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
    private bool hasAirDashed = false;

    [Header("Raycast Checks")]
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    public float groundCheckDistance = 0.05f;
    public float wallCheckDistance = 0.6f;
    public LayerMask groundLayer;

    [Header("Knockback")]
    public float force = 5;
    public ForceMode2D forceMode = ForceMode2D.Impulse;

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
        HandleInput();
        CheckCollisions();
        HandleJumpBufferAndCoyoteTime();
        HandleWallSlide();
        HandleDashInput();
        HandleDashCooldown();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        HandleMovement();
        HandleWallSlideMovement();
        HandleJump();
        ApplyGravity();
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
            facingDirection = (int)Mathf.Sign(horizontalInput);

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
    }

    void HandleJumpBufferAndCoyoteTime()
    {
        coyoteTimer = isGrounded ? coyoteTime : coyoteTimer - Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || isWallSliding))
        {
            jumpPressed = true;
            jumpBufferTimer = 0;
        }

        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0)
                isWallJumping = false;
        }
    }

    void HandleWallSlide()
    {
        bool slidingConditions = isTouchingWall && !isGrounded && rb.velocity.y < 0 &&
                                 Mathf.Sign(horizontalInput) == wallDirection &&
                                 Mathf.Abs(horizontalInput) > 0.1f && !isWallJumping;

        if (slidingConditions)
            isWallSliding = true;

        if (isTouchingWall && !isGrounded)
            hasAirDashed = false;

        if (isGrounded || !isTouchingWall || isWallJumping)
            isWallSliding = false;
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isDashing)
        {
            if (isGrounded && canDash)
            {
                StartDash();
                canDash = false;
                dashCooldownTimer = dashCooldown;
            }
            else if (!isGrounded && !hasAirDashed)
            {
                StartDash();
                hasAirDashed = true;
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                EndDash();
        }
    }

    void HandleDashCooldown()
    {
        if (isGrounded)
        {
            if (!canDash)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0)
                    canDash = true;
            }

            hasAirDashed = false;
        }
    }

    void HandleMovement()
    {
        if (isWallJumping) return;

        float targetSpeed = horizontalInput * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        rb.velocity = new Vector2(rb.velocity.x + speedDiff * acceleration * Time.fixedDeltaTime, rb.velocity.y);
    }

    void HandleWallSlideMovement()
    {
        if (isWallSliding)
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    void HandleJump()
    {
        if (!jumpPressed) return;

        if (isWallSliding)
        {
            isWallJumping = true;
            wallJumpTimer = wallJumpTime;

            Vector2 jumpDir = (Mathf.Sign(horizontalInput) == wallDirection && Mathf.Abs(horizontalInput) > 0.1f)
                ? new Vector2(-wallDirection * 0.3f, 1f)
                : new Vector2(-wallDirection, 1f);

            jumpDir.Normalize();
            rb.velocity = new Vector2(jumpDir.x * wallJumpForce.x, wallJumpForce.y);
            hasAirDashed = false;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        jumpPressed = false;
        coyoteTimer = 0;
    }

    void ApplyGravity()
    {
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
        rb.gravityScale = 0;

        float dashDir = horizontalInput != 0 ? horizontalInput : facingDirection;
        rb.velocity = new Vector2(dashDir * dashForce, 0f);
    }

    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = gravityScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Calculate the direction to push the player
            ContactPoint2D contactPoint = collision.GetContact(0);
            Vector2 playerPosition = transform.position;
            Vector2 dir = contactPoint.point - playerPosition;
            dir = -dir.normalized;

            rb.velocity = new Vector2(0, 0);
            rb.inertia = 0;

            // Apply the knockback force
            rb.AddForce(dir * force, forceMode);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + (Vector3)groundCheckOffset,
                        transform.position + (Vector3)groundCheckOffset + Vector3.down * groundCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
    }
}
