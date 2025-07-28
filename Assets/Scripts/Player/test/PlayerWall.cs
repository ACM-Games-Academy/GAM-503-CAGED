using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWall : MonoBehaviour
{
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(12f, 15f);
    public float wallJumpTime = 0.2f;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    public float groundCheckDistance = 0.05f;
    public float wallCheckDistance = 0.6f;
    public LayerMask groundLayer;

    public bool IsWallSliding { get; private set; }
    public bool IsGrounded { get; private set; }

    private bool isTouchingWall;
    private int wallDirection;
    private float wallJumpTimer;
    private bool isWallJumping;

    private Rigidbody2D rb;
    private PlayerMovement movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    public void HandleUpdate()
    {
        CheckCollisions();

        bool canSlide = isTouchingWall && !IsGrounded && rb.velocity.y < 0 &&
                        Mathf.Sign(movement.horizontalInput) == wallDirection &&
                        Mathf.Abs(movement.horizontalInput) > 0.1f && !isWallJumping;

        IsWallSliding = canSlide;

        if (IsGrounded || !isTouchingWall || isWallJumping)
            IsWallSliding = false;

        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0) isWallJumping = false;
        }
    }

    public void HandleWallSlideMovement()
    {
        if (IsWallSliding)
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
    }

    public void DoWallJump()
    {
        isWallJumping = true;
        wallJumpTimer = wallJumpTime;
        Vector2 jumpDir = new Vector2(-wallDirection, 1f).normalized;
        rb.velocity = new Vector2(jumpDir.x * wallJumpForce.x, wallJumpForce.y);
    }

    private void CheckCollisions()
    {
        Vector2 groundOrigin = (Vector2)transform.position + groundCheckOffset;
        IsGrounded = Physics2D.Raycast(groundOrigin, Vector2.down, groundCheckDistance, groundLayer);

        bool leftWall = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);
        bool rightWall = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        isTouchingWall = leftWall || rightWall;
        wallDirection = rightWall ? 1 : leftWall ? -1 : 0;
    }
}
