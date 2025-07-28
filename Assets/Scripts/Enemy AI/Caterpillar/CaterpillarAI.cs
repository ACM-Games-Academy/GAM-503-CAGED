using UnityEngine;

public class CaterpillarAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public Transform groundCheck;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    private bool movingRight = true;

    [Header("Combat Settings")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;
    private float damageTimer;

    [Header("Player Detection (optional)")]
    public float visionRange = 5f;
    public LayerMask visionBlockMask;
    private Transform player;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        damageTimer -= Time.deltaTime;
        Move();

        // Optional: flip to follow player if in range
        if (player != null && Vector2.Distance(transform.position, player.position) <= visionRange)
        {
            float dirToPlayer = player.position.x - transform.position.x;
            if ((dirToPlayer > 0 && !movingRight) || (dirToPlayer < 0 && movingRight))
                Flip();
        }
    }

    void Move()
    {
        Vector2 velocity = rb.velocity;
        velocity.x = movingRight ? moveSpeed : -moveSpeed;
        rb.velocity = velocity;

        // Edge check
        Vector2 checkPos = groundCheck.position + (Vector3.down * groundCheckDistance);
        bool isGroundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

        if (!isGroundAhead)
        {
            Flip(); // turn around at edge
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Wall detection — turn around
        if (collision.contacts.Length > 0 && Mathf.Abs(collision.contacts[0].normal.x) > 0.5f)
        {
            Flip();
        }

        // Player damage
        if (damageTimer <= 0)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                damageTimer = damageCooldown;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
