using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 15f;
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    private Rigidbody2D rb;
    private PlayerWall wall;
    private bool jumpPressed;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wall = GetComponent<PlayerWall>();
    }

    public void HandleUpdate()
    {
        coyoteTimer = wall.IsGrounded ? coyoteTime : coyoteTimer - Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || wall.IsWallSliding))
        {
            jumpPressed = true;
            jumpBufferTimer = 0;
        }
    }

    public void HandleFixedUpdate()
    {
        if (!jumpPressed) return;

        if (wall.IsWallSliding)
        {
            wall.DoWallJump();
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        jumpPressed = false;
        coyoteTimer = 0;
    }
}
