using UnityEngine;

public class FrogAI : EnemyAIBase
{
    [Header("Leap Settings")]
    public float leapCooldown = 2f;
    private float leapTimer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    [Header("Jumping")]
    public float maxJumpTime = 1.0f;
    public float minJumpTime = 0.3f;
    public float maxLeapDistance = 10f;

    [Header("Wall Adjustment")]
    public float wallCheckDistance = 0.5f;
    public float wallAdjustmentTime = 0.5f;
    public float adjustMoveSpeed = 2f;
    private float adjustTimer = 0f;
    private bool isAdjusting = false;

    [Header("Vision")]
    public float visionRange = 6f;
    public LayerMask visionBlockMask;

    [Header("Roaming (Hops)")]
    public float roamHopForceX = 2f;
    public float roamHopForceY = 4f;
    public float roamHopCooldown = 2f;
    private float roamHopTimer;
    private bool isRoaming = false;
    private int roamDirection = 1;

    // Debug trajectory
    private Vector2 debugInitialVelocity;
    private float debugFlightTime;
    private bool debugHasTrajectory;

    protected override void HandleAI()
    {
        leapTimer -= Time.fixedDeltaTime;
        roamHopTimer -= Time.fixedDeltaTime;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Wall avoidance
        if (IsAgainstWall() && isGrounded && !isAdjusting)
        {
            isAdjusting = true;
            adjustTimer = wallAdjustmentTime;
        }

        if (isAdjusting)
        {
            adjustTimer -= Time.fixedDeltaTime;
            float direction = isFacingRight ? -1f : 1f;
            rb.velocity = new Vector2(direction * adjustMoveSpeed, rb.velocity.y);

            if (adjustTimer <= 0f)
                isAdjusting = false;

            return;
        }

        if (PlayerInSight() && isGrounded && leapTimer <= 0f)
        {
            CalculateAndLeapToPlayer();
            leapTimer = leapCooldown;
            isRoaming = false;
        }
        else if (!PlayerInSight() && isGrounded && roamHopTimer <= 0f)
        {
            RoamHop();
            roamHopTimer = roamHopCooldown;
        }

        FlipTowardsPlayer();
    }

    private void RoamHop()
    {
        // Randomly choose direction if not currently roaming
        if (!isRoaming)
        {
            roamDirection = Random.value < 0.5f ? -1 : 1;
            isRoaming = true;
        }

        // Check if about to hit wall in roam direction
        Vector2 direction = roamDirection == 1 ? Vector2.right : Vector2.left;
        if (Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer))
        {
            roamDirection *= -1; // reverse
        }

        rb.velocity = new Vector2(roamDirection * roamHopForceX, roamHopForceY);

        // Store for debug
        debugInitialVelocity = rb.velocity;
        debugFlightTime = 0.6f;
        debugHasTrajectory = true;
    }

    private void CalculateAndLeapToPlayer()
    {
        Vector2 targetPos = player.position;
        Vector2 startPos = transform.position;

        float dx = targetPos.x - startPos.x;
        float dy = targetPos.y - startPos.y;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        float t = Mathf.Clamp(Mathf.Abs(dx) / 5f, minJumpTime, maxJumpTime);

        float vx = dx / t;
        float vy = (dy + 0.5f * gravity * t * t) / t;

        rb.velocity = new Vector2(vx, vy);

        debugInitialVelocity = new Vector2(vx, vy);
        debugFlightTime = t;
        debugHasTrajectory = true;
    }

    private bool PlayerInSight()
    {
        if (Vector2.Distance(transform.position, player.position) > visionRange)
            return false;

        Vector2 dir = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, visionRange, visionBlockMask);
        if (hit.collider != null && hit.collider.transform != player)
            return false;

        return true;
    }

    private bool IsAgainstWall()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        // Jump arc
        if (debugHasTrajectory)
        {
            Vector2 pos = transform.position;
            Vector2 velocity = debugInitialVelocity;
            float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

            Gizmos.color = Color.green;
            const int resolution = 20;
            Vector2 prev = pos;

            for (int i = 1; i <= resolution; i++)
            {
                float t = i * (debugFlightTime / resolution);
                Vector2 point = pos + velocity * t + 0.5f * Physics2D.gravity * rb.gravityScale * t * t;
                Gizmos.DrawLine(prev, point);
                prev = point;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawRay(pos, debugInitialVelocity.normalized * 1.5f);
        }

        // Vision range
        Gizmos.color = new Color(1, 1, 0, 0.4f);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Wall ray
        Gizmos.color = Color.cyan;
        Vector2 wallDir = isFacingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, wallDir * wallCheckDistance);
    }
}
