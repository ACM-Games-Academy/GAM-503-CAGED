using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool hasAirDashed = false;
    private bool canDash = true;
    public bool IsDashing { get; private set; }

    private float dashTimer;
    private float dashCooldownTimer;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerWall wall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        wall = GetComponent<PlayerWall>();
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (wall.IsGrounded && canDash)
            {
                StartDash();
                canDash = false;
                dashCooldownTimer = dashCooldown;
            }
            else if (!wall.IsGrounded && !hasAirDashed)
            {
                StartDash();
                hasAirDashed = true;
            }
        }

        if (IsDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                EndDash();
        }

        if (wall.IsGrounded)
        {
            if (!canDash)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer <= 0) canDash = true;
            }
            hasAirDashed = false;
        }
    }

    private void StartDash()
    {
        IsDashing = true;
        dashTimer = dashDuration;
        rb.gravityScale = 0;

        float dashDir = movement.horizontalInput != 0 ? movement.horizontalInput : movement.FacingDirection;
        rb.velocity = new Vector2(dashDir * dashForce, 0f);
    }

    private void EndDash()
    {
        IsDashing = false;
        rb.gravityScale = movement.gravityScale;
    }
}