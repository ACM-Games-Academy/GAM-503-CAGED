using UnityEngine;
using System.Collections;

public class ToadMotherAI : EnemyAIBase
{
    [Header("Leap Settings")]
    public float leapCooldown = 4f;
    private float leapTimer;

    [Header("Jumping")]
    public float highJumpArcTime = 1.5f;
    public float slamHeightBoost = 8f;
    public float maxLeapDistance = 20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    [Header("Slam Settings")]
    public float slamSpeedMultiplier = 2.5f;
    public int slamDamage = 2;

    private bool isSlamming = false;
    private bool isTrackingMidAir = false;

    private void Update()
    {
        if (isTrackingMidAir)
        {
            // Move horizontally toward player's x position mid-air
            float targetX = player.position.x;
            float currentX = transform.position.x;
            float followSpeed = 5f;

            float newX = Mathf.MoveTowards(currentX, targetX, followSpeed * Time.deltaTime);
            rb.velocity = new Vector2((newX - currentX) / Time.deltaTime, rb.velocity.y);
        }
    }

    protected override void HandleAI()
    {
        leapTimer -= Time.fixedDeltaTime;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!PlayerInRange() || !isGrounded || isSlamming) return;

        if (leapTimer <= 0f)
        {
            StartCoroutine(SlamAttack());
            leapTimer = leapCooldown;
        }

        FlipTowardsPlayer();
    }

    private IEnumerator SlamAttack()
    {
        isSlamming = true;

        Vector2 startPos = transform.position;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float t = highJumpArcTime;

        // Jump straight up with vertical boost
        float vy = (slamHeightBoost + 0.5f * gravity * t * t) / t;

        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(0f, vy), ForceMode2D.Impulse);

        // Start mid-air horizontal tracking
        isTrackingMidAir = true;

        // Wait until peak, then stop tracking and slam down
        yield return new WaitUntil(() => rb.velocity.y < 0);

        isTrackingMidAir = false;

        rb.velocity = new Vector2(0f, -Mathf.Abs(slamSpeedMultiplier * 10f));

        // Wait until grounded
        yield return new WaitUntil(() => Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer));

        isSlamming = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isSlamming) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(slamDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
