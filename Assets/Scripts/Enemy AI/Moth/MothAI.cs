using UnityEngine;

public class MothAI : EnemyAIBase
{
    [Header("Flight Movement")]
    public float followSpeed = 2f;
    public float stoppingDistance = 1.5f;

    [Header("Bob Settings")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 2f;
    private float bobOffset;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashCooldown = 3f;
    public float dashDuration = 0.3f;
    public float dashTriggerDistance = 4f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private float dashTimer = 0f;
    private Vector2 dashDirection;

    [Header("Avoidance")]
    public float groundAvoidDistance = 1f;
    public float obstacleAvoidDistance = 1f;
    public LayerMask groundMask;
    public float avoidUpForce = 2f;
    public float avoidanceLerpSpeed = 5f;

    [Header("Damage Settings")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;
    private float damageTimer = 0f;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f;
    public float knockbackForce = 8f;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    protected override void Start()
    {
        base.Start();
        bobOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    protected override void HandleAI()
    {
        damageTimer -= Time.fixedDeltaTime;

        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
                isKnockedBack = false;

            return;
        }

        dashCooldownTimer -= Time.fixedDeltaTime;

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.velocity = dashDirection * dashSpeed;

            if (dashTimer <= 0f)
                EndDash();

            return;
        }

        if (IsTooCloseToGround())
        {
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(rb.velocity.x, avoidUpForce), Time.fixedDeltaTime * avoidanceLerpSpeed);
            return;
        }

        if (IsObstacleAhead(out Vector2 avoidDir))
        {
            rb.velocity = Vector2.Lerp(rb.velocity, avoidDir * followSpeed, Time.fixedDeltaTime * avoidanceLerpSpeed);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= dashTriggerDistance && dashCooldownTimer <= 0f)
        {
            StartDash();
            return;
        }

        if (distanceToPlayer > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            float bob = Mathf.Sin(Time.time * bobFrequency + bobOffset) * bobAmplitude;
            direction.y += bob;
            rb.velocity = direction.normalized * followSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        HandleStuckUnstick(); // Detect & escape if stuck on platform sides
    }

    private void StartDash()
    {
        Vector2 dir = (player.position - transform.position).normalized;

        // Optional: cancel dash if wall is immediately ahead
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 0.5f, groundMask);
        if (hit.collider != null)
            return;

        isDashing = true;
        dashCooldownTimer = dashCooldown;
        dashTimer = dashDuration;
        dashDirection = dir;
    }

    private void EndDash()
    {
        isDashing = false;

        // Push slightly away from wall if right up against one
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, 0.3f, groundMask);
        if (hit.collider != null)
        {
            rb.position -= dashDirection * 0.2f;
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = force;
    }

    private bool IsTooCloseToGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundAvoidDistance, groundMask);
        return hit.collider != null;
    }

    private bool IsObstacleAhead(out Vector2 avoidanceDir)
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleAvoidDistance, groundMask);

        if (hit.collider != null)
        {
            Vector2 perpendicular = Vector2.Perpendicular(direction);
            Vector2 upwardBias = Vector2.up * 0.6f;
            avoidanceDir = (perpendicular + upwardBias).normalized;
            return true;
        }

        avoidanceDir = Vector2.zero;
        return false;
    }

    private void HandleStuckUnstick()
    {
        if (!isDashing && rb.velocity.magnitude < 0.1f)
        {
            RaycastHit2D rightWall = Physics2D.Raycast(transform.position, Vector2.right, 0.3f, groundMask);
            RaycastHit2D leftWall = Physics2D.Raycast(transform.position, Vector2.left, 0.3f, groundMask);

            if (rightWall.collider != null)
            {
                rb.velocity = new Vector2(-1f, 1f).normalized * 2f;
            }
            else if (leftWall.collider != null)
            {
                rb.velocity = new Vector2(1f, 1f).normalized * 2f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageTimer > 0f) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            damageTimer = damageCooldown;

            if (playerController != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                direction.y = Mathf.Abs(direction.y); // favor upward knockback
                Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
                playerRb.velocity = Vector2.zero;
                playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashTriggerDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundAvoidDistance);

        Gizmos.color = Color.cyan;
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * obstacleAvoidDistance);
        }
    }
}
