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

    [Header("Damage Settings")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;
    private float damageTimer = 0f;

    protected override void Start()
    {
        base.Start();
        bobOffset = Random.Range(0f, 2f * Mathf.PI); // random phase offset
    }

    protected override void HandleAI()
    {
        damageTimer -= Time.fixedDeltaTime;
        dashCooldownTimer -= Time.fixedDeltaTime;

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            rb.velocity = dashDirection * dashSpeed;

            if (dashTimer <= 0f)
                isDashing = false;

            return;
        }

        // Avoid ground
        if (IsTooCloseToGround())
        {
            rb.velocity = new Vector2(rb.velocity.x, avoidUpForce);
            return;
        }

        // Avoid wall
        if (IsObstacleAhead(out Vector2 avoidDir))
        {
            rb.velocity = avoidDir * followSpeed;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Dash if in range
        if (distanceToPlayer <= dashTriggerDistance && dashCooldownTimer <= 0f)
        {
            StartDash();
            return;
        }

        // Float toward player with bobbing
        if (distanceToPlayer > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Apply vertical bobbing
            float bob = Mathf.Sin(Time.time * bobFrequency + bobOffset) * bobAmplitude;
            direction.y += bob;

            rb.velocity = direction.normalized * followSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        dashTimer = dashDuration;
        dashDirection = (player.position - transform.position).normalized;
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
            // Try to steer up and sideways
            avoidanceDir = Vector2.Perpendicular(direction).normalized + Vector2.up * 0.5f;
            return true;
        }

        avoidanceDir = Vector2.zero;
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (damageTimer > 0f) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            damageTimer = damageCooldown;
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
